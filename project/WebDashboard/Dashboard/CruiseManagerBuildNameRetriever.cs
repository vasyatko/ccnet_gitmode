using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.WebDashboard.ServerConnection;

namespace ThoughtWorks.CruiseControl.WebDashboard.Dashboard
{
	public class CruiseManagerBuildNameRetriever : IBuildNameRetriever
	{
		private readonly ICruiseManagerWrapper cruiseManagerWrapper;

		public CruiseManagerBuildNameRetriever(ICruiseManagerWrapper cruiseManagerWrapper)
		{
			this.cruiseManagerWrapper = cruiseManagerWrapper;
		}

        public IBuildSpecifier GetLatestBuildSpecifier(IProjectSpecifier projectSpecifier, string sessionToken)
		{
			return cruiseManagerWrapper.GetLatestBuildSpecifier(projectSpecifier, sessionToken);	
		}

        public IBuildSpecifier GetNextBuildSpecifier(IBuildSpecifier buildSpecifier, string sessionToken)
		{
			IBuildSpecifier[] buildSpecifiers = cruiseManagerWrapper.GetBuildSpecifiers(buildSpecifier.ProjectSpecifier, sessionToken);

			if (buildSpecifiers.Length == 0 || buildSpecifier.Equals(buildSpecifiers[0]))
			{
				return buildSpecifier;
			}

			for (int i = 1; i < buildSpecifiers.Length; i++)
			{
				if (buildSpecifier.Equals(buildSpecifiers[i]))
				{
					return buildSpecifiers[i-1];
				}
			}
			throw new UnknownBuildException(buildSpecifier);
		}

        public IBuildSpecifier GetPreviousBuildSpecifier(IBuildSpecifier buildSpecifier, string sessionToken)
		{
			IBuildSpecifier[] buildSpecifiers = cruiseManagerWrapper.GetBuildSpecifiers(buildSpecifier.ProjectSpecifier, sessionToken);

			if (buildSpecifier.Equals(buildSpecifiers[buildSpecifiers.Length - 1]))
			{
				return buildSpecifier;
			}

			for (int i = 0; i < buildSpecifiers.Length - 1; i++)
			{
				if (buildSpecifier.Equals(buildSpecifiers[i]))
				{
					return buildSpecifiers[i+1];
				}
			}

			throw new UnknownBuildException(buildSpecifier);
		}
	}
}
