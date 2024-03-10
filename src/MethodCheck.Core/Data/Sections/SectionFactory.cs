// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using MethodCheck.Core.Data.Sections;

namespace MethodCheck.Core.Data
{
	public static class SectionFactory
	{
		// Creates a section from the provided values, assuming the handlers are ordered correctly.
		public static BaseSection Create(ILRange range, IEnumerable<ExceptionHandler> handlers)
		{
			ArgumentNullException.ThrowIfNull(handlers);

			var generator = Builder.New();

			foreach (var handler in handlers)
			{
				generator.Add(handler);
			}

			var result = generator.CreateSection(range);
			generator.VerifyEmpty();
			return result;
		}

		readonly struct Builder
		{
			public static Builder New() => new(new List<TryBuilder>());

			Builder(List<TryBuilder> pendingTryBlocks)
			{
				_pendingTryBlocks = pendingTryBlocks;
			}

			public void Add(ExceptionHandler handler)
			{
				var handlerBlock = CreateHandlerBlock(handler);
				var handlers = FindHandlersMatchingTryBlock(handler.TryRange);
				handlers.Add(handlerBlock);
			}

			public BaseSection CreateSection(ILRange range)
			{
				var sections = ExtractSectionsInRange(range);

				if (sections.Count == 0)
				{
					return new ILSection(range);
				}
				else if (sections.Count == 1 && sections[0].Range == range)
				{
					return sections[0];
				}

				sections.Sort((x, y) => x.Range.Offset.CompareTo(y.Range.Offset));
				return CreateSequenceSection(range, sections);
			}

			HandlerBlock CreateHandlerBlock(ExceptionHandler handler)
			{
				var handlerSection = CreateSection(handler.HandlerRange);

				if (handler.Type == ExceptionHandlingClauseOptions.Filter)
				{
					var filterStart = new Label(handler.FilterOrType);
					var filterRange = new ILRange(filterStart, handler.HandlerRange.Offset - filterStart);

					return new HandlerBlock(
						handler.Type,
						CreateSection(filterRange),
						handlerSection);
				}
				else
				{
					return new HandlerBlock(
						handler.Type,
						new MetadataToken(handler.FilterOrType),
						handlerSection);
				}
			}

			static BaseSection CreateSequenceSection(ILRange range, List<BaseSection> sections)
			{
				var builder = ImmutableArray.CreateBuilder<BaseSection>();
				var offset = range.Offset;

				foreach (var section in sections)
				{
					if (section.Range.Offset > offset)
					{
						builder.Add(new ILSection(new ILRange(offset, section.Range.Offset - offset)));
					}

					builder.Add(section);
					offset = End(section.Range);
				}

				var end = End(range);

				if (offset < end)
				{
					builder.Add(new ILSection(new ILRange(offset, end - offset)));
				}

				return new SequenceSection(range, builder.ToImmutable());
			}

			List<BaseSection> ExtractSectionsInRange(ILRange range)
			{
				var builder = new List<BaseSection>();
				var write = 0;

				for (var read = 0; read < _pendingTryBlocks.Count; read++)
				{
					var item = _pendingTryBlocks[read];

					if (range.Contains(item.TryRange))
					{
						var newItem = item.Complete();

						if (!range.Contains(newItem.Range))
						{
							throw new CannotGenerateSectionException();
						}

						foreach (var other in builder)
						{
							if (newItem.Range.Overlaps(other.Range))
							{
								throw new CannotGenerateSectionException();
							}
						}

						builder.Add(newItem);
					}
					else
					{
						if (write != read)
						{
							_pendingTryBlocks[write] = item;
						}

						write++;
					}
				}

				_pendingTryBlocks.RemoveRange(write, _pendingTryBlocks.Count - write);
				return builder;
			}

			ImmutableArray<HandlerBlock>.Builder FindHandlersMatchingTryBlock(ILRange range)
			{
				foreach (var item in _pendingTryBlocks)
				{
					if (item.TryRange == range)
					{
						return item.Handlers;
					}
				}

				var builder = new TryBuilder(range, CreateSection(range));
				_pendingTryBlocks.Add(builder);
				return builder.Handlers;
			}

			static Label End(ILRange range) => range.Offset + range.Length;

			public void VerifyEmpty()
			{
				if (_pendingTryBlocks.Count > 0)
				{
					throw new CannotGenerateSectionException();
				}
			}

			readonly List<TryBuilder> _pendingTryBlocks;

			readonly struct TryBuilder(ILRange tryRange, BaseSection trySection)
			{
				public ILRange TryRange { get; } = tryRange;
				public BaseSection TrySection { get; } = trySection;
				public ImmutableArray<HandlerBlock>.Builder Handlers { get; } = ImmutableArray.CreateBuilder<HandlerBlock>();

				public TryBlockSection Complete()
				{
					var handlers = Handlers.ToImmutable();
					handlers.Sort((x, y) => x.HandlerSection.Range.Offset.CompareTo(y.HandlerSection.Range.Offset));

					var end = End(TrySection.Range);

					foreach (var handler in handlers)
					{
						var handlerEnd = End(handler.HandlerSection.Range);

						if (handler.FilterSection != null)
						{
							var filterRange = handler.FilterSection.Range;

							if (filterRange.Offset != end)
							{
								throw new CannotGenerateSectionException();
							}

							end = End(filterRange);
						}

						var handlerRange = handler.HandlerSection.Range;

						if (handlerRange.Offset != end)
						{
							throw new CannotGenerateSectionException();
						}

						end = End(handlerRange);
					}

					return new TryBlockSection(
						new ILRange(TryRange.Offset, end - TryRange.Offset),
						TrySection,
						handlers);
				}
			}
		}
	}
}
