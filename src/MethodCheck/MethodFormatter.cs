// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MethodCheck.Data;
using FlowControl = System.Reflection.Emit.FlowControl;

namespace MethodCheck
{
	static class MethodFormatter
	{
		public static string Format(MethodData data)
		{
			var builder = new StringBuilder();
			var jumpTargets = CollectJumpTargets(data);

			WriteHeader(data, builder);
			WriteInstructions(data, builder, jumpTargets);
			WriteExceptionHandlers(data, builder);

			return builder.ToString();
		}

		static void WriteInstructions(MethodData data, StringBuilder builder, HashSet<Label> jumpTargets)
		{
			foreach (var instruction in data.Instructions)
			{
				if (jumpTargets.Contains(instruction.Range.Offset))
				{
					builder.Append(instruction.Range.Offset);
					builder.AppendLine();
				}

				builder.Append("  ");
				builder.Append(instruction.OpCode.Name);

				if (instruction.Argument != null)
				{
					builder.Append(' ', Instruction.MaxMnemonicLength + 1 - instruction.OpCode.Name.Length);
					builder.Append(' ');
					WriteArgument(builder, instruction);
				}

				builder.AppendLine();

				switch (instruction.OpCode.FlowControl)
				{
					case FlowControl.Branch:
					case FlowControl.Return:
					case FlowControl.Throw:
						builder.AppendLine();
						break;
				}
			}
		}

		static void WriteExceptionHandlers(MethodData data, StringBuilder builder)
		{
			foreach (var ex in data.DataSections.SelectMany(x => x.ExceptionHandlers))
			{
				builder.Append(".try ");
				builder.Append(ex.TryRange.Offset);
				builder.Append(" to ");
				builder.Append(ex.TryRange.Offset + ex.TryRange.Length);

				switch (ex.Type)
				{
					case ExceptionHandlingClauseOptions.Clause:
						builder.Append(" catch ");
						builder.Append((MetadataToken)ex.FilterOrType);
						builder.Append(' ');
						break;

					case ExceptionHandlingClauseOptions.Fault:
						builder.Append(" fault ");
						break;

					case ExceptionHandlingClauseOptions.Finally:
						builder.Append(" finally ");
						break;

					case ExceptionHandlingClauseOptions.Filter:
						builder.Append(" filter ");
						builder.Append(new Label(ex.FilterOrType));
						builder.Append(' ');
						break;
				}

				builder.Append(ex.HandlerRange.Offset);
				builder.Append(" to ");
				builder.Append(ex.HandlerRange.Offset + ex.HandlerRange.Length);
				builder.AppendLine();
			}
		}

		static void WriteArgument(StringBuilder builder, Instruction instruction)
		{
			if (instruction.Argument is Label[] labelArr)
			{
				builder.Append('{');

				foreach (var label in labelArr)
				{
					builder.Append(' ');
					builder.Append(label);
				}

				builder.Append(" }");
			}
			else if (instruction.Argument is Label label)
			{
				var diff = label - (instruction.Range.Offset + instruction.Range.Length);
				builder.Append(label);
				builder.Append(" // ");

				if (diff >= 0)
				{
					builder.Append('+');
				}

				builder.Append(diff);
			}
			else
			{
				builder.Append(instruction.Argument);
			}
		}

		static HashSet<Label> CollectJumpTargets(MethodData data)
		{
			var jumpTargets = new HashSet<Label>();

			foreach (var instruction in data.Instructions)
			{
				if (instruction.Argument is Label label)
				{
					jumpTargets.Add(label);
				}
				else if (instruction.Argument is Label[] argArr)
				{
					for (var i = 0; i < argArr.Length; i++)
					{
						jumpTargets.Add(argArr[i]);
					}
				}
			}

			foreach (var ex in data.DataSections.SelectMany(x => x.ExceptionHandlers))
			{
				jumpTargets.Add(ex.TryRange.Offset);
				jumpTargets.Add(ex.TryRange.Offset + ex.TryRange.Length);
				jumpTargets.Add(ex.HandlerRange.Offset);
				jumpTargets.Add(ex.HandlerRange.Offset + ex.HandlerRange.Length);

				if (ex.Type == ExceptionHandlingClauseOptions.Filter)
				{
					jumpTargets.Add(new Label(ex.FilterOrType));
				}
			}
			return jumpTargets;
		}

		static void WriteHeader(MethodData data, StringBuilder builder)
		{
			if (data.MaxStack != 0)
			{
				builder.Append(".maxstack ");
				builder.Append(data.MaxStack);
				builder.AppendLine();
			}

			if (data.LocalsToken != 0)
			{
				builder.Append(".locals ");
				builder.Append(data.LocalsToken);
				builder.AppendLine();
			}

			if (data.CodeSize != 0)
			{
				builder.Append("// code size: ");
				builder.Append(data.CodeSize);
				builder.AppendLine();
			}
		}
	}
}
