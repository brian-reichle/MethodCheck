// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MethodCheck.Core.Data;
using OpCode = System.Reflection.Emit.OpCode;

#pragma warning disable CA1815 // Override equals and operator equals on value types

namespace MethodCheck.Core.Parsing
{
	public ref partial struct ILReader
	{
		public ILReader(ReadOnlySpan<byte> buffer)
		{
			_buffer = buffer;
			_offset = 0;
			Current = null;
		}

		public Instruction Current { get; private set; }

		public bool MoveNext()
		{
			if (_offset >= _buffer.Length)
			{
				Current = null;
				return false;
			}
			else
			{
				var tmp = Read();
				_offset += tmp.Range.Length;
				Current = tmp;
				return true;
			}
		}

		Instruction ReadInlineNoneInstruction(OpCode opcode) => CreateInstruction(opcode.Size, opcode, null);
		Instruction ReadShortInlineVarInstruction(OpCode opcode) => CreateInstruction<byte>(opcode);
		Instruction ReadShortInlineIInstruction(OpCode opcode) => CreateInstruction(opcode, (sbyte x, int _) => (int)x);
		Instruction ReadInlineIInstruction(OpCode opcode) => CreateInstruction<int>(opcode);
		Instruction ReadInlineI8Instruction(OpCode opcode) => CreateInstruction<long>(opcode);
		Instruction ReadShortInlineRInstruction(OpCode opcode) => CreateInstruction<float>(opcode);
		Instruction ReadInlineRInstruction(OpCode opcode) => CreateInstruction<double>(opcode);
		Instruction ReadInlineTokInstruction(OpCode opcode) => CreateInstruction(opcode, (int x, int _) => new MetadataToken(x));
		Instruction ReadInlineVarInstruction(OpCode opcode) => CreateInstruction<ushort>(opcode);
		Instruction InvalidInstruction(int length) => new Instruction(new Range(_offset, length));
		Instruction ReadShortInlineBrTargetInstruction(OpCode opcode) => CreateInstruction(opcode, (sbyte x, int end) => new Label(end + x));
		Instruction ReadInlineBrTargetInstruction(OpCode opcode) => CreateInstruction(opcode, (int x, int end) => new Label(end + x));

		Instruction ReadInlineSwitchInstruction(OpCode opcode)
		{
			var argStart = _offset + opcode.Size;

			if (argStart + 4 < _buffer.Length)
			{
				var values = MemoryMarshal.Cast<byte, int>(_buffer.Slice(argStart));
				var labelCount = values[0];

				if (labelCount + 1 < values.Length)
				{
					values = values.Slice(1, labelCount);
					var relativeOffset = argStart + 4 + labelCount * 4;
					var argument = CreateLabelList(relativeOffset, values);
					return CreateInstruction(relativeOffset - _offset, opcode, argument);
				}
			}

			return CreateInstruction(_buffer.Length - _offset, opcode, IncompleteArgument.Value);
		}

		Instruction CreateInstruction<T>(OpCode opCode)
			where T : unmanaged
			=> CreateInstruction(opCode, (T x, int _) => x);

		Instruction CreateInstruction<TIn, TOut>(OpCode opCode, Func<TIn, int, TOut> converter)
			where TIn : unmanaged
		{
			object value;
			var length = opCode.Size + Unsafe.SizeOf<TIn>();
			var end = _offset + length;

			if (end < _buffer.Length)
			{
				var rawValue = MemoryMarshal.Read<TIn>(_buffer.Slice(_offset + opCode.Size));
				value = converter(rawValue, end);
			}
			else
			{
				value = IncompleteArgument.Value;
			}

			return CreateInstruction(length, opCode, value);
		}

		Instruction CreateInstruction(int length, OpCode opcode, object value) => new Instruction(new Range(_offset, length), opcode, value);

		static ImmutableArray<Label> CreateLabelList(int relativeOffset, ReadOnlySpan<int> offsets)
		{
			var builder = ImmutableArray.CreateBuilder<Label>(offsets.Length);
			builder.Count = offsets.Length;

			for (var i = 0; i < offsets.Length; i++)
			{
				builder[i] = new Label(relativeOffset + offsets[i]);
			}

			return builder.MoveToImmutable();
		}

		readonly ReadOnlySpan<byte> _buffer;
		int _offset;
	}
}

#pragma warning restore CA1815 // Override equals and operator equals on value types
