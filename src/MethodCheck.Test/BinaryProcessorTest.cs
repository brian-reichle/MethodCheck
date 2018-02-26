// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using NUnit.Framework;

namespace MethodCheck.Test
{
	[TestFixture]
	class BinaryProcessorTest
	{
		[Test]
		public void ParseEmpty()
		{
			var blob = BinaryProcessor.Parse(string.Empty);
			Assert.That(blob, Is.Empty);
		}

		[Test]
		public void Parse()
		{
			var blob = BinaryProcessor.Parse("42 51 0A // 05\r\n\r\n54");
			Assert.That(blob, Is.EqualTo(new byte[] { 0x42, 0x51, 0x0A, 0x54 }));
		}

		[Test]
		public void ParseHalfByte()
		{
			var blob = BinaryProcessor.Parse("4");
			Assert.That(blob, Is.EqualTo(new byte[] { 0x40 }));
		}

		[Test]
		public void ParseComments()
		{
			var blob = BinaryProcessor.Parse("42 51 0A\r\n\r\n54");
			Assert.That(blob, Is.EqualTo(new byte[] { 0x42, 0x51, 0x0A, 0x54 }));
		}

		[Test]
		public void FormatEmpty()
		{
			Assert.That(BinaryProcessor.Format(Array.Empty<byte>()), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Format()
		{
			const string Expected =
@"00 01 02 03  04 05 06 07  08 09 0A 0B  0C 0D 0E 0F
10 11 12 13  14 15 16 17  18 19 1A 1B  1C 1D 1E 1F
20";

			var blob = new byte[]
			{
0x00, 0x01, 0x02, 0x03,  0x04, 0x05, 0x06, 0x07,  0x08, 0x09, 0x0A, 0x0B,  0x0C, 0x0D, 0x0E, 0x0F,
0x10, 0x11, 0x12, 0x13,  0x14, 0x15, 0x16, 0x17,  0x18, 0x19, 0x1A, 0x1B,  0x1C, 0x1D, 0x1E, 0x1F,
0x20
			};

			Assert.That(BinaryProcessor.Format(blob), Is.EqualTo(Expected));
		}
	}
}
