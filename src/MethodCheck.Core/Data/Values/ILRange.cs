// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;

namespace MethodCheck.Core.Data
{
	public readonly struct ILRange : IEquatable<ILRange>
	{
		public ILRange(Label offset, int length)
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

		public override bool Equals(object obj) => obj is ILRange range && Equals(range);
		public bool Equals(ILRange other) => Offset == other.Offset && Length == other.Length;
		public bool Contains(ILRange range) => range.Offset >= Offset && (range.Offset + range.Length - Offset) <= Length;
		public override string ToString() => $"{Offset} ({Length})";

		public static bool operator ==(ILRange range1, ILRange range2) => range1.Equals(range2);
		public static bool operator !=(ILRange range1, ILRange range2) => !range1.Equals(range2);
	}
}
