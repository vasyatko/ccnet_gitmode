using NMock;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
using ThoughtWorks.CruiseControl.WebDashboard.IO;
using ThoughtWorks.CruiseControl.WebDashboard.MVC;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.DeleteProject;
using ThoughtWorks.CruiseControl.WebDashboard.ServerConnection;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Plugins.DeleteProject
{
	[TestFixture]
	public class DoDeleteProjectActionTest
	{
		private DynamicMock viewBuilderMock;
		private DynamicMock farmServiceMock;
		private DoDeleteProjectAction doDeleteProjectAction;

		private DynamicMock cruiseRequestMock;
		private DynamicMock requestMock;
		private ICruiseRequest cruiseRequest;

		[SetUp]
		public void Setup()
		{
			viewBuilderMock = new DynamicMock(typeof(IDeleteProjectViewBuilder));
			farmServiceMock = new DynamicMock(typeof(IFarmService));
			doDeleteProjectAction = new DoDeleteProjectAction((IDeleteProjectViewBuilder) viewBuilderMock.MockInstance, (IFarmService) farmServiceMock.MockInstance);

			cruiseRequestMock = new DynamicMock(typeof(ICruiseRequest));
			cruiseRequest = (ICruiseRequest) cruiseRequestMock.MockInstance;
			requestMock = new DynamicMock(typeof(IRequest));
			cruiseRequestMock.SetupResult("Request", requestMock.MockInstance);
		}

		private void VerifyAll()
		{
			viewBuilderMock.Verify();
			cruiseRequestMock.Verify();
			requestMock.Verify();
		}

		[Test]
		public void ShouldCallFarmServiceAndIfSuccessfulShowSuccessMessage()
		{
			IResponse response = new HtmlFragmentResponse("foo");
			// Setup
			IProjectSpecifier projectSpecifier = new DefaultProjectSpecifier(new DefaultServerSpecifier("myServer"), "myProject");
			cruiseRequestMock.ExpectAndReturn("ProjectSpecifier", projectSpecifier);
			requestMock.ExpectAndReturn("GetChecked", true, "PurgeWorkingDirectory");
			requestMock.ExpectAndReturn("GetChecked", false, "PurgeArtifactDirectory");
			requestMock.ExpectAndReturn("GetChecked", true, "PurgeSourceControlEnvironment");
			farmServiceMock.Expect("DeleteProject", projectSpecifier, true, false, true, null);
			string expectedMessage = "Project Deleted";
			viewBuilderMock.ExpectAndReturn("BuildView", response, new DeleteProjectModel(projectSpecifier, expectedMessage, false, true, false, true));

			// Execute
			IResponse returnedResponse = doDeleteProjectAction.Execute(cruiseRequest);

			// Verify
			Assert.AreEqual(response, returnedResponse);
			VerifyAll();
		}
	}
}
