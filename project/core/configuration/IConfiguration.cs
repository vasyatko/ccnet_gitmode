using System.Collections.Generic;
using ThoughtWorks.CruiseControl.Core.Config;
using ThoughtWorks.CruiseControl.Core.Security;

namespace ThoughtWorks.CruiseControl.Core
{
	public interface IConfiguration
	{
		IProjectList Projects { get; }

        /// <summary>
        /// Store any custom queue configurations.
        /// </summary>
        List<IQueueConfiguration> QueueConfigurations { get; }

        /// <summary>
        /// Finds the queue configuration by name.
        /// </summary>
        /// <param name="name">The name of the configuration to find.</param>
        /// <returns>The queue configuration if found, or a default instance of the queue configuration.</returns>
        IQueueConfiguration FindQueueConfiguration(string name);

        /// <summary>
        /// Store the security manager that is being used.
        /// </summary>
        ISecurityManager SecurityManager { get; }

		void AddProject(IProject project);
		void DeleteProject(string name);
	}
}
