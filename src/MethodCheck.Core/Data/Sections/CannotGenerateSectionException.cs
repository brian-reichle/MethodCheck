// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;

namespace MethodCheck.Core.Data.Sections
{
	public sealed class CannotGenerateSectionException : Exception
	{
		public CannotGenerateSectionException()
		{
		}

		public CannotGenerateSectionException(string message)
			: base(message)
		{
		}

		public CannotGenerateSectionException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
