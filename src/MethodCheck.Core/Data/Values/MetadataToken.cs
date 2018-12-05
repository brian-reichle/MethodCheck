// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Diagnostics;

namespace MethodCheck.Core.Data
{
	public readonly struct MetadataToken : IEquatable<MetadataToken>
	{
		public MetadataToken(int token)
		{
			_token = token;
		}

		public override bool Equals(object obj) => obj is MetadataToken && Equals((MetadataToken)obj);
		public bool Equals(MetadataToken other) => other._token == _token;
		public override int GetHashCode() => _token;
		public override string ToString() => _token.ToString("X8");

		public static bool operator ==(MetadataToken token1, MetadataToken token2) => token1._token == token2._token;
		public static bool operator !=(MetadataToken token1, MetadataToken token2) => token1._token != token2._token;

		public static implicit operator MetadataToken(int token) => new MetadataToken(token);

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly int _token;
	}
}
