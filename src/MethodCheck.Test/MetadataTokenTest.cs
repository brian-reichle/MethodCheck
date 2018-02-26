// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using MethodCheck.Data;
using NUnit.Framework;

namespace MethodCheck.Test
{
	[TestFixture]
	class MetadataTokenTest
	{
		[TestCase(0, ExpectedResult = "00000000")]
		[TestCase(0x0A000042, ExpectedResult = "0A000042")]
		public string Format(int value) => new MetadataToken(value).ToString();
	}
}
