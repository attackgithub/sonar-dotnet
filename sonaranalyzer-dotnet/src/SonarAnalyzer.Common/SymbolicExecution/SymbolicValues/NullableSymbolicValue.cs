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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.SymbolicValues
{
    public class NullableSymbolicValue : SymbolicValue
    {
        public SymbolicValue WrappedValue { get; }

        public NullableSymbolicValue(SymbolicValue wrappedValue)
        {
            WrappedValue = wrappedValue;
        }

        public override ImmutableArray<ProgramState> TrySetConstraint(SymbolicValueConstraint constraint,
            ProgramState programState)
        {
            if (constraint == null)
            {
                return ImmutableArray.Create(programState);
            }

            if (constraint is ObjectConstraint)
            {
                var optionalConstraint = constraint == ObjectConstraint.Null
                    ? NullableValueConstraint.NoValue
                    : NullableValueConstraint.HasValue;

                return TrySetConstraint(optionalConstraint, programState);
            }

            var oldConstraint = programState.Constraints.GetValueOrDefault(this)?
                .GetConstraintOrDefault<NullableValueConstraint>();
            if (constraint is NullableValueConstraint)
            {
                if (oldConstraint == null)
                {
                    return ImmutableArray.Create(programState.SetConstraint(this, constraint));
                }

                if (oldConstraint != constraint)
                {
                    return ImmutableArray<ProgramState>.Empty;
                }

                return ImmutableArray.Create(programState);
            }

            return TrySetConstraint(NullableValueConstraint.HasValue, programState)
                .SelectMany(ps => WrappedValue.TrySetConstraint(constraint, ps))
                .ToImmutableArray();
        }

        public override ImmutableArray<ProgramState> TrySetOppositeConstraint(SymbolicValueConstraint constraint, ProgramState programState)
        {
            var negateConstraint = constraint?.OppositeForLogicalNot;

            if (constraint is BoolConstraint)
            {
                return TrySetConstraint(negateConstraint, programState)
                  .Union(TrySetConstraint(NullableValueConstraint.NoValue, programState))
                  .ToImmutableArray();
            }

            return TrySetConstraint(negateConstraint, programState);
        }

        public override string ToString()
        {
            if (base.identifier != null)
            {
                return $"NULLABLE_SV_{base.identifier}";
            }

            return WrappedValue?.ToString() ?? base.ToString();
        }
    }
}
