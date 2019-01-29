// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MethodCheck
{
	public static class BinaryProcessor
	{
		public static byte[] Parse(ReadOnlySpan<char> text)
		{
			var index = 0;
			var buffer = new List<byte>();
			var halfByte = false;
			var readingComment = false;
			byte tmp = 0;

			while (index < text.Length)
			{
				var c = text[index++];
				byte x;

				if (c == '\r' || c == '\n')
				{
					readingComment = false;
					continue;
				}

				if (char.IsWhiteSpace(c) || readingComment)
				{
					continue;
				}
				else if (c >= '0' && c <= '9')
				{
					x = unchecked((byte)(c - '0'));
				}
				else if (c >= 'a' && c <= 'f')
				{
					x = unchecked((byte)(c - 'a' + 10));
				}
				else if (c >= 'A' && c <= 'F')
				{
					x = unchecked((byte)(c - 'A' + 10));
				}
				else if (c == '/' && index < text.Length && text[index] == '/')
				{
					readingComment = true;
					index++;
					continue;
				}
				else
				{
					return null;
				}

				if (halfByte)
				{
					buffer.Add(unchecked((byte)(tmp | x)));
					halfByte = false;
				}
				else
				{
					tmp = unchecked((byte)(x << 4));
					halfByte = true;
				}
			}

			if (halfByte)
			{
				buffer.Add(tmp);
			}

			return buffer.ToArray();
		}

		public static string Format(ReadOnlySpan<byte> blob)
		{
			var builder = new StringBuilder();

			for (var i = 0; i < blob.Length; i++)
			{
				if (i > 0)
				{
					if ((i & 0xF) == 0)
					{
						builder.AppendLine();
					}
					else if ((i & 0x3) == 0)
					{
						builder.Append("  ");
					}
					else
					{
						builder.Append(' ');
					}
				}

				builder.Append(blob[i].ToString("X2", CultureInfo.InvariantCulture));
			}

			return builder.ToString();
		}
	}
}
