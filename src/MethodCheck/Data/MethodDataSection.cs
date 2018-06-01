// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Collections.Immutable;

namespace MethodCheck.Data
{
	sealed class MethodDataSection
	{
		public MethodDataSection(ImmutableArray<ExceptionHandler> exceptionHandlers)
		{
			ExceptionHandlers = exceptionHandlers;
		}

		public ImmutableArray<ExceptionHandler> ExceptionHandlers { get; }
	}
}
