// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Diagnostics;

namespace MethodCheck.Core.Data
{
	public readonly struct Label : IEquatable<Label>, IComparable<Label>
	{
		public Label(int offset)
		{
			_offset = offset;
		}

		public override bool Equals(object obj) => obj is Label && Equals((Label)obj);
		public bool Equals(Label other) => _offset == other._offset;
		public int CompareTo(Label other) => _offset.CompareTo(other._offset);
		public override int GetHashCode() => _offset;
		public override string ToString() => "IL_" + _offset.ToString("X4");

		public static explicit operator int(Label label) => label._offset;
		public static implicit operator Label(int offset) => new Label(offset);
		public static Label operator +(Label label, int offset) => new Label(label._offset + offset);
		public static Label operator -(Label label, int offset) => new Label(label._offset - offset);
		public static int operator -(Label lhs, Label rhs) => lhs._offset - rhs._offset;
		public static bool operator ==(Label lhs, Label rhs) => lhs.Equals(rhs);
		public static bool operator !=(Label lhs, Label rhs) => !lhs.Equals(rhs);
		public static bool operator >(Label lhs, Label rhs) => lhs.CompareTo(rhs) > 0;
		public static bool operator <(Label lhs, Label rhs) => lhs.CompareTo(rhs) < 0;
		public static bool operator >=(Label lhs, Label rhs) => lhs.CompareTo(rhs) >= 0;
		public static bool operator <=(Label lhs, Label rhs) => lhs.CompareTo(rhs) <= 0;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly int _offset;
	}
}
