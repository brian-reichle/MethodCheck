using System;

namespace MethodCheck.Core.Data
{
	[Flags]
	public enum MethodDataFlags
	{
		None = 0,
		InitFields = 1 << 0,
	}
}
