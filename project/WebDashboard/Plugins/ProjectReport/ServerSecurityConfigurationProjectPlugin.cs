using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
using ThoughtWorks.CruiseControl.WebDashboard.IO;
using ThoughtWorks.CruiseControl.WebDashboard.MVC;
using ThoughtWorks.CruiseControl.WebDashboard.MVC.Cruise;
using ThoughtWorks.CruiseControl.WebDashboard.MVC.View;
using ThoughtWorks.CruiseControl.WebDashboard.Plugins.ServerReport;
using ThoughtWorks.CruiseControl.WebDashboard.ServerConnection;

namespace ThoughtWorks.CruiseControl.WebDashboard.Plugins.ProjectReport
{
    /// <title>Server Security Configuration Project Plugin</title>
    /// <version>1.5</version>
    /// <summary>
    /// Displays the security configuration for a project on a build server.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;serverSecurityConfigurationProjectPlugin /&gt;
    /// </code>
    /// </example>
    /// <remarks>
    /// <para type="tip">
    /// This can be installed using the "Security Configuration Display" package.
    /// </para>
    /// </remarks>
    [ReflectorType("serverSecurityConfigurationProjectPlugin")]
	public class ServerSecurityConfigurationProjectPlugin : ICruiseAction, IPlugin
	{
        public const string ActionName = "ViewProjectSecurityConfiguration";
        private readonly ServerSecurityConfigurationServerPlugin plugin;

		public ServerSecurityConfigurationProjectPlugin(IFarmService farmService, 
            IVelocityViewGenerator viewGenerator, 
            ISessionRetriever sessionRetriever)
		{
            plugin = new ServerSecurityConfigurationServerPlugin(farmService, viewGenerator, sessionRetriever);
		}

		public IResponse Execute(ICruiseRequest request)
		{
			return plugin.Execute(request);
		}

		public string LinkDescription
		{
			get { return plugin.LinkDescription; }
		}

		public INamedAction[] NamedActions
		{
			get {  return new INamedAction[] { new ImmutableNamedAction(ActionName, this) }; }
		}	
	}
}
