// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Collections.Immutable;

namespace MethodCheck.Core.Data
{
	public sealed class MethodData(
		MetadataToken localsToken,
		int maxStack,
		int codeSize,
		MethodDataFlags flags,
		ImmutableArray<Instruction> instructions,
		ImmutableArray<MethodDataSection> dataSections)
	{
		public MetadataToken LocalsToken { get; } = localsToken;
		public int MaxStack { get; } = maxStack;
		public int CodeSize { get; } = codeSize;
		public MethodDataFlags Flags { get; } = flags;
		public ImmutableArray<Instruction> Instructions { get; } = instructions;
		public ImmutableArray<MethodDataSection> DataSections { get; } = dataSections;
	}
}
