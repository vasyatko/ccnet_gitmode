using System;
using Exortech.NetReflector;
using NMock;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core.Triggers;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
	[TestFixture]
	public class ProjectTriggerTest : IntegrationFixture
	{
		private IMock mockFactory;
		private IMock mockCruiseManager;
		private IMock mockInnerTrigger;
		private ProjectTrigger trigger;
		private DateTime now;
		private DateTime later;

		[SetUp]
		protected void SetUp()
		{
			now = DateTime.Now;
			later = now.AddHours(1);
			mockCruiseManager = new DynamicMock(typeof (ICruiseManager));
			mockFactory = new DynamicMock(typeof (ICruiseManagerFactory));
			mockInnerTrigger = new DynamicMock(typeof (ITrigger));
			trigger = new ProjectTrigger((ICruiseManagerFactory) mockFactory.MockInstance);
			trigger.Project = "project";
			trigger.InnerTrigger = (ITrigger) mockInnerTrigger.MockInstance;
		}

		protected void Verify()
		{
			mockFactory.Verify();
			mockCruiseManager.Verify();
			mockInnerTrigger.Verify();
		}

		[Test]
		public void ShouldNotTriggerOnFirstIntegration()
		{
			mockInnerTrigger.ExpectAndReturn("Fire", ModificationExistRequest());
			mockInnerTrigger.Expect("IntegrationCompleted");
			mockFactory.ExpectAndReturn("GetCruiseManager", mockCruiseManager.MockInstance, ProjectTrigger.DefaultServerUri);
			mockCruiseManager.ExpectAndReturn("GetProjectStatus", new ProjectStatus[]
				{
					NewProjectStatus("wrong", IntegrationStatus.Failure, now), NewProjectStatus("project", IntegrationStatus.Success, now)
				});
			Assert.IsNull(trigger.Fire());
			Verify();			
		}

		[Test]
		public void ShouldTriggerOnFirstIntegrationIfDependentProjectBuildSucceededAndTriggerFirstTimeIsSet()
		{
			mockInnerTrigger.ExpectAndReturn("Fire", ModificationExistRequest());
			mockInnerTrigger.Expect("IntegrationCompleted");
			mockFactory.ExpectAndReturn("GetCruiseManager", mockCruiseManager.MockInstance, ProjectTrigger.DefaultServerUri);
			mockCruiseManager.ExpectAndReturn("GetProjectStatus", new ProjectStatus[]
				{
					NewProjectStatus("project", IntegrationStatus.Success, now)
				});
			trigger.TriggerFirstTime = true;
			Assert.AreEqual(ModificationExistRequest(), trigger.Fire());
			Verify();
		}

		[Test]
		public void ShouldNotTriggerOnFirstIntegrationIfDependentProjectBuildFailedAndTriggerFirstTimeIsSet()
		{
			mockInnerTrigger.ExpectAndReturn("Fire", ModificationExistRequest());
			mockInnerTrigger.Expect("IntegrationCompleted");
			mockFactory.ExpectAndReturn("GetCruiseManager", mockCruiseManager.MockInstance, ProjectTrigger.DefaultServerUri);
			mockCruiseManager.ExpectAndReturn("GetProjectStatus", new ProjectStatus[]
				{
					NewProjectStatus("project", IntegrationStatus.Failure, now)
				});
			trigger.TriggerFirstTime = true;
			Assert.IsNull(trigger.Fire());
			Verify();
		}

		[Test]
		public void TriggerWhenDependentProjectBuildsSuccessfully()
		{
			ShouldNotTriggerOnFirstIntegration();
			mockInnerTrigger.ExpectAndReturn("Fire", ModificationExistRequest());
			mockInnerTrigger.Expect("IntegrationCompleted");
			mockFactory.ExpectAndReturn("GetCruiseManager", mockCruiseManager.MockInstance, ProjectTrigger.DefaultServerUri);
			mockCruiseManager.ExpectAndReturn("GetProjectStatus", new ProjectStatus[]
				{
					NewProjectStatus("wrong", IntegrationStatus.Failure, later), NewProjectStatus("project", IntegrationStatus.Success, later)
				});
			Assert.AreEqual(ModificationExistRequest(), trigger.Fire());
			Verify();
		}

		[Test]
		public void DoNotTriggerWhenInnerTriggerReturnsNoBuild()
		{
			mockInnerTrigger.ExpectAndReturn("Fire", null);
			mockFactory.ExpectNoCall("GetCruiseManager", typeof(string));
			mockCruiseManager.ExpectNoCall("GetProjectStatus");
			Assert.IsNull(trigger.Fire());
			Verify();
		}

		[Test]
		public void DoNotTriggerWhenDependentProjectBuildFails()
		{
			mockInnerTrigger.ExpectAndReturn("Fire", ModificationExistRequest());
			mockInnerTrigger.Expect("IntegrationCompleted");
			mockFactory.ExpectAndReturn("GetCruiseManager", mockCruiseManager.MockInstance, ProjectTrigger.DefaultServerUri);
			mockCruiseManager.ExpectAndReturn("GetProjectStatus", new ProjectStatus[] {NewProjectStatus("project", IntegrationStatus.Failure, now)});
			Assert.IsNull(trigger.Fire());
			Verify();
		}

		[Test]
		public void DoNotTriggerIfProjectHasNotBuiltSinceLastPoll()
		{
			ProjectStatus status = NewProjectStatus("project", IntegrationStatus.Success, now);
			mockInnerTrigger.ExpectAndReturn("Fire", ModificationExistRequest());
			mockInnerTrigger.Expect("IntegrationCompleted");
			mockFactory.ExpectAndReturn("GetCruiseManager", mockCruiseManager.MockInstance, ProjectTrigger.DefaultServerUri);
			mockCruiseManager.ExpectAndReturn("GetProjectStatus", new ProjectStatus[] {status});

			mockFactory.ExpectAndReturn("GetCruiseManager", mockCruiseManager.MockInstance, ProjectTrigger.DefaultServerUri);
			mockInnerTrigger.ExpectAndReturn("Fire", ModificationExistRequest());
			mockInnerTrigger.Expect("IntegrationCompleted");
			mockCruiseManager.ExpectAndReturn("GetProjectStatus", new ProjectStatus[] {status}); // same build as last time

			Assert.IsNull(trigger.Fire());
			Assert.IsNull(trigger.Fire());
			Verify();
		}

		[Test]
		public void IntegrationCompletedShouldDelegateToInnerTrigger()
		{
			mockInnerTrigger.Expect("IntegrationCompleted");
			trigger.IntegrationCompleted();
			Verify();
		}

		[Test]
		public void NextBuildShouldReturnInnerTriggerNextBuildIfUnknown()
		{
			mockInnerTrigger.ExpectAndReturn("NextBuild", now);
			Assert.AreEqual(now, trigger.NextBuild);
			Verify();
		}

		private ProjectStatus NewProjectStatus(string name, IntegrationStatus integrationStatus, DateTime dateTime)
		{
			return ProjectStatusFixture.New(name, integrationStatus, dateTime);
		}

		[Test]
		public void PopulateFromConfiguration()
		{
			string xml = @"<projectTrigger>
	<serverUri>http://fooserver:12342/CruiseManager.rem</serverUri>
	<project>Foo</project>
	<triggerStatus>Failure</triggerStatus>
	<triggerFirstTime>True</triggerFirstTime>
	<innerTrigger type=""intervalTrigger"">
		<buildCondition>ForceBuild</buildCondition>
		<seconds>10</seconds>
	</innerTrigger>
</projectTrigger>";
			trigger = (ProjectTrigger) NetReflector.Read(xml);
			Assert.AreEqual("http://fooserver:12342/CruiseManager.rem", trigger.ServerUri);
			Assert.AreEqual("Foo", trigger.Project);
			Assert.IsNotNull(trigger.InnerTrigger);
			Assert.AreEqual(IntegrationStatus.Failure, trigger.TriggerStatus);
			Assert.IsTrue(trigger.TriggerFirstTime);
		}

		[Test]
		public void PopulateFromMinimalConfiguration()
		{
			string xml = @"<projectTrigger><project>Foo</project></projectTrigger>";
			trigger = (ProjectTrigger) NetReflector.Read(xml);
			Assert.AreEqual("tcp://localhost:21234/CruiseManager.rem", trigger.ServerUri);
			Assert.AreEqual("Foo", trigger.Project);
			Assert.IsNotNull(trigger.InnerTrigger);
			Assert.AreEqual(IntegrationStatus.Success, trigger.TriggerStatus);
			Assert.IsFalse(trigger.TriggerFirstTime);
		}

		[Test, Ignore("not implemented yet.")]
		public void HandleExceptionInProjectLocator()
		{}
	}
}
