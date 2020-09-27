// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Reflection;

namespace MethodCheck.Core.Data
{
	public sealed class ExceptionHandler
	{
		public ExceptionHandler(ExceptionHandlingClauseOptions type, ILRange tryRange, ILRange handlerRange, int filterOrType)
		{
			Type = type;
			TryRange = tryRange;
			HandlerRange = handlerRange;
			FilterOrType = filterOrType;
		}

		public ExceptionHandlingClauseOptions Type { get; }
		public ILRange TryRange { get; }
		public ILRange HandlerRange { get; }
		public int FilterOrType { get; }
	}
}
