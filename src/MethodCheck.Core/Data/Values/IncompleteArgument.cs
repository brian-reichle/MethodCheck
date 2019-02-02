namespace MethodCheck.Core.Data
{
	sealed class IncompleteArgument
	{
		public static IncompleteArgument Value { get; } = new IncompleteArgument();

		IncompleteArgument()
		{
		}

		public override string ToString() => "??";
	}
}
