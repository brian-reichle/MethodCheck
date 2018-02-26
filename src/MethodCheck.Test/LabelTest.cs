// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using MethodCheck.Data;
using NUnit.Framework;

namespace MethodCheck.Test
{
	[TestFixture]
	class LabelTest
	{
		[TestCase(0, ExpectedResult = "IL_0000")]
		[TestCase(10, ExpectedResult = "IL_000A")]
		[TestCase(32, ExpectedResult = "IL_0020")]
		public string Format(int offset) => new Label(offset).ToString();
	}
}
