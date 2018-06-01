// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Immutable;
using System.Reflection;
using MethodCheck.Data;

namespace MethodCheck.Parsing
{
	static class MethodParser
	{
		public static MethodData ParseBody(byte[] blob)
		{
			if (blob.Length == 0)
			{
				return null;
			}

			var type = blob[0] & 0x03;

			try
			{
				if (type == CorILMethod_TinyFormat)
				{
					return ParseTiny(blob);
				}
				else if (type == CorILMethod_FatFormat)
				{
					return ParseFat(blob);
				}
			}
			catch (IndexOutOfRangeException)
			{
			}
			catch (ArgumentException)
			{
			}

			return null;
		}

		public static MethodData ParseIL(byte[] blob)
		{
			return new MethodData(
				default,
				0,
				0,
				ReadIL(blob, 0, blob.Length),
				ImmutableArray<MethodDataSection>.Empty);
		}

		static MethodData ParseTiny(byte[] blob)
		{
			var codeSize = blob[0] >> 2;

			return new MethodData(
				default,
				8,
				codeSize,
				ReadIL(blob, 1, codeSize),
				ImmutableArray<MethodDataSection>.Empty);
		}

		static MethodData ParseFat(byte[] blob)
		{
			if (blob.Length < 12) return null;
			var headerLength = blob[1] >> 2 & 0x3C;
			var codeSize = BitConverter.ToInt32(blob, 4);

			ImmutableArray<MethodDataSection> dataSections;

			if ((blob[0] & CorILMethod_MoreSects) != 0)
			{
				dataSections = ExtractDataSections(blob, headerLength, codeSize);
			}
			else
			{
				dataSections = ImmutableArray<MethodDataSection>.Empty;
			}

			return new MethodData(
				BitConverter.ToInt32(blob, 8),
				BitConverter.ToUInt16(blob, 2),
				codeSize,
				ReadIL(blob, headerLength, codeSize),
				dataSections);
		}

		static ImmutableArray<Instruction> ReadIL(byte[] blob, int start, int length)
		{
			if (start >= blob.Length)
			{
				return ImmutableArray<Instruction>.Empty;
			}

			try
			{
				using (var reader = new ILReader(blob, start, length))
				{
					return reader.ToImmutableArray();
				}
			}
			catch (ILException)
			{
				return ImmutableArray<Instruction>.Empty;
			}
		}

		static ImmutableArray<MethodDataSection> ExtractDataSections(byte[] blob, int headerLength, int codeSize)
		{
			var builder = ImmutableArray.CreateBuilder<MethodDataSection>();
			ReadDataSection(builder, blob, (headerLength + codeSize + 3) & ~3);
			return builder.ToImmutable();
		}

		static void ReadDataSection(ImmutableArray<MethodDataSection>.Builder builder, byte[] blob, int start)
		{
			switch (blob[start] & 0x7F)
			{
				case CorILMethod_Sect_EHTable:
					ReadSmallExceptionSection(builder, blob, start);
					return;

				case CorILMethod_Sect_EHTable | CorILMethod_Sect_FatFormat:
					ReadFatExecptionSection(builder, blob, start);
					return;
			}
		}

		static void ReadSmallExceptionSection(ImmutableArray<MethodDataSection>.Builder builder, byte[] blob, int start)
		{
			var flags = blob[start];
			var dataSize = blob[start + 1];
			var n = (dataSize - 4) / 12;

			var handlers = ImmutableArray.CreateBuilder<ExceptionHandler>(n);
			start += 4;

			for (var i = 0; i < n; i++)
			{
				handlers.Add(new ExceptionHandler(
					(ExceptionHandlingClauseOptions)BitConverter.ToInt16(blob, start),
					new Range(BitConverter.ToInt16(blob, start + 2), blob[start + 4]),
					new Range(BitConverter.ToInt16(blob, start + 5), blob[start + 7]),
					BitConverter.ToInt32(blob, start + 8)));

				start += 12;
			}

			builder.Add(new MethodDataSection(handlers.MoveToImmutable()));

			if ((flags & CorILMethod_MoreSects) != 0)
			{
				ReadDataSection(builder, blob, start);
			}
		}

		static void ReadFatExecptionSection(ImmutableArray<MethodDataSection>.Builder builder, byte[] blob, int start)
		{
			var flags = blob[start];
			var dataSize = BitConverter.ToInt32(blob, start) >> 8;
			var n = (dataSize - 4) / 24;

			var handlers = ImmutableArray.CreateBuilder<ExceptionHandler>(n);
			start += 4;

			for (var i = 0; i < n; i++)
			{
				handlers.Add(new ExceptionHandler(
					(ExceptionHandlingClauseOptions)BitConverter.ToInt32(blob, start),
					new Range(BitConverter.ToInt32(blob, start + 4), BitConverter.ToInt32(blob, start + 8)),
					new Range(BitConverter.ToInt32(blob, start + 12), BitConverter.ToInt32(blob, start + 16)),
					BitConverter.ToInt32(blob, start + 20)));

				start += 24;
			}

			builder.Add(new MethodDataSection(handlers.MoveToImmutable()));

			if ((flags & CorILMethod_MoreSects) != 0)
			{
				ReadDataSection(builder, blob, start);
			}
		}

		const int CorILMethod_FatFormat = 0x03;
		const int CorILMethod_TinyFormat = 0x02;
		const int CorILMethod_MoreSects = 0x08;
		const int CorILMethod_InitLocals = 0x10;

		const int CorILMethod_Sect_EHTable = 0x01;
		const int CorILMethod_Sect_OptILTable = 0x02;
		const int CorILMethod_Sect_FatFormat = 0x40;
		const int CorILMethod_Sect_MoreSects = 0x80;
	}
}
