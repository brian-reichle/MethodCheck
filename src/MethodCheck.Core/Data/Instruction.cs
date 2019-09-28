// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Reflection.Emit;

namespace MethodCheck.Core.Data
{
	public sealed class Instruction
	{
		public const int MaxMnemonicLength = 14;

		public Instruction(Range range, OpCode opcode, object? argument)
		{
			Kind = InstructionKind.OpCode;
			Range = range;
			OpCode = opcode;
			Argument = argument;
		}

		public Instruction(Range range)
		{
			Kind = InstructionKind.Invalid;
			Range = range;
		}

		public InstructionKind Kind { get; }
		public Range Range { get; }
		public OpCode OpCode { get; }
		public object? Argument { get; }

		public override string ToString()
		{
			if (Argument == null)
			{
				return Range.Offset + ": " + OpCode.Name;
			}
			else
			{
				return Range.Offset + ": " + OpCode.Name + " " + Argument;
			}
		}
	}
}
