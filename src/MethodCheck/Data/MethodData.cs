// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;

namespace MethodCheck.Data
{
	sealed class MethodData
	{
		public MethodData(
			MetadataToken localsToken,
			int maxStack,
			int codeSize,
			InstructionSequence instructions,
			IReadOnlyList<MethodDataSection> dataSections)
		{
			if (instructions == null) throw new ArgumentNullException(nameof(instructions));
			if (dataSections == null) throw new ArgumentNullException(nameof(dataSections));

			LocalsToken = localsToken;
			MaxStack = maxStack;
			CodeSize = codeSize;
			Instructions = instructions;
			DataSections = dataSections;
		}

		public MetadataToken LocalsToken { get; }
		public int MaxStack { get; }
		public int CodeSize { get; }
		public InstructionSequence Instructions { get; }
		public IReadOnlyList<MethodDataSection> DataSections { get; }
	}
}
