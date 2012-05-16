﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Remote;
using Rhino.Mocks;
using ThoughtWorks.CruiseControl.Remote.Messages;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    [TestFixture]
    public class XmlConversionUtilTests
    {
        #region Private fields
        private MockRepository mocks = new MockRepository();
        #endregion

        #region Test methods
        #region FindMessageType()
        [Test]
        public void FindMessageTypeMatchesKnownXmlMessage()
        {
            Type messageType = XmlConversionUtil.FindMessageType("response");
            Assert.AreEqual(typeof(Response), messageType);
        }

        [Test]
        public void FindMessageTypeReturnsNullForUnknownXmlMessage()
        {
            Type messageType = XmlConversionUtil.FindMessageType("garbage");
            Assert.IsNull(messageType);
        }
        #endregion

        #region ConvertXmlToObject()
        [Test]
        public void ConvertXmlToObjectConvertsCorrectly()
        {
            string xml = string.Format("<response xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "timestamp=\"{1:yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz}\" result=\"{0}\" />",
                ResponseResult.Success,
                DateTime.Today);
            object result = XmlConversionUtil.ConvertXmlToObject(typeof(Response), xml);
            Assert.IsInstanceOfType(typeof(Response), result);
            Assert.AreEqual(xml, result.ToString());
        }
        #endregion

        #region ProcessResponse()
        [Test]
        public void ProcessResponseHandlesKnownMessage()
        {
            string xml = string.Format("<response xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "timestamp=\"{1:yyyy-MM-ddTHH:mm:ss.FFFFFFFzzz}\" result=\"{0}\" />",
                ResponseResult.Success,
                DateTime.Today);
            object result = XmlConversionUtil.ProcessResponse(xml);
            Assert.IsInstanceOfType(typeof(Response), result);
            Assert.AreEqual(xml, result.ToString());
        }

        [Test]
        public void ProcessResponseThrowsAnExceptionForUnknownMessage()
        {
            Assert.That(delegate { XmlConversionUtil.ProcessResponse("<garbage/>"); },
                        Throws.TypeOf<CommunicationsException>());
        }
        #endregion
        #endregion
    }
}
