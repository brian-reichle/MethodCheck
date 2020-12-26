// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using MethodCheck.Core.Data;
using MethodCheck.Core.Data.Sections;
using NUnit.Framework;

namespace MethodCheck.Test
{
	[TestFixture]
	class SectionFactoryTest
	{
		[Test]
		public void Empty()
		{
			const string Expected =
@"<il range=""IL_0000 (10)"" />";

			var section = SectionFactory.Create(new ILRange(0, 10), Array.Empty<ExceptionHandler>());

			Assert.That(Format(section), Is.EqualTo(Expected));
		}

		[Test]
		public void Catch()
		{
			const string Expected =
@"<try range=""IL_0000 (10)"">
  <try.block>
    <il range=""IL_0000 (6)"" />
  </try.block>
  <handler type=""Clause"" exception=""02000010"">
    <il range=""IL_0006 (4)"" />
  </handler>
</try>";

			var handlers = new[]
			{
				new ExceptionHandler(ExceptionHandlingClauseOptions.Clause, new ILRange(0, 6), new ILRange(6, 4), 0x02000010),
			};

			var section = SectionFactory.Create(new ILRange(0, 10), handlers);

			Assert.That(Format(section), Is.EqualTo(Expected));
		}

		[Test]
		public void Finally()
		{
			const string Expected =
@"<try range=""IL_0000 (10)"">
  <try.block>
    <il range=""IL_0000 (6)"" />
  </try.block>
  <handler type=""Finally"">
    <il range=""IL_0006 (4)"" />
  </handler>
</try>";

			var handlers = new[]
			{
				new ExceptionHandler(ExceptionHandlingClauseOptions.Finally, new ILRange(0, 6), new ILRange(6, 4), 0),
			};

			var section = SectionFactory.Create(new ILRange(0, 10), handlers);

			Assert.That(Format(section), Is.EqualTo(Expected));
		}

		[Test]
		public void Filter()
		{
			const string Expected =
@"<try range=""IL_0000 (12)"">
  <try.block>
    <il range=""IL_0000 (3)"" />
  </try.block>
  <handler type=""Filter"">
    <filter>
      <il range=""IL_0003 (4)"" />
    </filter>
    <il range=""IL_0007 (5)"" />
  </handler>
</try>";

			var handlers = new[]
			{
				new ExceptionHandler(ExceptionHandlingClauseOptions.Filter, new ILRange(0, 3), new ILRange(7, 5), 3),
			};

			var section = SectionFactory.Create(new ILRange(0, 12), handlers);

			Assert.That(Format(section), Is.EqualTo(Expected));
		}

		[Test]
		public void OverlappingTry()
		{
			const string Expected =
@"<try range=""IL_0000 (10)"">
  <try.block>
    <il range=""IL_0000 (2)"" />
  </try.block>
  <handler type=""Clause"" exception=""02000011"">
    <il range=""IL_0002 (4)"" />
  </handler>
  <handler type=""Clause"" exception=""02000012"">
    <il range=""IL_0006 (4)"" />
  </handler>
</try>";

			var handlers = new[]
			{
				new ExceptionHandler(ExceptionHandlingClauseOptions.Clause, new ILRange(0, 2), new ILRange(2, 4), 0x02000011),
				new ExceptionHandler(ExceptionHandlingClauseOptions.Clause, new ILRange(0, 2), new ILRange(6, 4), 0x02000012),
			};

			var section = SectionFactory.Create(new ILRange(0, 10), handlers);

			Assert.That(Format(section), Is.EqualTo(Expected));
		}

		[Test]
		public void Nested()
		{
			const string Expected =
@"<try range=""IL_0000 (20)"">
  <try.block>
    <try range=""IL_0000 (10)"">
      <try.block>
        <il range=""IL_0000 (5)"" />
      </try.block>
      <handler type=""Clause"" exception=""02000010"">
        <il range=""IL_0005 (5)"" />
      </handler>
    </try>
  </try.block>
  <handler type=""Finally"">
    <try range=""IL_000A (10)"">
      <try.block>
        <il range=""IL_000A (5)"" />
      </try.block>
      <handler type=""Clause"" exception=""02000010"">
        <il range=""IL_000F (5)"" />
      </handler>
    </try>
  </handler>
</try>";

			var handlers = new[]
			{
				new ExceptionHandler(ExceptionHandlingClauseOptions.Clause, new ILRange(0, 5), new ILRange(5, 5), 0x02000010),
				new ExceptionHandler(ExceptionHandlingClauseOptions.Clause, new ILRange(10, 5), new ILRange(15, 5), 0x02000010),
				new ExceptionHandler(ExceptionHandlingClauseOptions.Finally, new ILRange(0, 10), new ILRange(10, 10), 0),
			};

			var section = SectionFactory.Create(new ILRange(0, 20), handlers);

			Assert.That(Format(section), Is.EqualTo(Expected));
		}

		[Test]
		public void Sequence()
		{
			const string Expected =
@"<seq range=""IL_0000 (22)"">
  <il range=""IL_0000 (2)"" />
  <try range=""IL_0002 (8)"">
    <try.block>
      <il range=""IL_0002 (4)"" />
    </try.block>
    <handler type=""Clause"" exception=""02000010"">
      <il range=""IL_0006 (4)"" />
    </handler>
  </try>
  <il range=""IL_000A (2)"" />
  <try range=""IL_000C (8)"">
    <try.block>
      <il range=""IL_000C (4)"" />
    </try.block>
    <handler type=""Clause"" exception=""02000010"">
      <il range=""IL_0010 (4)"" />
    </handler>
  </try>
  <il range=""IL_0014 (2)"" />
</seq>";

			var handlers = new[]
			{
				new ExceptionHandler(ExceptionHandlingClauseOptions.Clause, new ILRange(2, 4), new ILRange(6, 4), 0x02000010),
				new ExceptionHandler(ExceptionHandlingClauseOptions.Clause, new ILRange(12, 4), new ILRange(16, 4), 0x02000010),
			};

			var section = SectionFactory.Create(new ILRange(0, 22), handlers);

			Assert.That(Format(section), Is.EqualTo(Expected));
		}

