// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;

namespace MethodCheck.Core;

public static class BinaryProcessor
{
	public static byte[]? Parse(ReadOnlySpan<char> text)
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

			if ((uint)(c - '0') <= 9)
			{
				x = unchecked((byte)(c - '0'));
			}
			else
			{
				c |= '\x20';

				if ((uint)(c - 'a') <= 5)
				{
					x = unchecked((byte)(c - 'a' + 10));
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
		if (blob.Length == 0)
		{
			return string.Empty;
		}

		// 2 characters per byte +
		// one separator character per byte after the first +
		// one additional separator character every 4 bytes after the first.
		//
		// After every 16 bytes, the separator will be a newline, otherwise it will be spaces.
		var length = 2 + (((blob.Length - 1) * 13) >> 2);

		if (Environment.NewLine.Length != 2)
		{
			// We calculated length based on Environment.NewLine being 2 characters.
			// We need to adjust if it wasn't for some reason.
			length += (Environment.NewLine.Length - 2) * ((blob.Length - 1) >> 4);
		}

		return string.Create(
			length,
			blob,
			static (target, blob) =>
			{
				var write = 0;
				var nl = Environment.NewLine.AsSpan();

				WriteByte(target.Slice(write), blob[0]);
				write += 2;

				for (var i = 1; i < blob.Length; i++)
				{
					if ((i & 0xF) == 0)
					{
						nl.CopyTo(target.Slice(write));
						write += nl.Length;
					}
					else
					{
						if ((i & 0x3) == 0)
						{
							target[write++] = ' ';
						}

						target[write++] = ' ';
					}

					WriteByte(target.Slice(write), blob[i]);
					write += 2;
				}
			});

		static void WriteByte(Span<char> target, byte b)
		{
			const string Alphabet = "0123456789ABCDEF";
			target[0] = Alphabet[b >> 4];
			target[1] = Alphabet[b & 0xF];
		}
	}
}
