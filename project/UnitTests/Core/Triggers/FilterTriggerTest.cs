using System;
using Exortech.NetReflector;
using NMock;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core.Triggers;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
	[TestFixture]
	public class FilterTriggerTest : IntegrationFixture
	{
		private IMock mockTrigger;
		private IMock mockDateTime;
		private FilterTrigger trigger;

		[SetUp]
		protected void CreateMocksAndInitialiseObjectUnderTest()
		{
			mockTrigger = new DynamicMock(typeof (ITrigger));
			mockDateTime = new DynamicMock(typeof (DateTimeProvider));

			trigger = new FilterTrigger((DateTimeProvider) mockDateTime.MockInstance);
			trigger.InnerTrigger = (ITrigger) mockTrigger.MockInstance;
			trigger.StartTime = "10:00";
			trigger.EndTime = "11:00";
			trigger.WeekDays = new DayOfWeek[] {DayOfWeek.Wednesday};
			trigger.BuildCondition = BuildCondition.NoBuild;
		}

		[TearDown]
		protected void VerifyMocks()
		{
			mockDateTime.Verify();
			mockTrigger.Verify();
		}

		[Test]
		public void ShouldNotInvokeDecoratedTriggerWhenTimeAndWeekDayMatch()
		{
			mockTrigger.ExpectNoCall("Fire");
			mockDateTime.ExpectAndReturn("Now", new DateTime(2004, 12, 1, 10, 30, 0, 0));

            Assert.IsNull(trigger.Fire(), "trigger.Fire()");
		}

		[Test]
		public void ShouldNotInvokeDecoratedTriggerWhenWeekDaysNotSpecified()
		{
			mockTrigger.ExpectNoCall("Fire");
			mockDateTime.ExpectAndReturn("Now", new DateTime(2004, 12, 1, 10, 30, 0, 0));
			trigger.WeekDays = new DayOfWeek[] {};

            Assert.IsNull(trigger.Fire(), "trigger.Fire()");
		}

		[Test]
		public void ShouldInvokeDecoratedTriggerWhenTimeIsOutsideOfRange()
		{
			mockTrigger.ExpectAndReturn("Fire", ModificationExistRequest());
			mockDateTime.ExpectAndReturn("Now", new DateTime(2004, 12, 1, 11, 30, 0, 0));

            Assert.AreEqual(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");
		}

		[Test]
		public void ShouldNotInvokeOverMidnightTriggerWhenCurrentTimeIsBeforeMidnight()
		{
			trigger.StartTime = "23:00";
			trigger.EndTime = "7:00";

			mockTrigger.ExpectNoCall("Fire");
			mockDateTime.ExpectAndReturn("Now", new DateTime(2004, 12, 1, 23, 30, 0, 0));

            Assert.IsNull(trigger.Fire(), "trigger.Fire()");
		}

		[Test]
		public void ShouldNotInvokeOverMidnightTriggerWhenCurrentTimeIsAfterMidnight()
		{
			trigger.StartTime = "23:00";
			trigger.EndTime = "7:00";

			mockTrigger.ExpectNoCall("Fire");
			mockDateTime.ExpectAndReturn("Now", new DateTime(2004, 12, 1, 00, 30, 0, 0));

            Assert.IsNull(trigger.Fire(), "trigger.Fire()");
		}

		[Test]
		public void ShouldInvokeOverMidnightTriggerWhenCurrentTimeIsOutsideOfRange()
		{
			trigger.StartTime = "23:00";
			trigger.EndTime = "7:00";

			mockTrigger.ExpectAndReturn("Fire", ModificationExistRequest());
			mockDateTime.ExpectAndReturn("Now", new DateTime(2004, 12, 1, 11, 30, 0, 0));

            Assert.AreEqual(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");
		}

		[Test]
		public void ShouldNotInvokeDecoratedTriggerWhenTimeIsEqualToStartTimeOrEndTime()
		{
			mockTrigger.ExpectNoCall("Fire");
			mockDateTime.ExpectAndReturn("Now", new DateTime(2004, 12, 1, 10, 00, 0, 0));
			mockDateTime.ExpectAndReturn("Now", new DateTime(2004, 12, 1, 11, 00, 0, 0));

            Assert.IsNull(trigger.Fire(), "trigger.Fire()");
            Assert.IsNull(trigger.Fire(), "trigger.Fire()");
		}

		[Test]
		public void ShouldNotInvokeDecoratedTriggerWhenTodayIsOneOfSpecifiedWeekdays()
		{
			mockTrigger.ExpectAndReturn("Fire", ModificationExistRequest());
			mockDateTime.ExpectAndReturn("Now", new DateTime(2004, 12, 2, 10, 30, 0, 0));

            Assert.AreEqual(ModificationExistRequest(), trigger.Fire(), "trigger.Fire()");
		}

		[Test]
		public void ShouldDelegateIntegrationCompletedCallToInnerTrigger()
		{
			mockTrigger.Expect("IntegrationCompleted");
			trigger.IntegrationCompleted();
		}

		[Test]
		public void ShouldUseFilterEndTimeIfTriggerBuildTimeIsInFilter()
		{
			DateTime triggerNextBuildTime = new DateTime(2004, 12, 1, 10, 30, 00);
			mockTrigger.SetupResult("NextBuild", triggerNextBuildTime);
            Assert.AreEqual(new DateTime(2004, 12, 1, 11, 00, 00), trigger.NextBuild, "trigger.NextBuild");
		}

		[Test]
		public void ShouldNotFilterIfTriggerBuildDayIsNotInFilter()
		{
			DateTime triggerNextBuildTime = new DateTime(2004, 12, 4, 10, 00, 00);
			mockTrigger.SetupResult("NextBuild", triggerNextBuildTime);
            Assert.AreEqual(triggerNextBuildTime, trigger.NextBuild, "trigger.NextBuild");
		}

		[Test]
		public void ShouldNotFilterIfTriggerBuildTimeIsNotInFilter()
		{
			DateTime nextBuildTime = new DateTime(2004, 12, 1, 13, 30, 00);
			mockTrigger.ExpectAndReturn("NextBuild", nextBuildTime);
            Assert.AreEqual(nextBuildTime, trigger.NextBuild, "trigger.NextBuild");
		}

		[Test]
		public void ShouldFullyPopulateFromReflector()
		{
			string xml =
				string.Format(
					@"<filterTrigger startTime=""8:30:30"" endTime=""22:30:30"" buildCondition=""ForceBuild"">
											<trigger type=""scheduleTrigger"" time=""12:00:00""/>
											<weekDays>
												<weekDay>Monday</weekDay>
												<weekDay>Tuesday</weekDay>
											</weekDays>
										</filterTrigger>");
			trigger = (FilterTrigger) NetReflector.Read(xml);
            Assert.AreEqual("08:30:30", trigger.StartTime, "trigger.StartTime");
            Assert.AreEqual("22:30:30", trigger.EndTime, "trigger.EndTime");
			Assert.AreEqual(typeof (ScheduleTrigger), trigger.InnerTrigger.GetType(), "trigger.InnerTrigger type");
			Assert.AreEqual(DayOfWeek.Monday, trigger.WeekDays[0], "trigger.WeekDays[0]");
			Assert.AreEqual(DayOfWeek.Tuesday, trigger.WeekDays[1], "trigger.WeekDays[1]");
			Assert.AreEqual(BuildCondition.ForceBuild, trigger.BuildCondition, "trigger.BuildCondition");
		}

		[Test]
		public void ShouldMinimallyPopulateFromReflector()
		{
			string xml =
				string.Format(
					@"<filterTrigger>
											<trigger type=""scheduleTrigger"" time=""12:00:00"" />
										</filterTrigger>");
			trigger = (FilterTrigger) NetReflector.Read(xml);
            Assert.AreEqual("00:00:00", trigger.StartTime, "trigger.StartTime");
            Assert.AreEqual("23:59:59", trigger.EndTime, "trigger.EndTime");
            Assert.AreEqual(typeof(ScheduleTrigger), trigger.InnerTrigger.GetType(), "trigger.InnerTrigger type");
            Assert.AreEqual(7, trigger.WeekDays.Length, "trigger.WeekDays.Length");
            Assert.AreEqual(BuildCondition.NoBuild, trigger.BuildCondition, "trigger.BuildCondition");
		}

		[Test]
		public void ShouldHandleNestedFilterTriggers()
		{
			string xml =
				@"<filterTrigger startTime=""19:00"" endTime=""07:00"">
                    <trigger type=""filterTrigger"" startTime=""0:00"" endTime=""23:59:59"">
                        <trigger type=""intervalTrigger"" name=""continuous"" seconds=""900"" buildCondition=""ForceBuild""/>
                        <weekDays>
                            <weekDay>Saturday</weekDay>
                            <weekDay>Sunday</weekDay>
                        </weekDays>
                    </trigger>
				  </filterTrigger>";
			trigger = (FilterTrigger) NetReflector.Read(xml);
            Assert.AreEqual(typeof(FilterTrigger), trigger.InnerTrigger.GetType(), "trigger.InnerTrigger type");
            Assert.AreEqual(typeof(IntervalTrigger), ((FilterTrigger)trigger.InnerTrigger).InnerTrigger.GetType(), "trigger.InnerTrigger.InnerTrigger type");
		}

		[Test]
		public void ShouldOnlyBuildBetween7AMAnd7PMOnWeekdays()
		{
			FilterTrigger outerTrigger = new FilterTrigger((DateTimeProvider) mockDateTime.MockInstance);
			outerTrigger.StartTime = "19:00";
			outerTrigger.EndTime = "7:00";
			outerTrigger.InnerTrigger = trigger;
			
			trigger.StartTime = "0:00";
			trigger.EndTime = "23:59:59";
			trigger.WeekDays = new DayOfWeek[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
			IntegrationRequest request = ModificationExistRequest();
			mockTrigger.SetupResult("Fire", request);
			
			mockDateTime.SetupResult("Now", new DateTime(2006, 8, 10, 11, 30, 0, 0)); // Thurs midday
            Assert.AreEqual(request, outerTrigger.Fire(), "outerTrigger.Fire()");
			
			mockDateTime.SetupResult("Now", new DateTime(2006, 8, 10, 19, 30, 0, 0)); // Thurs evening
            Assert.IsNull(outerTrigger.Fire(), "outerTrigger.Fire()");			

			mockDateTime.SetupResult("Now", new DateTime(2006, 8, 12, 11, 30, 0, 0)); // Sat midday
            Assert.IsNull(outerTrigger.Fire(), "outerTrigger.Fire()");			

			mockDateTime.SetupResult("Now", new DateTime(2006, 8, 12, 19, 30, 0, 0)); // Sat evening
            Assert.IsNull(outerTrigger.Fire(), "outerTrigger.Fire()");			
		}
	}
}