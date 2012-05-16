﻿using NUnit.Framework;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Remote
{
    [TestFixture]
    public class ProjectActivityTests
    {
        #region Test methods
        #region Properties
        [Test]
        public void TypeGetSetTest()
        {
            ProjectActivity activity = new ProjectActivity();
            activity.Type = "testing";
            Assert.AreEqual("testing", activity.Type);
        }
        #endregion

        #region IsPending()
        [Test]
        public void IsPendingReturnsTrueForPendingType()
        {
            ProjectActivity activity = ProjectActivity.Pending;
            Assert.IsTrue(activity.IsPending());
        }

        [Test]
        public void IsPendingReturnsFalseForNonPendingType()
        {
            ProjectActivity activity = ProjectActivity.CheckingModifications;
            Assert.IsFalse(activity.IsPending());
        }
        #endregion
        #endregion
    }
}
