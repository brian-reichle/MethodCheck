// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Reflection;

namespace MethodCheck.Data
{
	sealed class ExceptionHandler
	{
		public ExceptionHandler(ExceptionHandlingClauseOptions type, Range tryRange, Range handlerRange, int filterOrType)
		{
			Type = type;
			TryRange = tryRange;
			HandlerRange = handlerRange;
			FilterOrType = filterOrType;
		}

		public ExceptionHandlingClauseOptions Type { get; }
		public Range TryRange { get; }
		public Range HandlerRange { get; }
		public int FilterOrType { get; }
	}
}
