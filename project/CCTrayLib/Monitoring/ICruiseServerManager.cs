using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.CCTrayLib.Monitoring
{
	/// <summary>
	/// This is like ICruiseManager, but relates to an individual server.
	/// </summary>
	public interface ICruiseServerManager
	{
		string DisplayName { get; }
        BuildServer Configuration { get; }
        string SessionToken { get; }

		/// <summary>
		/// Cancel the pending request on the integration queue for the specified project on this server.
		/// </summary>
		/// <param name="projectName">Name of the project to cancel.</param>
		void CancelPendingRequest(string projectName);
		/// <summary>
		/// Gets the projects and integration queues snapshot from this server.
		/// </summary>
        CruiseServerSnapshot GetCruiseServerSnapshot();

        /// <summary>
        /// Gets the list of projects.
        /// </summary>
        /// <returns>The available projects on the server.</returns>
        CCTrayProject[] GetProjectList();

        bool Login();
        void Logout();
     }
}