		[Test]
		public void NestedSequence()
		{
			const string Expected =
@"<try range=""IL_0000 (10)"">
  <try.block>
    <seq range=""IL_0000 (8)"">
      <il range=""IL_0000 (2)"" />
      <try range=""IL_0002 (4)"">
        <try.block>
          <il range=""IL_0002 (2)"" />
        </try.block>
        <handler type=""Clause"" exception=""02000010"">
          <il range=""IL_0004 (2)"" />
        </handler>
      </try>
      <il range=""IL_0006 (2)"" />
    </seq>
  </try.block>
  <handler type=""Finally"">
    <il range=""IL_0008 (2)"" />
  </handler>
</try>";

			var handlers = new[]
			{
				new ExceptionHandler(ExceptionHandlingClauseOptions.Clause, new ILRange(2, 2), new ILRange(4, 2), 0x02000010),
				new ExceptionHandler(ExceptionHandlingClauseOptions.Finally, new ILRange(0, 8), new ILRange(8, 2), 0),
			};

			var section = SectionFactory.Create(new ILRange(0, 10), handlers);

			Assert.That(Format(section), Is.EqualTo(Expected));
		}

		[TestCaseSource(nameof(InvalidHandlers))]
		public void InvalidHandlerConfigurations(ExceptionHandler[] handlers)
		{
			Assert.That(
				() => SectionFactory.Create(new ILRange(0, 10), handlers),
				Throws.InstanceOf<CannotGenerateSectionException>());
		}

		protected static IEnumerable<TestCaseData> InvalidHandlers()
		{
			const int ExceptionType = 0x02000010;

			var baseHandler = new ExceptionHandler(ExceptionHandlingClauseOptions.Clause, new ILRange(2, 4), new ILRange(6, 2), ExceptionType);

			yield return Data(
				"Not fully contained.",
				new ExceptionHandler(ExceptionHandlingClauseOptions.Clause, new ILRange(2, 2), new ILRange(8, 2), ExceptionType),
				baseHandler);

			yield return Data(
				"Non contiguous handlers.",
				baseHandler,
				new ExceptionHandler(ExceptionHandlingClauseOptions.Clause, new ILRange(2, 4), new ILRange(9, 1), ExceptionType));

			yield return Data(
				"Handler extends outside bounds.",
				new ExceptionHandler(ExceptionHandlingClauseOptions.Clause, new ILRange(2, 4), new ILRange(6, 5), ExceptionType));

			yield return Data(
				"Try outside bounds.",
				new ExceptionHandler(ExceptionHandlingClauseOptions.Clause, new ILRange(10, 2), new ILRange(12, 2), ExceptionType));

			static TestCaseData Data(string name, params ExceptionHandler[] handlers)
				=> new TestCaseData(new object[] { handlers }).SetName("{m}(" + name + ")");
		}

		static string Format(BaseSection section)
		{
			var settings = new XmlWriterSettings()
			{
				Indent = true,
				IndentChars = "  ",
				OmitXmlDeclaration = true,
			};

			using var writer = new StringWriter();
			using var xmlWriter = XmlWriter.Create(writer, settings);
			Write(xmlWriter, section);
			xmlWriter.Flush();
			return writer.ToString();
		}

		static void Write(XmlWriter writer, BaseSection section)
		{
			switch (section)
			{
				case ILSection ilSection:
					Write(writer, ilSection);
					break;

				case SequenceSection sequenceSection:
					Write(writer, sequenceSection);
					break;

				case TryBlockSection tryBlock:
					Write(writer, tryBlock);
					break;
			}
		}

		static void Write(XmlWriter writer, ILSection section)
		{
			writer.WriteStartElement("il");
			AppendRangeAttributes(writer, section);
			writer.WriteEndElement();
		}

		static void Write(XmlWriter writer, SequenceSection section)
		{
			writer.WriteStartElement("seq");
			AppendRangeAttributes(writer, section);

			foreach (var seq in section.Sections)
			{
				Write(writer, seq);
			}

			writer.WriteEndElement();
		}

		static void Write(XmlWriter writer, TryBlockSection section)
		{
			writer.WriteStartElement("try");
			AppendRangeAttributes(writer, section);
			writer.WriteStartElement("try.block");
			Write(writer, section.TryBlock);
			writer.WriteEndElement();

			foreach (var handler in section.HandlerBlocks)
			{
				writer.WriteStartElement("handler");
				writer.WriteAttributeString("type", handler.Type.ToString());

				if (handler.ExceptionType != default)
				{
					writer.WriteAttributeString("exception", handler.ExceptionType.ToString());
				}

				if (handler.FilterSection != null)
				{
					writer.WriteStartElement("filter");
					Write(writer, handler.FilterSection);
					writer.WriteEndElement();
				}

				Write(writer, handler.HandlerSection);
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
		}

		static void AppendRangeAttributes(XmlWriter writer, BaseSection ilSection)
		{
			writer.WriteAttributeString("range", ilSection.Range.ToString());
		}
	}
}
