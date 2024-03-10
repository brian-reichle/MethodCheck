// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Diagnostics;
using System.Globalization;

namespace MethodCheck.Core.Data
{
	public readonly struct MetadataToken(int token) : IEquatable<MetadataToken>
	{
		public override bool Equals(object? obj) => obj is MetadataToken token && Equals(token);
		public bool Equals(MetadataToken other) => other._token == _token;
		public override int GetHashCode() => _token;
		public override string ToString() => _token.ToString("X8", CultureInfo.InvariantCulture);

		public static bool operator ==(MetadataToken token1, MetadataToken token2) => token1._token == token2._token;
		public static bool operator !=(MetadataToken token1, MetadataToken token2) => token1._token != token2._token;

		public static implicit operator MetadataToken(int token) => new(token);

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly int _token = token;
	}
}
