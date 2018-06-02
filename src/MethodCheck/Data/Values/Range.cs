// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;

namespace MethodCheck.Data
{
	readonly struct Range : IEquatable<Range>
	{
		public Range(Label offset, int length)
		{
			Offset = offset;
			Length = length;
		}

		public Label Offset { get; }
		public int Length { get; }

		public override int GetHashCode()
		{
			unchecked
			{
				var tmp = (uint)Offset.GetHashCode();
				tmp = ((tmp << 16) | (tmp >> 16)) ^ (uint)Length;
				return (int)tmp;
			}
		}

		public override bool Equals(object obj) => obj is Range && Equals((Range)obj);
		public bool Equals(Range other) => Offset == other.Offset && Length == other.Length;
		public bool Contains(Range range) => range.Offset >= Offset && (range.Offset + range.Length - Offset) <= Length;
		public override string ToString() => $"{Offset} ({Length})";

		public static bool operator ==(Range range1, Range range2) => range1.Equals(range2);
		public static bool operator !=(Range range1, Range range2) => !range1.Equals(range2);
	}
}
