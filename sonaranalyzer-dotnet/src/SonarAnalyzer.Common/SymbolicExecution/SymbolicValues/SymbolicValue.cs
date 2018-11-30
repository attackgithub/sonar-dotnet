/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.SymbolicValues;

namespace SonarAnalyzer.SymbolicExecution
{
    public class SymbolicValue
    {
        public static readonly SymbolicValue True = new BoolLiteralSymbolicValue(true);
        public static readonly SymbolicValue False = new BoolLiteralSymbolicValue(false);
        public static readonly SymbolicValue Null = new NullSymbolicValue();
        public static readonly SymbolicValue This = new ThisSymbolicValue();
        public static readonly SymbolicValue Base = new BaseSymbolicValue();

        private class BoolLiteralSymbolicValue : SymbolicValue
        {
            internal BoolLiteralSymbolicValue(bool value)
                : base(value)
            {
            }
        }

        private class ThisSymbolicValue : SymbolicValue
        {
            internal ThisSymbolicValue()
                : base(new object())
            {
            }

            public override string ToString()
            {
                return "SV_THIS";
            }
        }

        private class BaseSymbolicValue : SymbolicValue
        {
            internal BaseSymbolicValue()
                : base(new object())
            {
            }

            public override string ToString()
            {
                return "SV_BASE";
            }
        }

        private class NullSymbolicValue : SymbolicValue
        {
            internal NullSymbolicValue()
                : base(new object())
            {
            }

            public override string ToString()
            {
                return "SV_NULL";
            }
        }

        protected readonly object identifier;

        private static int SymbolicValueCounter;

        public SymbolicValue()
            : this(SymbolicValueCounter++)
        {
        }

        private SymbolicValue(object identifier)
        {
            this.identifier = identifier;
        }

        internal static SymbolicValue Create(ITypeSymbol type = null)
        {
            if (type != null &&
                type.OriginalDefinition.Is(KnownType.System_Nullable_T))
            {
                return new NullableSymbolicValue(new SymbolicValue());
            }

            return new SymbolicValue();
        }

        public override string ToString()
        {
            if (this.identifier != null)
            {
                return "SV_" + this.identifier;
            }

            return base.ToString();
        }

        public bool TryGetConstraints(ProgramState programState, out SymbolicValueConstraints constraints)
        {
            return programState.Constraints.TryGetValue(this, out constraints);
        }

        public bool IsNull(ProgramState programState)
        {
            return programState.HasConstraint(this, ObjectConstraint.Null);
        }

        protected ImmutableArray<ProgramState> ThrowIfTooMany(IEnumerable<ProgramState> states)
        {
            var stateList = states.ToImmutableArray();
            if (stateList.Length >= AbstractExplodedGraph.MaxInternalStateCount)
            {
                throw new TooManyInternalStatesException();
            }

            return stateList;
        }

        public virtual ImmutableArray<ProgramState> TrySetConstraint(SymbolicValueConstraint constraint,
            ProgramState programState)
        {
            if (constraint == null)
            {
                return ImmutableArray.Create(programState);
            }

            if (!programState.Constraints.TryGetValue(this, out var oldConstraints))
            {
                return ImmutableArray.Create(programState.SetConstraint(this, constraint));
            }

            if (constraint is BoolConstraint boolConstraint)
            {
                return TrySetBoolConstraint(boolConstraint, oldConstraints, programState);
            }

            if (constraint is ObjectConstraint objectConstraint)
            {
                return TrySetObjectConstraint(objectConstraint, oldConstraints, programState);
            }

            if (constraint is NullableValueConstraint ||
                constraint is DisposableConstraint ||
                constraint is CollectionCapacityConstraint)
            {
                return ImmutableArray.Create(programState);
            }

            throw new NotSupportedException($"Neither one of {nameof(BoolConstraint)}, {nameof(ObjectConstraint)}, " +
                $"{nameof(ObjectConstraint)}, {nameof(DisposableConstraint)}, {nameof(CollectionCapacityConstraint)}.");
        }

        public virtual ImmutableArray<ProgramState> TrySetOppositeConstraint(SymbolicValueConstraint constraint,
            ProgramState programState)
        {
            return TrySetConstraint(constraint?.OppositeForLogicalNot, programState);
        }

        public ImmutableArray<ProgramState> TrySetConstraints(SymbolicValueConstraints constraints,
            ProgramState programState)
        {
            return TrySetConstraints(constraints, programState, false);
        }

        public ImmutableArray<ProgramState> TrySetOppositeConstraints(SymbolicValueConstraints constraints,
            ProgramState programState)
        {
            return TrySetConstraints(constraints, programState, true);
        }

        private ImmutableArray<ProgramState> TrySetConstraints(SymbolicValueConstraints constraints,
            ProgramState programState, bool isOppositeConstraints)
        {
            var programStates = ImmutableArray.Create(programState);

            if (constraints == null)
            {
                return programStates;
            }

            foreach (var constraint in constraints.GetConstraints())
            {
                programStates = programStates.SelectMany(ps =>
                    isOppositeConstraints
                        ? TrySetOppositeConstraint(constraint, ps)
                        : TrySetConstraint(constraint, ps))
                    .ToImmutableArray();
            }

            return programStates;
        }

        private ImmutableArray<ProgramState> TrySetBoolConstraint(BoolConstraint constraint,
            SymbolicValueConstraints oldConstraints, ProgramState programState)
        {
            if (oldConstraints.HasConstraint(ObjectConstraint.Null))
            {
                // It was null, and now it should be true or false
                return ImmutableArray<ProgramState>.Empty;
            }

            var oldBoolConstraint = oldConstraints.GetConstraintOrDefault<BoolConstraint>();
            if (oldBoolConstraint != null &&
                oldBoolConstraint != constraint)
            {
                return ImmutableArray<ProgramState>.Empty;
            }

            // Either same bool constraint, or previously not null, and now a bool constraint
            return ImmutableArray.Create(programState.SetConstraint(this, constraint));
        }

        private ImmutableArray<ProgramState> TrySetObjectConstraint(ObjectConstraint constraint,
            SymbolicValueConstraints oldConstraints, ProgramState programState)
        {
            if (oldConstraints.HasConstraint<BoolConstraint>())
            {
                if (constraint == ObjectConstraint.Null)
                {
                    return ImmutableArray<ProgramState>.Empty;
                }

                return ImmutableArray.Create(programState);
            }

            var oldObjectConstraint = oldConstraints.GetConstraintOrDefault<ObjectConstraint>();
            if (oldObjectConstraint != null)
            {
                if (oldObjectConstraint != constraint)
                {
                    return ImmutableArray<ProgramState>.Empty;
                }

                return ImmutableArray.Create(programState.SetConstraint(this, constraint));
            }

            throw new NotSupportedException($"Neither {nameof(BoolConstraint)}, nor {nameof(ObjectConstraint)}");
        }
    }
}
