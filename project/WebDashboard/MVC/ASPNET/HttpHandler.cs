using System.IO;
using System.Reflection;
using System.Web;
using Objection;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;
using ThoughtWorks.CruiseControl.WebDashboard.IO;

namespace ThoughtWorks.CruiseControl.WebDashboard.MVC.ASPNET
{
	// No need for session state yet, but if we do later then we should also add IRequiresSessionState to list of interfaces
	public class HttpHandler : IHttpHandler
	{
		private const string RESOLVED_TYPE_MAP = "ResolvedTypeMap";

        public void ProcessRequest(HttpContext context)
        {
            ObjectionStore objectionStore = new ObjectionStore(
                new CachingImplementationResolver(new NMockAwareImplementationResolver(), new CachedTypeMap(context.Cache, RESOLVED_TYPE_MAP)),
                new MaxLengthConstructorSelectionStrategy());
            ObjectSource objectSource = new CruiseObjectSourceInitializer(objectionStore).SetupObjectSourceForRequest(context);

            context.Response.AppendHeader("X-CCNet-Version",
                string.Format("CruiseControl.NET/{0}", Assembly.GetExecutingAssembly().GetName().Version));
            Assembly.GetExecutingAssembly().GetName().Version.ToString();

            IResponse response = ((RequestController)objectSource.GetByType(typeof(RequestController))).Do();
            response.Process(context.Response);
        }

		public bool IsReusable
		{
			get { return true; }
		}
	}
}
