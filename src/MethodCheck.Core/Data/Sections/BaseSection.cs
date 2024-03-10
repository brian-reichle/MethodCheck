// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
namespace MethodCheck.Core.Data
{
	public abstract class BaseSection(ILRange range)
	{
		public ILRange Range { get; } = range;
	}
}
