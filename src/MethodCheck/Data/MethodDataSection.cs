// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;

namespace MethodCheck.Data
{
	sealed class MethodDataSection
	{
		public MethodDataSection(IEnumerable<ExceptionHandler> exceptionHandlers)
		{
			if (exceptionHandlers == null) throw new ArgumentNullException(nameof(exceptionHandlers));

			ExceptionHandlers = exceptionHandlers;
		}

		public IEnumerable<ExceptionHandler> ExceptionHandlers { get; }
	}
}
