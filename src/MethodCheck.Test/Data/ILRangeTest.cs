// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using MethodCheck.Core.Data;
using NUnit.Framework;

namespace MethodCheck.Test
{
	[TestFixture]
	class ILRangeTest
	{
		[TestCase(10, 1, ExpectedResult = "IL_000A (1)")]
		[TestCase(1, 10, ExpectedResult = "IL_0001 (10)")]
		public string Format(int start, int length) => new ILRange(start, length).ToString();

		[TestCase(5, 10, ExpectedResult = true, TestName = "{m}_Exact")]
		[TestCase(5, 1, ExpectedResult = true, TestName = "{m}_InsideStart")]
		[TestCase(14, 1, ExpectedResult = true, TestName = "{m}_InsideEnd")]
		[TestCase(4, 2, ExpectedResult = false, TestName = "{m}_OverlapStart")]
		[TestCase(14, 2, ExpectedResult = false, TestName = "{m}_OverlapEnd")]
		[TestCase(4, 1, ExpectedResult = false, TestName = "{m}_Prologue")]
		[TestCase(15, 1, ExpectedResult = false, TestName = "{m}_Epilogue")]
		public bool Contains(int start, int length) => new ILRange(5, 10).Contains(new ILRange(start, length));

		[TestCase(5, 10, ExpectedResult = true, TestName = "{m}_Exact")]
		[TestCase(5, 1, ExpectedResult = true, TestName = "{m}_InsideStart")]
		[TestCase(14, 1, ExpectedResult = true, TestName = "{m}_InsideEnd")]
		[TestCase(4, 2, ExpectedResult = true, TestName = "{m}_OverlapStart")]
		[TestCase(14, 2, ExpectedResult = true, TestName = "{m}_OverlapEnd")]
		[TestCase(4, 1, ExpectedResult = false, TestName = "{m}_Prologue")]
		[TestCase(15, 1, ExpectedResult = false, TestName = "{m}_Epilogue")]
		public bool Overlaps(int start, int length) => new ILRange(5, 10).Overlaps(new ILRange(start, length));
	}
}
