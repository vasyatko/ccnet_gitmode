using Exortech.NetReflector;
using NUnit.Framework;
using ThoughtWorks.CruiseControl.Core;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Sourcecontrol
{
	[TestFixture]
	public class ViewCVSUrlBuilderTest : CustomAssertion
	{
		const string URL = "http://localhost:7899/viewcvs/ccnet/NUnitDemo";

		static string CreateSourceControlXml()
		{
			return string.Format( "<webUrlBuilder type=\"websvn\"><url>{0}</url></webUrlBuilder>", URL );	
		}

		static ViewCVSUrlBuilder CreateBuilder() 
		{
			ViewCVSUrlBuilder cvsurl = new ViewCVSUrlBuilder();
			NetReflector.Read( CreateSourceControlXml(), cvsurl );
			return cvsurl;
		}


		[Test]
		public void ValuePopulation()
		{
			ViewCVSUrlBuilder cvsurl = new ViewCVSUrlBuilder();
			NetReflector.Read( CreateSourceControlXml(), cvsurl );
			
			Assert.AreEqual( URL + @"/{0}", cvsurl.Url );
		}

		[Test]
		public void CheckSetup()
		{
			Modification[] mods = new Modification[2];
			mods[0] = new Modification();
			mods[0].FolderName =string.Empty;
			mods[0].FileName = "NUnitDemo.build";
			mods[1] = new Modification();
			mods[1].FolderName = "NUnitDemo";
			mods[1].FileName = "TestClass.cs";

			CreateBuilder().SetupModification(mods);

			string url0 = URL + "/NUnitDemo.build";
			string url1 = URL + "/NUnitDemo/TestClass.cs";

			Assert.AreEqual( url0, mods[0].Url );
			Assert.AreEqual( url1, mods[1].Url );
		}
		
	}

}