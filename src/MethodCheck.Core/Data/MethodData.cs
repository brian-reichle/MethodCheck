// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Collections.Immutable;

namespace MethodCheck.Core.Data
{
	public sealed class MethodData
	{
		public MethodData(
			MetadataToken localsToken,
			int maxStack,
			int codeSize,
			ImmutableArray<Instruction> instructions,
			ImmutableArray<MethodDataSection> dataSections)
		{
			LocalsToken = localsToken;
			MaxStack = maxStack;
			CodeSize = codeSize;
			Instructions = instructions;
			DataSections = dataSections;
		}

		public MetadataToken LocalsToken { get; }
		public int MaxStack { get; }
		public int CodeSize { get; }
		public ImmutableArray<Instruction> Instructions { get; }
		public ImmutableArray<MethodDataSection> DataSections { get; }
	}
}
