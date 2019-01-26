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
#pragma warning disable SA1206 // Declaration keywords must follow order
	public ref partial struct ILReader
#pragma warning restore SA1206 // Declaration keywords must follow order
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
			if (_offset == _buffer.Length)
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
		Instruction ReadShortInlineVarInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 1, opcode, Read<byte>(_offset + opcode.Size));
		Instruction ReadShortInlineIInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 1, opcode, (int)Read<sbyte>(_offset + opcode.Size));
		Instruction ReadInlineIInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 4, opcode, Read<int>(_offset + opcode.Size));
		Instruction ReadInlineI8Instruction(OpCode opcode) => CreateInstruction(opcode.Size + 8, opcode, Read<long>(_offset + opcode.Size));
		Instruction ReadShortInlineRInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 4, opcode, Read<float>(_offset + opcode.Size));
		Instruction ReadInlineRInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 8, opcode, Read<double>(_offset + opcode.Size));
		Instruction ReadInlineTokInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 4, opcode, new MetadataToken(Read<int>(_offset + opcode.Size)));
		Instruction ReadInlineVarInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 2, opcode, Read<short>(_offset + opcode.Size));

		Instruction ReadShortInlineBrTargetInstruction(OpCode opcode)
		{
			var delta = Read<sbyte>(_offset + opcode.Size);
			var length = opcode.Size + 1;
			return CreateInstruction(length, opcode, new Label(_offset + length + delta));
		}

		Instruction ReadInlineBrTargetInstruction(OpCode opcode)
		{
			var delta = Read<int>(_offset + opcode.Size);
			var length = opcode.Size + 4;
			return CreateInstruction(length, opcode, new Label(_offset + length + delta));
		}

		Instruction ReadInlineSwitchInstruction(OpCode opcode)
		{
			var pos = _offset + opcode.Size;
			var labelCount = Read<int>(pos);

			pos += 4;
			var relativeOffset = pos + labelCount * 4;

			var builder = ImmutableArray.CreateBuilder<Label>(labelCount);

			for (var i = 0; i < labelCount; i++)
			{
				builder.Add(relativeOffset + Read<int>(pos));
				pos += 4;
			}

			return CreateInstruction(pos - _offset, opcode, builder.MoveToImmutable());
		}

		Instruction CreateInstruction(int length, OpCode opcode, object value) => new Instruction(new Range(_offset, length), opcode, value);

		T Read<T>(int offset)
			where T : unmanaged
		{
			if (offset + Unsafe.SizeOf<T>() > _buffer.Length)
			{
				throw new ILException("Incomplete Argument");
			}

			return MemoryMarshal.Read<T>(_buffer.Slice(offset));
		}

		readonly ReadOnlySpan<byte> _buffer;
		int _offset;
	}
}

#pragma warning restore CA1815 // Override equals and operator equals on value types
