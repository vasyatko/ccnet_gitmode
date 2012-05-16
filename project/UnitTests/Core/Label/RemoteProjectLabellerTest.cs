using NMock;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Label;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Label
{
	[TestFixture]
	public class RemoteProjectLabellerTest
	{
		private IMock mockCruiseManager;
		private IMock mockRemotingService;
		private RemoteProjectLabeller labeller;

		[SetUp]
		protected void SetUp()
		{
			mockCruiseManager = new DynamicMock(typeof (ICruiseManager));
			mockCruiseManager.ExpectAndReturn("GetProjectStatus", new ProjectStatus[1] {NewProjectStatus("foo", "1")});

			mockRemotingService = new DynamicMock(typeof (IRemotingService));
			mockRemotingService.ExpectAndReturn("Connect", mockCruiseManager.MockInstance, typeof (ICruiseManager), RemoteCruiseServer.DefaultManagerUri);

			labeller = new RemoteProjectLabeller((IRemotingService) mockRemotingService.MockInstance);
		}

		[Test]
		public void ShouldConnectToRemoteServerAndRetrieveLabel()
		{
			labeller.ProjectName = "foo";
			Assert.AreEqual("1", labeller.Generate(IntegrationResultMother.CreateSuccessful()));
		}

		[Test]
		public void ShouldThrowExceptionIfProjectNameIsInvalid()
		{
			labeller.ProjectName = "invalid";
            Assert.That(delegate { labeller.Generate(IntegrationResultMother.CreateSuccessful()); },
                        Throws.TypeOf<NoSuchProjectException>());
		}

		private ProjectStatus NewProjectStatus(string projectName, string label)
		{
			return ProjectStatusFixture.New(projectName, label);
		}
	}
}