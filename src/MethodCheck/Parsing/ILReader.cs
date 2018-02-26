// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using MethodCheck.Data;
using OpCode = System.Reflection.Emit.OpCode;

namespace MethodCheck.Parsing
{
	partial class ILReader : IEnumerator<Instruction>, IEnumerable<Instruction>
	{
		public ILReader(byte[] buffer, int start, int length)
		{
			_buffer = buffer;
			_start = start;
			_end = start + length;
			_offset = start;
		}

		public IEnumerator<Instruction> GetEnumerator()
		{
			if (Interlocked.Exchange(ref _started, 1) == 0)
			{
				return this;
			}
			else
			{
				return new ILReader(_buffer, _start, _end - _start);
			}
		}

		protected bool MoveNext()
		{
			if (_offset == _end)
			{
				_current = null;
				return false;
			}
			else
			{
				var tmp = Read();
				_offset += tmp.Range.Length;
				_current = tmp;
				return true;
			}
		}

		#region IEnumerable Members

		[DebuggerStepThrough]
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion

		#region IEnumerator<Instruction> Members

		Instruction IEnumerator<Instruction>.Current => _current;

		#endregion

		#region IDisposable Members

		void IDisposable.Dispose()
		{
		}

		#endregion

		#region IEnumerator Members

		object IEnumerator.Current => _current;

		[DebuggerStepThrough]
		bool IEnumerator.MoveNext() => MoveNext();

		void IEnumerator.Reset()
		{
			_offset = _start;
			_current = null;
		}

		#endregion

		Instruction ReadInlineNoneInstruction(OpCode opcode) => CreateInstruction(opcode.Size, opcode, null);
		Instruction ReadShortInlineVarInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 1, opcode, (short)ReadUInt8(_offset + opcode.Size));
		Instruction ReadShortInlineIInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 1, opcode, (int)(sbyte)ReadUInt8(_offset + opcode.Size));
		Instruction ReadInlineIInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 4, opcode, ReadInt32(_offset + opcode.Size));
		Instruction ReadInlineI8Instruction(OpCode opcode) => CreateInstruction(opcode.Size + 8, opcode, ReadInt64(_offset + opcode.Size));
		Instruction ReadShortInlineRInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 4, opcode, ReadSingle(_offset + opcode.Size));
		Instruction ReadInlineRInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 8, opcode, ReadDouble(_offset + opcode.Size));
		Instruction ReadInlineTokInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 4, opcode, new MetadataToken(ReadInt32(_offset + opcode.Size)));
		Instruction ReadInlineVarInstruction(OpCode opcode) => CreateInstruction(opcode.Size + 2, opcode, ReadInt16(_offset + opcode.Size));

		Instruction ReadShortInlineBrTargetInstruction(OpCode opcode)
		{
			var delta = (sbyte)ReadUInt8(_offset + opcode.Size);
			var length = opcode.Size + 1;
			return CreateInstruction(length, opcode, new Label(_offset - _start + length + delta));
		}

		Instruction ReadInlineBrTargetInstruction(OpCode opcode)
		{
			var delta = ReadInt32(_offset + opcode.Size);
			var length = opcode.Size + 4;
			return CreateInstruction(length, opcode, new Label(_offset - _start + length + delta));
		}

		Instruction ReadInlineSwitchInstruction(OpCode opcode)
		{
			var pos = _offset + opcode.Size;
			var labelCount = ReadInt32(pos);

			pos += 4;
			var relativeOffset = pos - _start + labelCount * 4;

			var value = new Label[labelCount];

			for (var i = 0; i < value.Length; i++)
			{
				value[i] = relativeOffset + ReadInt32(pos);
				pos += 4;
			}

			return CreateInstruction(pos - _offset, opcode, value);
		}

		Instruction CreateInstruction(int length, OpCode opcode, object value) => new Instruction(new Range(_offset - _start, length), opcode, value);

		byte ReadUInt8(int offset)
		{
			if (offset + 1 > _end)
			{
				throw new ILException("Incomplete Argument");
			}

			return _buffer[offset];
		}

		short ReadInt16(int offset)
		{
			if (offset + 2 > _end)
			{
				throw new ILException("Incomplete Argument");
			}

			return BitConverter.ToInt16(_buffer, offset);
		}

		int ReadInt32(int offset)
		{
			if (offset + 4 > _end)
			{
				throw new ILException("Incomplete Argument");
			}

			return BitConverter.ToInt32(_buffer, offset);
		}

		long ReadInt64(int offset)
		{
			if (offset + 8 > _end)
			{
				throw new ILException("Incomplete Argument");
			}

			return BitConverter.ToInt64(_buffer, offset);
		}

		float ReadSingle(int offset)
		{
			if (offset + 4 > _end)
			{
				throw new ILException("Incomplete Argument");
			}

			return BitConverter.ToSingle(_buffer, offset);
		}

		double ReadDouble(int offset)
		{
			if (offset + 8 > _end)
			{
				throw new ILException("Incomplete Argument");
			}

			return BitConverter.ToDouble(_buffer, offset);
		}

		readonly byte[] _buffer;
		readonly int _start;
		readonly int _end;
		int _started;
		int _offset;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		Instruction _current;
	}
}
