using Exortech.NetReflector;
using NMock;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;
using System;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	/// <remarks>
	/// This code is based on code\sourcecontrol\ClearCase.cs.
	/// </remarks>
	[TestFixture]
	public class AccuRevTest
	{
		[Test]
		public void ShouldPopulateCorrectlyFromXml()
		{
			const string AccuRev_XML =
@"<sourceControl type=""accuRev"">
    <autoGetSource>false</autoGetSource>
    <executable>accurev.exe</executable>
    <labelOnSuccess>true</labelOnSuccess>
    <workspace>C:\DOES NOT\EXIST</workspace>
</sourceControl>";
			
			AccuRev accurev = new AccuRev();
			NetReflector.Read(AccuRev_XML, accurev);
			Assert.AreEqual(false, accurev.AutoGetSource);
			Assert.AreEqual("accurev.exe", accurev.Executable);
			Assert.AreEqual(true, accurev.LabelOnSuccess);
			Assert.AreEqual(@"C:\DOES NOT\EXIST", accurev.Workspace);
		}

        [Test]
        public void CanCatchConfigInvalidAutoGetSource()
        {
            AccuRev accurev = new AccuRev();
            const string invalidXml =
@"<sourcecontrol type=""accurev"">
    <autoGetSource>NOT_A_BOOLEAN</autoGetSource>
</sourcecontrol>";
            Assert.That(delegate { NetReflector.Read(invalidXml, accurev); },
                        Throws.TypeOf<NetReflectorConverterException>());
        }

        [Test]
        public void CanCatchConfigInvalidLabelOnSuccess()
        {
            AccuRev accurev = new AccuRev();
            const string invalidXml =
@"<sourcecontrol type=""accurev"">
    <labelOnSuccess>NOT_A_BOOLEAN</labelOnSuccess>
</sourcecontrol>";
            Assert.That(delegate { NetReflector.Read(invalidXml, accurev); },
                        Throws.TypeOf<NetReflectorConverterException>());
        }

		[Test]
		public void ShouldGetSourceIfAutoGetSourceTrue()
		{
			DynamicMock executor = new DynamicMock(typeof(ProcessExecutor));
			AccuRev accurev = new AccuRev((ProcessExecutor) executor.MockInstance);
			accurev.AutoGetSource = true;

			ProcessInfo expectedProcessRequest = new ProcessInfo("accurev.exe", "update");
			expectedProcessRequest.TimeOut = Timeout.DefaultTimeout.Millis;

			executor.ExpectAndReturn("Execute", new ProcessResult("foo", null, 0, false), expectedProcessRequest);
			accurev.GetSource(new IntegrationResult());
			executor.Verify();
		}

		[Test]
		public void ShouldNotGetSourceIfAutoGetSourceFalse()
		{
			DynamicMock executor = new DynamicMock(typeof(ProcessExecutor));
			AccuRev accurev = new AccuRev((ProcessExecutor) executor.MockInstance);
			accurev.AutoGetSource = false;

			executor.ExpectNoCall("Execute", typeof(ProcessInfo));
			accurev.GetSource(new IntegrationResult());
			executor.Verify();
		}

        [Test]
        public void ShouldUpdateSourceToHighestKnownModification()
        {
            DynamicMock executor = new DynamicMock(typeof(ProcessExecutor));
            AccuRev accurev = new AccuRev((ProcessExecutor)executor.MockInstance);
            accurev.AutoGetSource = true;

            ProcessInfo expectedProcessRequest = new ProcessInfo("accurev.exe", "update -t 10");
            expectedProcessRequest.TimeOut = Timeout.DefaultTimeout.Millis;

            executor.ExpectAndReturn("Execute", new ProcessResult("foo", null, 0, false), expectedProcessRequest);
            IntegrationResult result = new IntegrationResult();
            result.Modifications = new Modification[2];
            result.Modifications[0] = new Modification
            {
                ChangeNumber = "5",
                ModifiedTime = new DateTime(2009, 1, 1)
            };
            result.Modifications[1] = new Modification
            {
                ChangeNumber = "10",
                ModifiedTime = new DateTime(2009, 1, 2)
            };
            accurev.mods = result.Modifications;

            accurev.GetSource(result);
            executor.Verify();
        }

        [Test]
        public void ShouldUpdateSourceToCurrentIfNoModifications()
        {
            DynamicMock executor = new DynamicMock(typeof(ProcessExecutor));
            AccuRev accurev = new AccuRev((ProcessExecutor)executor.MockInstance);
            accurev.AutoGetSource = true;

            ProcessInfo expectedProcessRequest = new ProcessInfo("accurev.exe", "update");  // Note: No "-t whatever"
            expectedProcessRequest.TimeOut = Timeout.DefaultTimeout.Millis;

            executor.ExpectAndReturn("Execute", new ProcessResult("foo", null, 0, false), expectedProcessRequest);
            IntegrationResult result = new IntegrationResult();
            accurev.GetSource(result);
            executor.Verify();
        }
    }
}
