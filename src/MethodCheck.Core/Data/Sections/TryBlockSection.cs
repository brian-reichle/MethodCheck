// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Collections.Immutable;

namespace MethodCheck.Core.Data
{
	public sealed class TryBlockSection : BaseSection
	{
		public TryBlockSection(ILRange range, BaseSection tryBlock, ImmutableArray<HandlerBlock> handlerBlocks)
			: base(range)
		{
			TryBlock = tryBlock;
			HandlerBlocks = handlerBlocks;
		}

		public BaseSection TryBlock { get; }
		public ImmutableArray<HandlerBlock> HandlerBlocks { get; }
	}
}
