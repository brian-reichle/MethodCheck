// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MethodCheck.Data;
using MethodCheck.Parsing;
using NUnit.Framework;

namespace MethodCheck.Test
{
	[TestFixture]
	class MethodFormatterTest
	{
		[Test]
		public void ParseAndFormat([ValueSource(nameof(Samples))] string name)
		{
			var source = Load(name);
			var match = Splitter.Match(source);
			Assert.That(match.Success, Is.True);

			var group = match.Groups["splitter"];
			var blob = BinaryProcessor.Parse(source.AsSpan(0, group.Index));

			MethodData data;

			if (name.StartsWith("Body."))
			{
				data = MethodParser.ParseBody(blob);
			}
			else if (name.StartsWith("IL."))
			{
				data = MethodParser.ParseIL(blob);
			}
			else
			{
				throw new ArgumentException();
			}

			var actual = MethodFormatter.Format(data);
			var expected = source.Substring(group.Index + group.Length);

			Assert.That(actual, Is.EqualTo(expected));
		}

		#region Implementation

		protected static IEnumerable<string> Samples
		{
			get
			{
				return
					from resource in typeof(MethodFormatterTest).Assembly.GetManifestResourceNames()
					where resource.StartsWith(ResourcePrefix)
					select resource.Substring(ResourcePrefix.Length);
			}
		}

		static string Load(string name)
		{
			using (var stream = typeof(MethodFormatterTest).Assembly.GetManifestResourceStream(ResourcePrefix + name))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		const string ResourcePrefix = "MethodCheck.Test.Samples.";
		static readonly Regex Splitter = new Regex(@"(?<splitter>===+\r?\n)", RegexOptions.ExplicitCapture | RegexOptions.Multiline);

		#endregion
	}
}
