// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
namespace MethodCheck.Core.Data
{
	sealed class IncompleteArgument
	{
		public static IncompleteArgument Value { get; } = new IncompleteArgument();

		IncompleteArgument()
		{
		}

		public override string ToString() => "??";
	}
}
