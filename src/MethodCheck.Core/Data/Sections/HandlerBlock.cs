// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Reflection;

namespace MethodCheck.Core.Data
{
	public sealed class HandlerBlock
	{
		public HandlerBlock(ExceptionHandlingClauseOptions type, MetadataToken exceptionType, BaseSection handlerSection)
		{
			Type = type;
			ExceptionType = exceptionType;
			HandlerSection = handlerSection;
		}

		public HandlerBlock(ExceptionHandlingClauseOptions type, BaseSection filterSection, BaseSection handlerSection)
		{
			Type = type;
			FilterSection = filterSection;
			HandlerSection = handlerSection;
		}

		public ExceptionHandlingClauseOptions Type { get; }
		public BaseSection? FilterSection { get; }
		public MetadataToken ExceptionType { get; }
		public BaseSection HandlerSection { get; }
	}
}
