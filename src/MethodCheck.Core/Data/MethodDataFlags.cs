// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;

namespace MethodCheck.Core.Data
{
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
	[Flags]
	public enum MethodDataFlags
	{
		None = 0,
		InitFields = 1 << 0,
	}
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
}
