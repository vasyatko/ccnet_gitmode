using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.Remote;
using System;
using ThoughtWorks.CruiseControl.Remote.Messages;

namespace ThoughtWorks.CruiseControl.CCTrayLib.Monitoring
{
	/// <summary>
	/// Allows access to the state of a single cruise control server, over remoting
	/// </summary>
	public class RemotingCruiseServerManager : ICruiseServerManager
	{
        private readonly CruiseServerClientBase manager;
		private readonly BuildServer configuration;
		private readonly string displayName;

        public RemotingCruiseServerManager(CruiseServerClientBase manager, BuildServer buildServer)
		{
			this.manager = manager;
			this.displayName = buildServer.DisplayName;
            this.configuration = buildServer;
		}

        public BuildServer Configuration
        {
            get { return configuration; }
        }

        public string SessionToken
        {
            get { return manager.SessionToken; }
        }

		public string DisplayName
		{
			get { return displayName; }
		}

		public void CancelPendingRequest(string projectName)
		{
            manager.CancelPendingRequest(projectName);
		}

		/// <summary>
		/// Gets the projects and integration queues snapshot from this server.
		/// </summary>
        public CruiseServerSnapshot GetCruiseServerSnapshot()
		{
            var snapshot = manager.GetCruiseServerSnapshot();
            return snapshot;
		}

        /// <summary>
        /// Gets the list of projects.
        /// </summary>
        /// <returns>The available projects on the server.</returns>
        public CCTrayProject[] GetProjectList()
        {
            ProjectStatus[] statuses = manager.GetProjectStatus();
            CCTrayProject[] projects = new CCTrayProject[statuses.Length];

            for (int i = 0; i < statuses.Length; i++)
            {
                ProjectStatus status = statuses[i];
                projects[i] = new CCTrayProject(this.configuration, status.Name);
            }

            return projects;
        }

        public bool Login()
        {
            if (configuration.SecurityType != null)
            {
                Logout();
                IAuthenticationMode authentication = ExtensionHelpers.RetrieveAuthenticationMode(configuration.SecurityType);
                authentication.Settings = configuration.SecuritySettings;
                var isValid = manager.Login(authentication.GenerateCredentials().Credentials);
                return isValid;
            }
            else
            {
                return true;
            }
        }

        public void Logout()
        {
            manager.Logout();
        }
	}
}
