using System.IO;
using System.Xml;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.WebDashboard.MVC;
using ThoughtWorks.CruiseControl.WebDashboard.ServerConnection;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace ThoughtWorks.CruiseControl.WebDashboard.Plugins.FarmReport
{
	public class XmlReportAction : IAction
	{
		public const string ACTION_NAME = "XmlStatusReport";

		private readonly IFarmService farmService;

		public XmlReportAction(IFarmService farmService)
		{
			this.farmService = farmService;
		}

		public IResponse Execute(IRequest request)
		{
			ProjectStatusListAndExceptions allProjectStatus = farmService.GetProjectStatusListAndCaptureExceptions(null);

			StringWriter stringWriter = new StringWriter();
			XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
			xmlWriter.WriteStartElement("Projects");

            // bvc enhancement
            xmlWriter.WriteAttributeString("CCType","CCNet"); // to identify the type of cruisecontrol, .Net, .rb, ...
            //todo xmlWriter.WriteAttributeString("CCVersion", farmService.GetServerVersion( )); // to identify the version of the server


            

			foreach (ProjectStatusOnServer projectStatusOnServer in allProjectStatus.StatusAndServerList)
			{
			    WriteProjectStatus(xmlWriter, projectStatusOnServer.ProjectStatus, projectStatusOnServer.ServerSpecifier);
            }

			xmlWriter.WriteEndElement();

			return new XmlFragmentResponse(stringWriter.ToString());
		}

	    private static void WriteProjectStatus(XmlWriter xmlWriter, ProjectStatus status, IServerSpecifier serverSpecifier)
	    {
	        xmlWriter.WriteStartElement("Project");
	        xmlWriter.WriteAttributeString("name", status.Name);
	        xmlWriter.WriteAttributeString("category", status.Category);
	        xmlWriter.WriteAttributeString("activity", status.Activity.ToString());
	        xmlWriter.WriteAttributeString("lastBuildStatus", status.BuildStatus.ToString());
	        xmlWriter.WriteAttributeString("lastBuildLabel", status.LastSuccessfulBuildLabel);
	        xmlWriter.WriteAttributeString("lastBuildTime", XmlConvert.ToString(status.LastBuildDate, XmlDateTimeSerializationMode.Local));
	        xmlWriter.WriteAttributeString("nextBuildTime", XmlConvert.ToString(status.NextBuildTime, XmlDateTimeSerializationMode.Local));
	        xmlWriter.WriteAttributeString("webUrl", status.WebURL);
            xmlWriter.WriteAttributeString("CurrentMessage", status.CurrentMessage);
            xmlWriter.WriteAttributeString("BuildStage", status.BuildStage);
            xmlWriter.WriteAttributeString("serverName", serverSpecifier.ServerName);

            xmlWriter.WriteStartElement("messages");

            foreach (Message m in status.Messages)
            {
                xmlWriter.WriteStartElement("message");
                xmlWriter.WriteAttributeString("text", m.Text);
                xmlWriter.WriteAttributeString("kind", m.Kind.ToString());
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();

	        xmlWriter.WriteEndElement();
	    }
	}
}