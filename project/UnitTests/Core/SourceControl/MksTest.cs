using System;
using System.IO;
using Exortech.NetReflector;
using NMock;
using NMock.Constraints;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;
using System.Diagnostics;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	[TestFixture]
	public class MksTest : CustomAssertion
	{
		private static DateTime FROM = DateTime.Now.AddMinutes(-10);
		private static DateTime TO = DateTime.Now;

		private string sandboxRoot;

		private Mks mks;
		private IHistoryParser mockHistoryParser;
		private Mock mockHistoryParserWrapper;
		private Mock mockExecutorWrapper;
		private ProcessExecutor mockProcessExecutor;
		private Mock mockIntegrationResult;
		private IIntegrationResult integrationResult;
		private Mock mksHistoryParserWrapper;
		private MksHistoryParser mksHistoryParser;

		[SetUp]
		public void SetUp()
		{
			sandboxRoot = TempFileUtil.GetTempPath("MksSandBox");

			mockHistoryParserWrapper = new DynamicMock(typeof (IHistoryParser));
			mockHistoryParser = (IHistoryParser) mockHistoryParserWrapper.MockInstance;

			mksHistoryParserWrapper = new DynamicMock(typeof (MksHistoryParser));
			mksHistoryParser = (MksHistoryParser) mksHistoryParserWrapper.MockInstance;

			mockExecutorWrapper = new DynamicMock(typeof (ProcessExecutor));
			mockProcessExecutor = (ProcessExecutor) mockExecutorWrapper.MockInstance;

			mockIntegrationResult = new DynamicMock(typeof (IIntegrationResult));
			integrationResult = (IIntegrationResult) mockIntegrationResult.MockInstance;
		}

		[TearDown]
		public void TearDown()
		{
			mockExecutorWrapper.Verify();
			mockHistoryParserWrapper.Verify();
			mksHistoryParserWrapper.Verify();
			mockIntegrationResult.Verify();
		}

		private string CreateSourceControlXml()
		{
			return string.Format(
				@"    <sourceControl type=""mks"">
						  <executable>..\bin\si.exe</executable>
						  <port>8722</port>
						  <hostname>hostname</hostname>
						  <user>CCNetUser</user>
						  <password>CCNetPassword</password>
						  <sandboxroot>{0}</sandboxroot>
						  <sandboxfile>myproject.pj</sandboxfile>
						  <autoGetSource>true</autoGetSource>
						  <checkpointOnSuccess>true</checkpointOnSuccess>
						  <autoDisconnect>true</autoDisconnect>
					  </sourceControl>
				 ", sandboxRoot);
		}

		[Test]
		public void CheckDefaults()
		{
			Mks defalutMks = new Mks();
			Assert.AreEqual(@"si.exe", defalutMks.Executable);
			Assert.AreEqual(8722, defalutMks.Port);
			Assert.AreEqual(true, defalutMks.AutoGetSource);
			Assert.AreEqual(false, defalutMks.CheckpointOnSuccess);
			Assert.AreEqual(false, defalutMks.AutoDisconnect);
		}

		[Test]
		public void ValuePopulation()
		{
			mks = CreateMks(CreateSourceControlXml(), null, null);

			Assert.AreEqual(@"..\bin\si.exe", mks.Executable);
			Assert.AreEqual(@"hostname", mks.Hostname);
			Assert.AreEqual(8722, mks.Port);
			Assert.AreEqual(@"CCNetUser", mks.User);
			Assert.AreEqual(@"CCNetPassword", mks.Password);
			Assert.AreEqual(sandboxRoot, mks.SandboxRoot);
			Assert.AreEqual(@"myproject.pj", mks.SandboxFile);
			Assert.AreEqual(true, mks.AutoGetSource);
			Assert.AreEqual(true, mks.CheckpointOnSuccess);
			Assert.AreEqual(true, mks.AutoDisconnect);
		}

		[Test]
		public void GetSource()
		{
            string expectedResyncCommand = string.Format(@"resync --overwriteChanged --restoreTimestamp --forceConfirm=yes --includeDropped -R -S {0} --user=CCNetUser --password=CCNetPassword --quiet", 
                GeneratePath(@"{0}\myproject.pj", sandboxRoot));
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), ExpectedProcessInfo(expectedResyncCommand));
			string expectedAttribCommand = string.Format(@"-R /s {0}", 
                GeneratePath(@"{0}\*", sandboxRoot));
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), ExpectedProcessInfo("attrib", expectedAttribCommand));

			string expectedDisconnectCommand = string.Format(@"disconnect --user=CCNetUser --password=CCNetPassword --quiet --forceConfirm=yes");
			ProcessInfo expectedDisconnectProcessInfo = ExpectedProcessInfo(expectedDisconnectCommand);
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), expectedDisconnectProcessInfo);
			
			mks = CreateMks(CreateSourceControlXml(), mockHistoryParser, mockProcessExecutor);
            mks.GetSource(new IntegrationResult());
		}

		[Test]
		public void GetSourceWithSpacesInSandbox()
		{
			sandboxRoot = TempFileUtil.GetTempPath("Mks Sand Box");
            string expectedResyncCommand = string.Format(@"resync --overwriteChanged --restoreTimestamp --forceConfirm=yes --includeDropped -R -S ""{0}\myproject.pj"" --user=CCNetUser --password=CCNetPassword --quiet", sandboxRoot);
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), ExpectedProcessInfo(expectedResyncCommand));
			
			string expectedAttribCommand = string.Format(@"-R /s ""{0}\*""", sandboxRoot);
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), ExpectedProcessInfo("attrib", expectedAttribCommand));
			
			string expectedDisconnectCommand = string.Format(@"disconnect --user=CCNetUser --password=CCNetPassword --quiet --forceConfirm=yes");
			ProcessInfo expectedDisconnectProcessInfo = ExpectedProcessInfo(expectedDisconnectCommand);
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), expectedDisconnectProcessInfo);
			
			mks = CreateMks(CreateSourceControlXml(), mockHistoryParser, mockProcessExecutor);
            mks.GetSource(new IntegrationResult());
		}

		[Test]
		public void CheckpointSourceOnSuccessfulBuild()
		{
            string path = GeneratePath(@"{0}\myproject.pj", sandboxRoot);
			string expectedCommand = string.Format(@"checkpoint -d ""Cruise Control.Net Build - 20"" -L ""Build - 20"" -R -S {0} --user=CCNetUser --password=CCNetPassword --quiet", path);
			ProcessInfo expectedProcessInfo = ExpectedProcessInfo(expectedCommand);
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), expectedProcessInfo);
			
			string expectedDisconnectCommand = string.Format(@"disconnect --user=CCNetUser --password=CCNetPassword --quiet --forceConfirm=yes");
			ProcessInfo expectedDisconnectProcessInfo = ExpectedProcessInfo(expectedDisconnectCommand);
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), expectedDisconnectProcessInfo);
			
			mockIntegrationResult.ExpectAndReturn("Succeeded", true);
			mockIntegrationResult.ExpectAndReturn("Label", "20");
			
			mks = CreateMks(CreateSourceControlXml(), mockHistoryParser, mockProcessExecutor);
			mks.LabelSourceControl(integrationResult);
		}

		[Test]
		public void CheckpointSourceOnUnSuccessfulBuild()
		{
			string expectedDisconnectCommand = string.Format(@"disconnect --user=CCNetUser --password=CCNetPassword --quiet --forceConfirm=yes");
			ProcessInfo expectedDisconnectProcessInfo = ExpectedProcessInfo(expectedDisconnectCommand);
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), expectedDisconnectProcessInfo);
			
			mockIntegrationResult.ExpectAndReturn("Succeeded", false);
			mockIntegrationResult.ExpectNoCall("Label", typeof (string));
			
			mks = CreateMks(CreateSourceControlXml(), mockHistoryParser, mockProcessExecutor);
			mks.LabelSourceControl(integrationResult);
		}

		[Test]
		public void GetModificationsCallsParseOnHistoryParser()
		{
			mksHistoryParserWrapper.ExpectAndReturn("Parse", new Modification[0], new IsTypeOf(typeof (TextReader)), FROM, TO);
			mksHistoryParserWrapper.ExpectNoCall("ParseMemberInfoAndAddToModification", new Type[] {(typeof (Modification)), typeof (StringReader)});
			
            ProcessInfo expectedProcessInfo = ExpectedProcessInfo(string.Format(@"viewsandbox --nopersist --filter=changed:all --xmlapi -R -S {0} --user=CCNetUser --password=CCNetPassword --quiet", 
                GeneratePath(@"{0}\myproject.pj", sandboxRoot)));
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), expectedProcessInfo);
			
			string expectedDisconnectCommand = string.Format(@"disconnect --user=CCNetUser --password=CCNetPassword --quiet --forceConfirm=yes");
			ProcessInfo expectedDisconnectProcessInfo = ExpectedProcessInfo(expectedDisconnectCommand);
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), expectedDisconnectProcessInfo);
			
			mks = CreateMks(CreateSourceControlXml(), mksHistoryParser, mockProcessExecutor);
			Modification[] modifications = mks.GetModifications(IntegrationResultMother.CreateSuccessful(FROM), IntegrationResultMother.CreateSuccessful(TO));
			Assert.AreEqual(0, modifications.Length);
		}

		[Test]
		public void GetModificationsCallsParseMemberInfo()
		{
			Modification addedModification = ModificationMother.CreateModification("myFile.file", "MyFolder");
			addedModification.Type = "Added";

			mksHistoryParserWrapper.ExpectAndReturn("Parse", new Modification[] {addedModification}, new IsTypeOf(typeof (TextReader)), FROM, TO);
			mksHistoryParserWrapper.ExpectAndReturn("ParseMemberInfoAndAddToModification", new Modification[] {addedModification}, new IsTypeOf(typeof (Modification)), new IsTypeOf(typeof (StringReader)));
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult("", null, 0, false), new IsTypeOf(typeof (ProcessInfo)));

            string expectedCommand = string.Format(@"memberinfo --xmlapi --user=CCNetUser --password=CCNetPassword --quiet {0}", 
                GeneratePath(@"{0}\MyFolder\myFile.file", sandboxRoot));
			ProcessInfo expectedProcessInfo = ExpectedProcessInfo(expectedCommand);
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), expectedProcessInfo);
			
			string expectedDisconnectCommand = string.Format(@"disconnect --user=CCNetUser --password=CCNetPassword --quiet --forceConfirm=yes");
			ProcessInfo expectedDisconnectProcessInfo = ExpectedProcessInfo(expectedDisconnectCommand);
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), expectedDisconnectProcessInfo);
			
			mks = CreateMks(CreateSourceControlXml(), mksHistoryParser, mockProcessExecutor);
			Modification[] modifications = mks.GetModifications(IntegrationResultMother.CreateSuccessful(FROM), IntegrationResultMother.CreateSuccessful(TO));
			Assert.AreEqual(1, modifications.Length);
		}

		[Test]
		public void GetModificationsForModificationInRootFolder()
		{
			sandboxRoot = TempFileUtil.GetTempPath("MksSandBox");
			
			Modification addedModification = ModificationMother.CreateModification("myFile.file", null);
			addedModification.Type = "Added";

			mksHistoryParserWrapper.ExpectAndReturn("Parse", new Modification[] {addedModification}, new IsTypeOf(typeof (TextReader)), FROM, TO);
			mksHistoryParserWrapper.ExpectAndReturn("ParseMemberInfoAndAddToModification", new Modification[] {addedModification}, new IsTypeOf(typeof (Modification)), new IsTypeOf(typeof (StringReader)));
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult("", null, 0, false), new IsTypeOf(typeof (ProcessInfo)));

			string expectedCommand = string.Format(@"memberinfo --xmlapi --user=CCNetUser --password=CCNetPassword --quiet {0}", 
                GeneratePath(@"{0}\myFile.file", sandboxRoot));
			ProcessInfo expectedProcessInfo = ExpectedProcessInfo(expectedCommand);
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), expectedProcessInfo);

			string expectedDisconnectCommand = string.Format(@"disconnect --user=CCNetUser --password=CCNetPassword --quiet --forceConfirm=yes");
			ProcessInfo expectedDisconnectProcessInfo = ExpectedProcessInfo(expectedDisconnectCommand);
			mockExecutorWrapper.ExpectAndReturn("Execute", new ProcessResult(null, null, 0, false), expectedDisconnectProcessInfo);
			
			mks = CreateMks(CreateSourceControlXml(), mksHistoryParser, mockProcessExecutor);
			Modification[] modifications = mks.GetModifications(IntegrationResultMother.CreateSuccessful(FROM), IntegrationResultMother.CreateSuccessful(TO));
			Assert.AreEqual(1, modifications.Length);
		}

		[Test]
		public void GetModificationsCallsMemberInfoForNonDeletedModifications()
		{
			Modification addedModification = ModificationMother.CreateModification("myFile.file", "MyFolder");
			addedModification.Type = "Added";

			Modification modifiedModification = ModificationMother.CreateModification("myFile.file", "MyFolder");
			modifiedModification.Type = "Modified";

			Modification deletedModification = ModificationMother.CreateModification("myFile.file", "MyFolder");
			deletedModification.Type = "Deleted";

			mksHistoryParserWrapper.ExpectAndReturn("Parse", new Modification[] {addedModification, modifiedModification, deletedModification}, new IsTypeOf(typeof (TextReader)), FROM, TO);
			mksHistoryParserWrapper.ExpectAndReturn("ParseMemberInfoAndAddToModification", null, addedModification, new IsTypeOf(typeof(StringReader)));
			mksHistoryParserWrapper.ExpectAndReturn("ParseMemberInfoAndAddToModification", null, modifiedModification, new IsTypeOf(typeof(StringReader)));
            mksHistoryParserWrapper.ExpectAndReturn("ParseMemberInfoAndAddToModification", null, deletedModification, new IsTypeOf(typeof(StringReader)));
			mockExecutorWrapper.SetupResult("Execute", new ProcessResult("", null, 0, false), new Type[]{typeof (ProcessInfo)});

			mks = CreateMks(CreateSourceControlXml(), mksHistoryParser, mockProcessExecutor);
			Modification[] modifications = mks.GetModifications(IntegrationResultMother.CreateSuccessful(FROM), IntegrationResultMother.CreateSuccessful(TO));
			Assert.AreEqual(3, modifications.Length);
		}

		[Test]
		public void GetModificationsFiltersByModifiedTimeIfCheckpointOnSuccessIsFalse()
		{
			Modification modificationBeforePreviousIntegration = ModificationMother.CreateModification("ccnet", FROM.AddMinutes(-2));
			Modification modificationInThisIntegration = ModificationMother.CreateModification("ccnet", TO.AddMinutes(-1));
			Modification modificationAfterIntegrationStartTime = ModificationMother.CreateModification("myFile.file", TO.AddMinutes(1));

			Modification[] integrationModifications = new Modification[] {modificationBeforePreviousIntegration, modificationInThisIntegration, modificationAfterIntegrationStartTime};
			mksHistoryParserWrapper.ExpectAndReturn("Parse", integrationModifications, new IsTypeOf(typeof (TextReader)), FROM, TO);
			mksHistoryParserWrapper.ExpectAndReturn("ParseMemberInfoAndAddToModification", null, modificationBeforePreviousIntegration, new IsTypeOf(typeof(StringReader)));
			mksHistoryParserWrapper.ExpectAndReturn("ParseMemberInfoAndAddToModification", null, modificationInThisIntegration, new IsTypeOf(typeof(StringReader)));
			mksHistoryParserWrapper.ExpectAndReturn("ParseMemberInfoAndAddToModification", null, modificationAfterIntegrationStartTime, new IsTypeOf(typeof(StringReader)));
			mockExecutorWrapper.SetupResult("Execute", new ProcessResult("", null, 0, false), new Type[]{typeof (ProcessInfo)});

			mks = CreateMks(CreateSourceControlXml(), mksHistoryParser, mockProcessExecutor);
			mks.CheckpointOnSuccess = false;
			Modification[] modifications = mks.GetModifications(IntegrationResultMother.CreateSuccessful(FROM), IntegrationResultMother.CreateSuccessful(TO));
			Assert.AreEqual(1, modifications.Length);
		}

		private static Mks CreateMks(string xml, IHistoryParser historyParser, ProcessExecutor executor)
		{
			Mks newMks = new Mks(historyParser, executor);
			NetReflector.Read(xml, newMks);
			return newMks;
		}

		private static ProcessInfo ExpectedProcessInfo(string arguments)
		{
			return ExpectedProcessInfo(@"..\bin\si.exe", arguments);
		}

		private static ProcessInfo ExpectedProcessInfo(string executable, string arguments)
		{
			ProcessInfo expectedProcessInfo = new ProcessInfo(executable, arguments);
			expectedProcessInfo.TimeOut = Timeout.DefaultTimeout.Millis;
			return expectedProcessInfo;
		}

        /// <summary>
        /// Path generation hack to text whether the desired path contains spaces.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is required because some environments contain spaces for their temp paths (e.g. WinXP), 
        /// other don't (e.g. WinVista). Previously the unit tests would fail between the different
        /// environments just because of this.
        /// </remarks>
        private string GeneratePath(string path, params string[] args)
        {
            string basePath = string.Format(path, args);
            if (basePath.Contains(" ")) basePath = "\"" + basePath + "\"";
            return basePath;
        }
	}
}