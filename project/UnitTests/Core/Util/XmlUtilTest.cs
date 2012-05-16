using System.Xml;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	[TestFixture]
	public class XmlUtilTest : CustomAssertion
	{
		private static string TWO_SUCH_ELEMENTS = "two";
		private static string ONE_SUCH_ELEMENT = "one";
		private XmlDocument doc;
		private XmlElement elementOne;
		private XmlElement elementTwo;
		private XmlElement elementTwoAgain;
		
		[SetUp]
		public void SetUp()
		{
			doc = new XmlDocument();
			doc.AppendChild(doc.CreateElement("root"));

			elementOne = doc.CreateElement(ONE_SUCH_ELEMENT);
			elementTwo = doc.CreateElement(TWO_SUCH_ELEMENTS);
			elementTwoAgain = doc.CreateElement(TWO_SUCH_ELEMENTS);

			doc.DocumentElement.AppendChild(elementOne);
			doc.DocumentElement.AppendChild(elementTwo);
			doc.DocumentElement.AppendChild(elementTwoAgain);
		}

		[Test]
		public void GetFirstElement()
		{
			Assert.AreEqual(elementTwo, XmlUtil.GetFirstElement(doc, TWO_SUCH_ELEMENTS));
		}

		[Test]
		public void GetSingleElement()
		{
			Assert.That(delegate { XmlUtil.GetSingleElement(doc, ONE_SUCH_ELEMENT); },
                        Throws.Nothing);
			Assert.That(delegate { XmlUtil.GetSingleElement(doc, TWO_SUCH_ELEMENTS); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void SelectValue()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><monkeys>bananas</monkeys></configuration>");
			Assert.AreEqual("bananas", XmlUtil.SelectValue(document, "/configuration/monkeys", "orangutan"));			
		}

		[Test]
		public void SelectValueWithMissingValue()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><monkeys></monkeys></configuration>");
			Assert.AreEqual("orangutan", XmlUtil.SelectValue(document, "/configuration/monkeys", "orangutan"));			
		}

		[Test]
		public void SelectValueWithMissingElement()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><monkeys></monkeys></configuration>");
			Assert.AreEqual("orangutan", XmlUtil.SelectValue(document, "/configuration/apes", "orangutan"));			
		}

		[Test]
		public void SelectValueWithAttribute()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><monkeys part=\"brains\">booyah</monkeys></configuration>");
			Assert.AreEqual("brains", XmlUtil.SelectValue(document, "/configuration/monkeys/@part", "orangutan"));			
		}

		[Test]
		public void SelectRequiredValue()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><martin>andersen</martin></configuration>");
			Assert.AreEqual("andersen", XmlUtil.SelectRequiredValue(document, "/configuration/martin"));			
		}

		[Test]
		public void SelectRequiredValueWithMissingValue()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><martin></martin></configuration>");
            Assert.That(delegate { XmlUtil.SelectRequiredValue(document, "/configuration/martin"); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void SelectRequiredValueWithMissingElement()
		{
			XmlDocument document = XmlUtil.CreateDocument("<configuration><martin></martin></configuration>");
			Assert.That(delegate { XmlUtil.SelectRequiredValue(document, "/configuration/larry"); },
                        Throws.TypeOf<CruiseControlException>());
		}

		[Test]
		public void VerifyCDATAEncode()
		{
			string test = "a b <f>]]></a>";
			Assert.AreEqual("a b <f>] ]></a>", XmlUtil.EncodeCDATA(test));
		}

        [Test]
        public void VerifyPCDATAEncodeEncodes()
        {
            string test = "a&b <c>-</d>";
            Assert.AreEqual("a&amp;b &lt;c&gt;&#x2d;&lt;/d&gt;", XmlUtil.EncodePCDATA(test));
        }

        [Test]
        public void VerifyPCDATAEncodeDoesNotEncodeOthers()
        {
            string test = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890`~!@#$%^*()_+={[}]|\:;',.?/';" + '"';
            Assert.AreEqual(test, XmlUtil.EncodePCDATA(test));
        }
    }
}