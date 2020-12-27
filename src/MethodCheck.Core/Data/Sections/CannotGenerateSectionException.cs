// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Runtime.Serialization;

namespace MethodCheck.Core.Data.Sections
{
	[Serializable]
	public class CannotGenerateSectionException : Exception
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

		protected CannotGenerateSectionException(SerializationInfo serializationInfo, StreamingContext streamingContext)
			: base(serializationInfo, streamingContext)
		{
		}
	}
}
