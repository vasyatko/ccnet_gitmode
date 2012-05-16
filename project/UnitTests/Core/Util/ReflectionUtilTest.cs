using System;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Util
{
	[TestFixture]
	public class ReflectionUtilTest : CustomAssertion
	{
        [Test]
		public void TestReflectionEquals()
		{
			ReflectTest o1 = new ReflectTest(1, "hello", new ReflectTest(2, "sub", null));
			ReflectTest o2 = new ReflectTest(1, "hello", new ReflectTest(2, "sub", null));
			Assert.AreEqual(o1, o2);
		}

		public void TestReflectionEquals_BothNull()
		{
			Assert.AreEqual(null, null);
		}

		public void TestReflectionEquals_OneNull()
		{
			ReflectTest o1 = new ReflectTest(1, "hello", new ReflectTest(2, "sub", null));
			Assert.IsTrue(! o1.Equals(null));
		}

		public void TestReflectionEquals_NotEqualFields()
		{
			ReflectTest o1 = new ReflectTest(1, "hello", new ReflectTest(2, "sub", null));
			ReflectTest o2 = new ReflectTest(2, "hello", new ReflectTest(2, "sub", null));
			Assert.IsTrue(! o1.Equals(o2));
		}

		public void TestReflectionEquals_NotEqualProperties()
		{
			ReflectTest o1 = new ReflectTest(1, "hello", new ReflectTest(2, "sub", null));
			ReflectTest o2 = new ReflectTest(1, "hello", null);
			Assert.IsTrue(! o1.Equals(o2));
		}

		public void TestReflectionEquals_DifferentTypes()
		{
			ReflectTest o1 = new ReflectTest(1, "hello", new ReflectTest(2, "sub", null));
			string o2 = "testing";
			Assert.IsTrue(! o1.Equals(o2));
		}

		public void TestReflectionEquals_Arrays()
		{
			ReflectTest o1 = new ReflectTest(1, "hello", new ReflectTest(2, "sub", null));
			o1.Values = new String[] { "a", "b" };
			ReflectTest o2 = new ReflectTest(1, "hello", new ReflectTest(2, "sub", null));
			o2.Values = new String[] { "a", "b" };
			Assert.AreEqual(o1, o2);
		}

		public void TestReflectionToString()
		{
			ReflectTest o1 = new ReflectTest(1, "hello", new ReflectTest(2, "sub", null));
			Assert.AreEqual("ReflectTest: (id=1,name=hello,Child=ReflectTest: (id=2,name=sub,Child=,Values=),Values=)", ReflectionUtil.ReflectionToString(o1));
		}

		private class ReflectTest
		{
			public int id;
			public string name;
			private ReflectTest child;
			private string[] values;

			public ReflectTest() { }

			public ReflectTest(int id, string name, ReflectTest child)
			{
				this.id = id;
				this.name = name;
				this.child = child;
			}

			public ReflectTest Child
			{
				get { return child; }
				set { child = value; }
			}

			public string[] Values
			{
				get { return values; }
				set { values = value; }
			}

			public override bool Equals(object obj)
			{
				return ReflectionUtil.ReflectionEquals(this, obj);
			}

			public override string ToString()
			{
				return ReflectionUtil.ReflectionToString(this);
			}

			public override int GetHashCode()
			{
				return ToString().GetHashCode();
			}
		}
	}
}
