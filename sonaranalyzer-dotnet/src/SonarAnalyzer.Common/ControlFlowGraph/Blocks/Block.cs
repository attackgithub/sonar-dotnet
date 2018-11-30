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

namespace SonarAnalyzer.ControlFlowGraph
{
    /// <summary>
    /// Basic building blocks of a Control Flow Graph (<see cref="IControlFlowGraph"/>).
    /// Holds a list of instructions which have no jumps between them.
    /// </summary>
    public class Block
    {
        private readonly Lazy<ImmutableArray<SyntaxNode>> instructions;
        private readonly Lazy<ImmutableArray<Block>> predecessorBlocks;
        private readonly Lazy<ISet<Block>> allSuccessors;
        private readonly Lazy<ISet<Block>> allPredecessors;

        // Protected to allow extending and mocking
        protected Block()
        {
            this.instructions = new Lazy<ImmutableArray<SyntaxNode>>(() => ReversedInstructions.Reverse().ToImmutableArray());
            this.predecessorBlocks = new Lazy<ImmutableArray<Block>>(() => EditablePredecessorBlocks.ToImmutableArray());
            this.allSuccessors = new Lazy<ISet<Block>>(() => GetAll(this, b => b.SuccessorBlocks));
            this.allPredecessors = new Lazy<ISet<Block>>(() => GetAll(this, b => b.PredecessorBlocks));
        }

        public virtual ImmutableArray<SyntaxNode> Instructions => this.instructions.Value;

        public virtual ImmutableArray<Block> PredecessorBlocks => this.predecessorBlocks.Value;

        public virtual ImmutableArray<Block> SuccessorBlocks { get; } = ImmutableArray.Create<Block>();

        internal IList<SyntaxNode> ReversedInstructions { get; } = new List<SyntaxNode>();

        internal ISet<Block> EditablePredecessorBlocks { get; } = new HashSet<Block>();

        internal virtual Block GetPossibleNonEmptySuccessorBlock()
        {
            return this;
        }

        internal virtual void ReplaceSuccessors(Dictionary<Block, Block> replacementMapping)
        {
        }

        public ISet<Block> AllSuccessorBlocks => this.allSuccessors.Value;

        public ISet<Block> AllPredecessorBlocks => this.allPredecessors.Value;

        private static ISet<Block> GetAll(Block initial, Func<Block, IEnumerable<Block>> getNexts)
        {
            var alreadyProcesses = new HashSet<Block>();

            var toProcess = new Queue<Block>(getNexts(initial));
            while (toProcess.Count != 0)
            {
                var current = toProcess.Dequeue();
                if (alreadyProcesses.Contains(current))
                {
                    continue;
                }

                alreadyProcesses.Add(current);

                foreach (var item in getNexts(current))
                {
                    toProcess.Enqueue(item);
                }
            }

            return alreadyProcesses;
        }
    }
}
