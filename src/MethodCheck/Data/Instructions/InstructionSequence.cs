// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MethodCheck.Data
{
	[DebuggerDisplay("Count = {Count}")]
	sealed class InstructionSequence : IList<Instruction>, IList, IReadOnlyList<Instruction>
	{
		public static readonly InstructionSequence Empty = new InstructionSequence(Array.Empty<Instruction>());

		public InstructionSequence(IEnumerable<Instruction> instructions)
			: this(instructions.ToArray())
		{
		}

		InstructionSequence(Instruction[] instructions)
		{
			_instructions = instructions;
			_indexOffset = 0;
			_indexLength = _instructions.Length;
			_offsets = new Label[_instructions.Length + 1];

			for (var i = 0; i < _instructions.Length; i++)
			{
				_offsets[i] = _instructions[i].Range.Offset;
			}

			if (_instructions.Length > 0)
			{
				var lastInstruction = _instructions[_instructions.Length - 1];
				_offsets[_instructions.Length] = lastInstruction.Range.Offset + lastInstruction.Range.Length;
			}
		}

		InstructionSequence(InstructionSequence parent, int indexOffset, int indexLength)
		{
			_instructions = parent._instructions;
			_offsets = parent._offsets;
			_indexOffset = indexOffset;
			_indexLength = indexLength;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public int Count => _indexLength;

		public Range ILRange
		{
			get
			{
				var from = _offsets[_indexOffset];
				var to = _offsets[_indexOffset + _indexLength];
				return new Range(from, to - from);
			}
		}

		public int IndexFromOffset(Label offset)
		{
			return Array.BinarySearch(_offsets, _indexOffset, _indexLength, offset);
		}

		public InstructionSequence SubSequence(int indexOffset, int indexLength)
		{
			if (indexOffset < 0) throw new ArgumentOutOfRangeException(nameof(indexOffset));
			if (indexLength < 0) throw new ArgumentOutOfRangeException(nameof(indexLength));
			if (indexOffset + indexLength > _indexLength) throw new ArgumentOutOfRangeException();

			return new InstructionSequence(this, _indexOffset + indexOffset, indexLength);
		}

		public InstructionSequence SubSequenceByRange(Range range)
		{
			var from = Array.BinarySearch(_offsets, _indexOffset, _indexLength + 1, range.Offset);

			if (from < 0) throw new ArgumentOutOfRangeException();

			var to = Array.BinarySearch(_offsets, from, _indexOffset - from + _indexLength + 1, range.Offset + range.Length);
			var len = to - from;

			if (len < 0) throw new ArgumentOutOfRangeException();

			return new InstructionSequence(this, from, len);
		}

		public Instruction this[int index] => _instructions[_indexOffset + index];

		#region IList<Instruction> Members

		int IList<Instruction>.IndexOf(Instruction item) => throw new NotSupportedException();
		void IList<Instruction>.Insert(int index, Instruction item) => throw new NotSupportedException();
		void IList<Instruction>.RemoveAt(int index) => throw new NotSupportedException();

		Instruction IList<Instruction>.this[int index]
		{
			[DebuggerStepThrough]
			get { return this[index]; }
			set { throw new NotSupportedException(); }
		}

		#endregion

		#region IList Members

		int IList.Add(object value) => throw new NotSupportedException();
		bool IList.Contains(object value) => IndexOfCore(value as Instruction) >= 0;
		void IList.Clear() => throw new NotSupportedException();
		int IList.IndexOf(object value) => IndexOfCore(value as Instruction);
		void IList.Insert(int index, object value) => throw new NotSupportedException();
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool IList.IsFixedSize => true;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool IList.IsReadOnly => true;
		void IList.Remove(object value) => throw new NotSupportedException();
		void IList.RemoveAt(int index) => throw new NotSupportedException();

		object IList.this[int index]
		{
			[DebuggerStepThrough]
			get { return this[index]; }
			set { throw new NotSupportedException(); }
		}

		#endregion

		#region ICollection<Instruction> Members

		void ICollection<Instruction>.Add(Instruction item) => throw new NotSupportedException();
		void ICollection<Instruction>.Clear() => throw new NotSupportedException();
		bool ICollection<Instruction>.Contains(Instruction item) => IndexOfCore(item) >= 0;
		void ICollection<Instruction>.CopyTo(Instruction[] array, int arrayIndex) => Array.Copy(_instructions, 0, array, arrayIndex, _instructions.Length);
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection<Instruction>.IsReadOnly => true;
		bool ICollection<Instruction>.Remove(Instruction item) => throw new NotSupportedException();

		#endregion

		#region ICollection Members

		void ICollection.CopyTo(Array array, int index) => Array.Copy(_instructions, _indexOffset, array, index, _indexLength);
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool ICollection.IsSynchronized => false;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		object ICollection.SyncRoot => _instructions.SyncRoot;

		#endregion

		#region IEnumerable<Instruction> Members

		public IEnumerator<Instruction> GetEnumerator()
		{
			var end = _indexOffset + _indexLength;

			for (var i = _indexOffset; i < end; i++)
			{
				yield return _instructions[i];
			}
		}

		#endregion

		#region IEnumerable Members

		[DebuggerStepThrough]
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion

		int IndexOfCore(Instruction item)
		{
			if (item == null)
			{
				return -1;
			}

			var index = IndexFromOffset(item.Range.Offset);

			if (index < 0)
			{
				return -1;
			}

			return _instructions[_indexOffset + index] == item ? index : -1;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly int _indexOffset;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly int _indexLength;
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		readonly Instruction[] _instructions;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly Label[] _offsets;
	}
}
