// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Collections.Immutable;

namespace MethodCheck.Core.Data
{
	public sealed class MethodDataSection(ImmutableArray<ExceptionHandler> exceptionHandlers)
	{
		public ImmutableArray<ExceptionHandler> ExceptionHandlers { get; } = exceptionHandlers;
	}
}
