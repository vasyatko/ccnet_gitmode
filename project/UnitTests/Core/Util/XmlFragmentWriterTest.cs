using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	[TestFixture]
	public class XmlFragmentWriterTest
	{
		private TextWriter baseWriter;
		private XmlFragmentWriter writer;

		[SetUp]
		public void CreateWriter()
		{
			baseWriter = new StringWriter();
			writer = new XmlFragmentWriter(baseWriter);
		}

		[Test]
		public void ShouldWriteValidXmlContentToUnderlyingWriter()
		{
			writer.WriteNode("<foo><bar /></foo>");
			Assert.AreEqual("<foo><bar /></foo>", baseWriter.ToString());
		}

		[Test]
		public void ShouldWriteInvalidXmlContentToUnderlyingWriterAsCData()
		{
			writer.WriteNode("<foo><bar></foo></bar>");
			Assert.AreEqual("<![CDATA[<foo><bar></foo></bar>]]>", baseWriter.ToString());
		}

		[Test]
		public void ShouldClearBufferIfInvalidXmlContentWrittenTwice()
		{
			writer.WriteNode("<foo><bar></foo></bar>");
			writer.WriteNode("<foo><bar/></foo>");
			Assert.AreEqual("<![CDATA[<foo><bar></foo></bar>]]><foo><bar /></foo>", baseWriter.ToString());
		}

		[Test]
		public void ShouldBeAbleToWriteWhenFragmentIsSurroundedByText()
		{
			writer.WriteNode("outside<foo/>text");
			Assert.AreEqual("outside<foo />text", baseWriter.ToString());
		}

		[Test]
		public void ShouldBeAbleToWriteWhenFragmentHasMultipleRootElements()
		{
			writer.WriteNode("<foo/><bar/>");
			Assert.AreEqual("<foo /><bar />", baseWriter.ToString());
		}

		[Test]
		public void ShouldIgnoreXmlDeclaration()
		{
			writer.WriteNode(@"<?xml version=""1.0"" encoding=""utf-16""?><foo/><bar/>");
			Assert.AreEqual("<foo /><bar />", baseWriter.ToString());
		}

		[Test]
		public void WriteOutputWithInvalidXmlContainingCDATACloseCommand()
		{
			writer.WriteNode("<tag><c>]]></tag>");
			Assert.AreEqual("<![CDATA[<tag><c>] ]></tag>]]>", baseWriter.ToString());
		}

		[Test]
		public void IndentOutputWhenFormattingIsSpecified()
		{
			writer.Formatting = Formatting.Indented;
			writer.WriteNode("<foo><bar/></foo>");
			Assert.AreEqual(@"<foo>
  <bar />
</foo>", baseWriter.ToString());
		}

		[Test]
		public void WriteTextContainingMalformedXmlElements()
		{
			string text = @"log4net:ERROR XmlConfigurator: Failed to find configuration section
'log4net' in the application's .config file. Check your .config file for the

<log4net> and <configSections> elements. The configuration section should
look like: <section name=""log4net""
type=""log4net.Config.Log4NetConfigurationSectionHandler,log4net"" />";
			writer.WriteNode(text);
			Assert.AreEqual(string.Format("<![CDATA[{0}]]>", text), baseWriter.ToString());
		}

		[Test]
		public void XmlWithoutClosingElementShouldEncloseInCDATA()
		{
			string text = "<a><b><c/></b>";
			writer.WriteNode(text);
			Assert.AreEqual(string.Format("<![CDATA[{0}]]>", text), baseWriter.ToString());
		}

		[Test]
		public void UnclosedXmlFragmentEndingInCarriageReturnShouldEncloseInCDATATag()
		{
			string xml = @"<a>
";
			writer.WriteNode(xml);
			Assert.AreEqual(@"<![CDATA[<a>
]]>", baseWriter.ToString());
		}

		[Test]
		public void ShouldStripIllegalCharacters()
		{
			writer.WriteNode(string.Format("<foo>{0}</foo>", IllegalCharacters()));
			Assert.AreEqual("<foo>\t\n\n</foo>", baseWriter.ToString());
		}

		[Test]
		public void ShouldStripIllegalCharactersFromCDATABlock()
		{
			writer.WriteNode("a <> b " + IllegalCharacters());
			Assert.AreEqual("<![CDATA[a <> b \t\n\r]]>", baseWriter.ToString());
		}

		[Test, Ignore("come back to this.")]
		public void ShouldIgnoreDTDEntities()
		{
			string xml = @"<?xml version=""1.0""?>
<!DOCTYPE Report
[
<!ELEMENT Report (General ,(Doc|BPT)) >
<!ATTLIST Report ver CDATA #REQUIRED tmZone CDATA #REQUIRED>

<!ELEMENT General ( DocLocation ) >
<!ATTLIST General productName CDATA #REQUIRED productVer CDATA #REQUIRED os CDATA #REQUIRED host CDATA #REQUIRED>
]
>
<Report ver=""2.0"" tmZone=""Pacific Standard Time"" />";
			writer.WriteNode(xml);
			Assert.AreEqual(@"<Report ver=""2.0"" tmZone=""Pacific Standard Time"" />", baseWriter.ToString());
		}
		
		private string IllegalCharacters()
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < 31; i++)
			{
				builder.Append((char) i, 1);
			}
			return builder.ToString();
		}
	}
}