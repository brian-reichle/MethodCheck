// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Collections.Immutable;

namespace MethodCheck.Core.Data.Sections
{
	public sealed class SequenceSection(ILRange range, ImmutableArray<BaseSection> sections) : BaseSection(range)
	{
		public ImmutableArray<BaseSection> Sections { get; } = sections;
	}
}
