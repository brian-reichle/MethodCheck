// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
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
				default(MetadataToken),
				0,
				0,
				ReadIL(blob, 0, blob.Length),
				EmptyDataSections);
		}

		static MethodData ParseTiny(byte[] blob)
		{
			var codeSize = blob[0] >> 2;

			return new MethodData(
				default(MetadataToken),
				8,
				codeSize,
				ReadIL(blob, 1, codeSize),
				EmptyDataSections);
		}

		static MethodData ParseFat(byte[] blob)
		{
			if (blob.Length < 12) return null;
			var headerLength = blob[1] >> 2 & 0x3C;
			var codeSize = BitConverter.ToInt32(blob, 4);

			IReadOnlyList<MethodDataSection> dataSections;

			if ((blob[0] & CorILMethod_MoreSects) != 0)
			{
				dataSections = ExtractDataSections(blob, headerLength, codeSize);
			}
			else
			{
				dataSections = EmptyDataSections;
			}

			return new MethodData(
				BitConverter.ToInt32(blob, 8),
				BitConverter.ToUInt16(blob, 2),
				codeSize,
				ReadIL(blob, headerLength, codeSize),
				dataSections);
		}

		static InstructionSequence ReadIL(byte[] blob, int start, int length)
		{
			if (start >= blob.Length)
			{
				return InstructionSequence.Empty;
			}

			try
			{
				using (var reader = new ILReader(blob, start, length))
				{
					return new InstructionSequence(reader);
				}
			}
			catch (ILException)
			{
				return InstructionSequence.Empty;
			}
		}

		static IReadOnlyList<MethodDataSection> ExtractDataSections(byte[] blob, int headerLength, int codeSize)
		{
			var tmp = new List<MethodDataSection>();
			ReadDataSection(tmp, blob, (headerLength + codeSize + 3) & ~3);
			return tmp.ToArray();
		}

		static void ReadDataSection(List<MethodDataSection> dataSections, byte[] blob, int start)
		{
			switch (blob[start] & 0x7F)
			{
				case CorILMethod_Sect_EHTable:
					ReadSmallExceptionSection(dataSections, blob, start);
					return;

				case CorILMethod_Sect_EHTable | CorILMethod_Sect_FatFormat:
					ReadFatExecptionSection(dataSections, blob, start);
					return;
			}
		}

		static void ReadSmallExceptionSection(List<MethodDataSection> dataSections, byte[] blob, int start)
		{
			var flags = blob[start];
			var dataSize = blob[start + 1];
			var n = (dataSize - 4) / 12;

			var handlers = new ExceptionHandler[n];
			start += 4;

			for (var i = 0; i < handlers.Length; i++)
			{
				handlers[i] = new ExceptionHandler(
					(ExceptionHandlingClauseOptions)BitConverter.ToInt16(blob, start),
					new Range(BitConverter.ToInt16(blob, start + 2), blob[start + 4]),
					new Range(BitConverter.ToInt16(blob, start + 5), blob[start + 7]),
					BitConverter.ToInt32(blob, start + 8));

				start += 12;
			}

			dataSections.Add(new MethodDataSection(handlers));

			if ((flags & CorILMethod_MoreSects) != 0)
			{
				ReadDataSection(dataSections, blob, start);
			}
		}

		static void ReadFatExecptionSection(List<MethodDataSection> dataSections, byte[] blob, int start)
		{
			var flags = blob[start];
			var dataSize = BitConverter.ToInt32(blob, start) >> 8;
			var n = (dataSize - 4) / 24;

			var handlers = new ExceptionHandler[n];
			start += 4;

			for (var i = 0; i < handlers.Length; i++)
			{
				handlers[i] = new ExceptionHandler(
					(ExceptionHandlingClauseOptions)BitConverter.ToInt32(blob, start),
					new Range(BitConverter.ToInt32(blob, start + 4), BitConverter.ToInt32(blob, start + 8)),
					new Range(BitConverter.ToInt32(blob, start + 12), BitConverter.ToInt32(blob, start + 16)),
					BitConverter.ToInt32(blob, start + 20));

				start += 24;
			}

			dataSections.Add(new MethodDataSection(handlers));

			if ((flags & CorILMethod_MoreSects) != 0)
			{
				ReadDataSection(dataSections, blob, start);
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

		static readonly IReadOnlyList<MethodDataSection> EmptyDataSections = Array.Empty<MethodDataSection>();
	}
}
