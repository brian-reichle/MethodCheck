// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using MethodCheck.Core.Data;
using MethodCheck.Core.Data.Sections;
using FlowControl = System.Reflection.Emit.FlowControl;

namespace MethodCheck.Core
{
	public static class MethodFormatter
	{
		public static string Format(MethodData data, bool inlineExceptionHandlers = false)
		{
			if (data == null) throw new ArgumentNullException(nameof(data));

			var sections = inlineExceptionHandlers ? BuildSections(data) : null;
			var builder = new StringBuilder();
			var jumpTargets = CollectJumpTargets(data, sections == null);

			WriteHeader(data, builder);

			var writer = new InstructionWriter(builder, jumpTargets, data.Instructions);

			if (sections != null)
			{
				writer.WriteInstructions(sections);
			}
			else
			{
				writer.WriteInstructions();
				WriteExceptionHandlers(data, builder);
			}

			return builder.ToString();
		}

		static BaseSection? BuildSections(MethodData data)
		{
			var ilRange = InstructionsRange(data.Instructions);
			BaseSection? sections = null;

			foreach (var dataSection in data.DataSections)
			{
				if (dataSection.ExceptionHandlers.Length > 0)
				{
					if (sections != null)
					{
						return null;
					}

					try
					{
						sections = SectionFactory.Create(ilRange, dataSection.ExceptionHandlers);
					}
					catch (CannotGenerateSectionException)
					{
						return null;
					}
				}
			}

			return sections;
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

		static ImmutableHashSet<Label> CollectJumpTargets(MethodData data, bool includeExceptionHandlers)
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

			if (includeExceptionHandlers)
			{
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

		static ILRange InstructionsRange(ImmutableArray<Instruction> instructions)
		{
			if (instructions.Length == 0)
			{
				return default;
			}

			var start = instructions[0].Range.Offset;
			var lastInstruction = instructions[instructions.Length - 1];
			var end = lastInstruction.Range.Offset + lastInstruction.Range.Length;
			return new ILRange(start, end - start);
		}

		struct InstructionWriter
		{
			public InstructionWriter(StringBuilder builder, ImmutableHashSet<Label> jumpTargets, ImmutableArray<Instruction> instructions)
			{
				_builder = builder;
				_pendingNewline = false;
				_indentDepth = 1;
				_jumpTargets = jumpTargets;
				_instructions = instructions;
			}

			public void WriteInstructions()
			{
				foreach (var instruction in _instructions)
				{
					WriteInstruction(instruction);
				}
			}

			public void WriteInstructions(BaseSection section)
			{
				switch (section)
				{
					case ILSection ilSection:
						WriteInstructions(ilSection);
						break;

					case SequenceSection sequenceSection:
						foreach (var subSection in sequenceSection.Sections)
						{
							WriteInstructions(subSection);
						}
						break;

					case TryBlockSection trySection:
						WriteInstructions(trySection);
						break;
				}
			}

			void WriteBlock(BaseSection section)
			{
				_pendingNewline = false;
				StartBlock();
				WriteInstructions(section);
				EndBlock();
				_pendingNewline = false;
			}

			void WriteInstructions(ILSection section)
			{
				var index = IndexOf(section.Range.Offset);
				var end = section.Range.Offset + section.Range.Length;

				while (index < _instructions.Length)
				{
					var instruction = _instructions[index++];

					if (instruction.Range.Offset >= end)
					{
						break;
					}

					WriteInstruction(instruction);
				}
			}

			void WriteInstructions(TryBlockSection section)
			{
				WriteIndent();
				_builder.Append(".try").AppendLine();

				WriteBlock(section.TryBlock);

				foreach (var handler in section.HandlerBlocks)
				{
					WriteHandler(handler);
				}
			}

			void WriteInstruction(Instruction instruction)
			{
				if (_jumpTargets.Contains(instruction.Range.Offset))
				{
					WriteLabelDef(instruction.Range.Offset);
				}

				WriteInstructionCore(instruction);
			}

			void WriteHandler(HandlerBlock handler)
			{
				WriteIndent();

				switch (handler.Type)
				{
					case ExceptionHandlingClauseOptions.Fault:
						_builder.Append(".fault").AppendLine();
						break;

					case ExceptionHandlingClauseOptions.Finally:
						_builder.Append(".finally").AppendLine();
						break;

					case ExceptionHandlingClauseOptions.Clause:
					case ExceptionHandlingClauseOptions.Filter:
						_builder.Append(".catch");

						if (handler.FilterSection != null)
						{
							_builder.AppendLine();
							WriteBlock(handler.FilterSection);
						}
						else if (handler.ExceptionType != default)
						{
							_builder.Append(' ').Append(handler.ExceptionType).AppendLine();
						}
						break;

					default:
						throw new ArgumentException("Unknown handler type: " + handler.Type, nameof(handler));
				}

				WriteBlock(handler.HandlerSection);
			}

			void WriteInstructionCore(Instruction instruction)
			{
				BeginLine();
				WriteIndent();

				switch (instruction.Kind)
				{
					case InstructionKind.Invalid:
						_builder.Append("??");
						break;

					case InstructionKind.OpCode:
						var name = instruction.OpCode.Name!;
						_builder.Append(name);

						if (instruction.Argument != null)
						{
							_builder.Append(' ', Instruction.MaxMnemonicLength + 1 - name.Length);
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

			void WriteLabelDef(Label label)
			{
				BeginLine();
				_builder.Append(label);
				_builder.AppendLine();
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

			void StartBlock()
			{
				WriteIndent();
				_builder.Append('{').AppendLine();
				_indentDepth++;
			}

			void EndBlock()
			{
				_indentDepth--;
				WriteIndent();
				_builder.Append('}').AppendLine();
			}

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
				_builder.Append(' ', _indentDepth * 2);
			}

			int IndexOf(Label label)
			{
				var min = 0;
				var max = _instructions.Length - 1;

				while (min <= max)
				{
					var mid = (min + max) / 2;
					var offset = _instructions[mid].Range.Offset;

					if (label < offset)
					{
						max = mid - 1;
					}
					else if (label > offset)
					{
						min = mid + 1;
					}
					else
					{
						return mid;
					}
				}

				throw new ArgumentException("Label does not correspond to an instruction.", nameof(label));
			}

			bool _pendingNewline;
			int _indentDepth;
			readonly StringBuilder _builder;
			readonly ImmutableHashSet<Label> _jumpTargets;
			readonly ImmutableArray<Instruction> _instructions;
		}
	}
}
