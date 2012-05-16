
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace ThoughtWorks.CruiseControl.Core.Util
{
	public class XslTransformer : ITransformer
	{
		public string Transform(string input, string xslFilename, Hashtable xsltArgs)
		{
			XslCompiledTransform transform = NewXslTransform(xslFilename);

			using (StringReader inputReader = new StringReader(input))
			{
				try
				{
					StringWriter output = new StringWriter();
					transform.Transform(new XPathDocument(inputReader), CreateXsltArgs(xsltArgs), output);
					return output.ToString();
				}
				catch (XmlException ex)
				{
					throw new CruiseControlException("Unable to execute transform: " + xslFilename, ex);
				}
			}
		}

		public string TransformToXml(string xslFilename, XPathDocument document)
		{
            XslCompiledTransform transform = NewXslTransform(xslFilename);
			try
			{
				StringWriter output = new StringWriter();
				XmlTextWriter xmlWriter = new XmlTextWriter(output);
				xmlWriter.Formatting = Formatting.Indented;
				transform.Transform(document, null, xmlWriter);
				return output.ToString();
			}
			catch (XmlException ex)
			{
				throw new CruiseControlException("Unable to execute transform: " + xslFilename, ex);
			}
		}

        private static XslCompiledTransform NewXslTransform(string transformerFileName)
		{
            XslCompiledTransform transform = new XslCompiledTransform();
			LoadStylesheet(transform, new SystemPath(transformerFileName).ToString());
			return transform;
		}

		private static XsltArgumentList CreateXsltArgs(Hashtable xsltArgs)
		{
			XsltArgumentList args = new XsltArgumentList();
			if (xsltArgs != null)
			{
				foreach (object key in xsltArgs.Keys)
				{
					args.AddParam(key.ToString(),string.Empty, xsltArgs[key]);
				}
			}
			return args;
		}

        private static void LoadStylesheet(XslCompiledTransform transform, string xslFileName)
		{
            XsltSettings settings = new XsltSettings(true, true);

            try
			{
				transform.Load(xslFileName,settings,new XmlUrlResolver());
			}
			catch (FileNotFoundException)
			{
				throw new CruiseControlException(string.Format("XSL stylesheet file not found: {0}", xslFileName));
			}
            catch (XsltException ex)
			{
				throw new CruiseControlException("Unable to load transform: " + xslFileName, ex);
			}
		}
	}
}
