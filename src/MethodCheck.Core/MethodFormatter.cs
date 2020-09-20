// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using MethodCheck.Core.Data;
using FlowControl = System.Reflection.Emit.FlowControl;

namespace MethodCheck.Core
{
	public static class MethodFormatter
	{
		public static string Format(MethodData data)
		{
			if (data == null) throw new ArgumentNullException(nameof(data));

			var builder = new StringBuilder();
			var jumpTargets = CollectJumpTargets(data);

			WriteHeader(data, builder);
			WriteInstructions(data, builder, jumpTargets);
			WriteExceptionHandlers(data, builder);

			return builder.ToString();
		}

		static void WriteInstructions(MethodData data, StringBuilder builder, ImmutableHashSet<Label> jumpTargets)
		{
			var writer = new Writer(builder);

			foreach (var instruction in data.Instructions)
			{
				if (jumpTargets.Contains(instruction.Range.Offset))
				{
					writer.WriteLabelDef(instruction.Range.Offset);
				}

				writer.WriteInstruction(instruction);
			}
		}

		static void WriteExceptionHandlers(MethodData data, StringBuilder builder)
		{
			var started = false;

			foreach (var section in data.DataSections)
			{
				foreach (var handler in section.ExceptionHandlers)
				{
					if (!started)
					{
						started = true;
						builder.AppendLine();
					}

					WriteExceptionHandler(builder, handler);
				}
			}
		}

		static void WriteExceptionHandler(StringBuilder builder, ExceptionHandler handler)
		{
			builder.Append(".try ");
			builder.Append(handler.TryRange.Offset);
			builder.Append(" to ");
			builder.Append(handler.TryRange.Offset + handler.TryRange.Length);

			switch (handler.Type)
			{
				case ExceptionHandlingClauseOptions.Clause:
					builder.Append(" catch ");
					builder.Append((MetadataToken)handler.FilterOrType);
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
					builder.Append(new Label(handler.FilterOrType));
					builder.Append(' ');
					break;
			}

			builder.Append(handler.HandlerRange.Offset);
			builder.Append(" to ");
			builder.Append(handler.HandlerRange.Offset + handler.HandlerRange.Length);
			builder.AppendLine();
		}

		static ImmutableHashSet<Label> CollectJumpTargets(MethodData data)
		{
			var jumpTargets = ImmutableHashSet.CreateBuilder<Label>();

			foreach (var instruction in data.Instructions)
			{
				switch (instruction.Argument)
				{
					case Label label:
						jumpTargets.Add(label);
						break;

					case ImmutableArray<Label> labels:
						for (var i = 0; i < labels.Length; i++)
						{
							jumpTargets.Add(labels[i]);
						}
						break;
				}
			}

			foreach (var section in data.DataSections)
			{
				foreach (var handler in section.ExceptionHandlers)
				{
					jumpTargets.Add(handler.TryRange.Offset);
					jumpTargets.Add(handler.TryRange.Offset + handler.TryRange.Length);
					jumpTargets.Add(handler.HandlerRange.Offset);
					jumpTargets.Add(handler.HandlerRange.Offset + handler.HandlerRange.Length);

					if (handler.Type == ExceptionHandlingClauseOptions.Filter)
					{
						jumpTargets.Add(new Label(handler.FilterOrType));
					}
				}
			}

			return jumpTargets.ToImmutable();
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

				if ((data.Flags & MethodDataFlags.InitFields) != 0)
				{
					builder.Append("init ");
				}

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

		struct Writer
		{
			public Writer(StringBuilder builder)
			{
				_builder = builder;
				_pendingNewline = false;
			}

			public void WriteLabelDef(Label label)
			{
				BeginLine();
				_builder.Append(label);
				_builder.AppendLine();
			}

			public void WriteInstruction(Instruction instruction)
			{
				BeginLine();
				WriteIndent();

				switch (instruction.Kind)
				{
					case InstructionKind.Invalid:
						_builder.Append("??");
						break;

					case InstructionKind.OpCode:
						_builder.Append(instruction.OpCode.Name);

						if (instruction.Argument != null)
						{
							_builder.Append(' ', Instruction.MaxMnemonicLength + 1 - instruction.OpCode.Name.Length);
							_builder.Append(' ');
							WriteArgument(instruction);
						}

						switch (instruction.OpCode.FlowControl)
						{
							case FlowControl.Branch:
							case FlowControl.Return:
							case FlowControl.Throw:
								_pendingNewline = true;
								break;
						}
						break;
				}

				_builder.AppendLine();
			}

			public override string ToString() => _builder.ToString();

			void BeginLine()
			{
				if (_pendingNewline)
				{
					_pendingNewline = false;
					_builder.AppendLine();
				}
			}

			void WriteIndent()
			{
				_builder.Append("  ");
			}

			void WriteArgument(Instruction instruction)
			{
				if (instruction.Argument == IncompleteArgument.Value)
				{
					_builder.Append("??");
					return;
				}

				switch (instruction.Argument)
				{
					case Label label:
						var diff = label - (instruction.Range.Offset + instruction.Range.Length);
						_builder.Append(label);
						_builder.Append(" // ");

						if (diff >= 0)
						{
							_builder.Append('+');
						}

						_builder.Append(diff);
						break;

					case ImmutableArray<Label> labels:
						_builder.Append('{');

						foreach (var label in labels)
						{
							_builder.Append(' ');
							_builder.Append(label);
						}

						_builder.Append(" }");
						break;

					default:
						_builder.Append(instruction.Argument);
						break;
				}
			}

			readonly StringBuilder _builder;
			bool _pendingNewline;
		}
	}
}
