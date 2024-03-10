// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Reflection;

namespace MethodCheck.Core.Data
{
	public sealed class ExceptionHandler(
		ExceptionHandlingClauseOptions type,
		ILRange tryRange,
		ILRange handlerRange,
		int filterOrType)
	{
		public ExceptionHandlingClauseOptions Type { get; } = type;
		public ILRange TryRange { get; } = tryRange;
		public ILRange HandlerRange { get; } = handlerRange;
		public int FilterOrType { get; } = filterOrType;
	}
}
