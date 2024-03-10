// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Collections.Immutable;

namespace MethodCheck.Core.Data
{
	public sealed class TryBlockSection(
		ILRange range,
		BaseSection tryBlock,
		ImmutableArray<HandlerBlock> handlerBlocks)
		: BaseSection(range)
	{
		public BaseSection TryBlock { get; } = tryBlock;
		public ImmutableArray<HandlerBlock> HandlerBlocks { get; } = handlerBlocks;
	}
}
