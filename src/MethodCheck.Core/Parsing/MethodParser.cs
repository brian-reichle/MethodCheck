// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using MethodCheck.Core.Data;

namespace MethodCheck.Core.Parsing
{
	public static class MethodParser
	{
		public static MethodData ParseBody(ReadOnlySpan<byte> buffer)
		{
			if (buffer.Length == 0)
			{
				return null;
			}

			var type = buffer[0] & 0x03;

			try
			{
				if (type == CorILMethod_TinyFormat)
				{
					return ParseTiny(buffer);
				}
				else if (type == CorILMethod_FatFormat)
				{
					return ParseFat(buffer);
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

		public static MethodData ParseIL(ReadOnlySpan<byte> buffer)
		{
			return new MethodData(
				default,
				0,
				0,
				MethodDataFlags.None,
				ReadIL(buffer),
				ImmutableArray<MethodDataSection>.Empty);
		}

		static MethodData ParseTiny(ReadOnlySpan<byte> buffer)
		{
			var codeSize = buffer[0] >> 2;

			return new MethodData(
				default,
				8,
				codeSize,
				MethodDataFlags.None,
				ReadIL(buffer.Slice(1, codeSize)),
				ImmutableArray<MethodDataSection>.Empty);
		}

		static MethodData ParseFat(ReadOnlySpan<byte> buffer)
		{
			if (buffer.Length < 12) return null;
			var header = MemoryMarshal.Cast<byte, FatMethodHeader>(buffer)[0];
			var headerLength = buffer[1] >> 2 & 0x3C;

			ImmutableArray<MethodDataSection> dataSections;

			if ((buffer[0] & CorILMethod_MoreSects) != 0)
			{
				var sectionStart = (headerLength + header.CodeSize + 3) & ~3;
				dataSections = CreateSections(buffer.Slice(sectionStart));
			}
			else
			{
				dataSections = ImmutableArray<MethodDataSection>.Empty;
			}

			var flags = MethodDataFlags.None;

			if ((buffer[0] & CorILMethod_InitLocals) != 0)
			{
				flags = MethodDataFlags.InitFields;
			}

			return new MethodData(
				header.LocalVarSigTok,
				header.MaxStack,
				header.CodeSize,
				flags,
				ReadIL(buffer.Slice(headerLength, header.CodeSize)),
				dataSections);
		}

		static ImmutableArray<Instruction> ReadIL(ReadOnlySpan<byte> buffer)
		{
			if (buffer.IsEmpty)
			{
				return ImmutableArray<Instruction>.Empty;
			}

			try
			{
				var result = ImmutableArray.CreateBuilder<Instruction>();
				var reader = new ILReader(buffer);

				while (reader.MoveNext())
				{
					result.Add(reader.Current);
				}

				return result.ToImmutable();
			}
			catch (ILException)
			{
				return ImmutableArray<Instruction>.Empty;
			}
		}

		static ImmutableArray<MethodDataSection> CreateSections(ReadOnlySpan<byte> buffer)
		{
			var start = 0;
			var builder = ImmutableArray.CreateBuilder<MethodDataSection>();

			while (start < buffer.Length)
			{
				var flags = buffer[start];
				int dataSize;

				if ((flags & CorILMethod_Sect_FatFormat) == 0)
				{
					dataSize = buffer[start + 1];
				}
				else
				{
					dataSize = MemoryMarshal.Read<int>(buffer) >> 8;
				}

				builder.Add(CreateSection(buffer.Slice(start, dataSize)));

				if ((flags & CorILMethod_Sect_MoreSects) == 0)
				{
					break;
				}

				start += dataSize;
			}

			return builder.ToImmutable();
		}

		static MethodDataSection CreateSection(ReadOnlySpan<byte> sectionBuffer)
		{
			switch (sectionBuffer[0] & 0x7F)
			{
				case CorILMethod_Sect_EHTable:
					{
						var clauses = MemoryMarshal.Cast<byte, SmallExceptionClause>(sectionBuffer.Slice(4));
						return new MethodDataSection(CreateHandlers(clauses));
					}

				case CorILMethod_Sect_EHTable | CorILMethod_Sect_FatFormat:
					{
						var clauses = MemoryMarshal.Cast<byte, FatExceptionClause>(sectionBuffer.Slice(4));
						return new MethodDataSection(CreateHandlers(clauses));
					}

				default:
					return new MethodDataSection(ImmutableArray<ExceptionHandler>.Empty);
			}
		}

		static ImmutableArray<ExceptionHandler> CreateHandlers(ReadOnlySpan<FatExceptionClause> clauses)
		{
			var handlers = ImmutableArray.CreateBuilder<ExceptionHandler>(clauses.Length);

			for (var i = 0; i < clauses.Length; i++)
			{
				handlers.Add(CreateHandler(clauses[i]));
			}

			return handlers.MoveToImmutable();
		}

		static ImmutableArray<ExceptionHandler> CreateHandlers(ReadOnlySpan<SmallExceptionClause> clauses)
		{
			var handlers = ImmutableArray.CreateBuilder<ExceptionHandler>(clauses.Length);

			for (var i = 0; i < clauses.Length; i++)
			{
				handlers.Add(CreateHandler(clauses[i]));
			}

			return handlers.MoveToImmutable();
		}

		static ExceptionHandler CreateHandler(in FatExceptionClause clause)
		{
			return new ExceptionHandler(
				(ExceptionHandlingClauseOptions)clause.Flags,
				new Range(clause.TryOffset, clause.TryLength),
				new Range(clause.HandlerOffset, clause.HandlerLength),
				clause.FilterOrType);
		}

		static ExceptionHandler CreateHandler(in SmallExceptionClause clause)
		{
			return new ExceptionHandler(
				(ExceptionHandlingClauseOptions)clause.Flags,
				new Range(clause.TryOffset, clause.TryLength),
				new Range(clause.HandlerOffset, clause.HandlerLength),
				clause.FilterOrType);
		}

		const int CorILMethod_FatFormat = 0x03;
		const int CorILMethod_TinyFormat = 0x02;
		const int CorILMethod_MoreSects = 0x08;
		const int CorILMethod_InitLocals = 0x10;

		const int CorILMethod_Sect_EHTable = 0x01;
		const int CorILMethod_Sect_FatFormat = 0x40;
		const int CorILMethod_Sect_MoreSects = 0x80;

		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12)]
		struct FatMethodHeader
		{
			public ushort FlagsAndSize;
			public ushort MaxStack;
			public int CodeSize;
			public int LocalVarSigTok;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 24)]
		struct FatExceptionClause
		{
			public int Flags;
			public int TryOffset;
			public int TryLength;
			public int HandlerOffset;
			public int HandlerLength;
			public int FilterOrType;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12)]
		struct SmallExceptionClause
		{
			public ushort Flags;
			public ushort TryOffset;
			public byte TryLength;
			public ushort HandlerOffset;
			public byte HandlerLength;
			public int FilterOrType;
		}
	}
}
