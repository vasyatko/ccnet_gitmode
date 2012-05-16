using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.Core.Sourcecontrol.Perforce
{
	public class ProcessP4Purger : IP4Purger
	{
		private readonly IP4ProcessInfoCreator infoCreator;
		private readonly ProcessExecutor executor;

		public ProcessP4Purger(ProcessExecutor executor, IP4ProcessInfoCreator infoCreator)
		{
			this.executor = executor;
			this.infoCreator = infoCreator;
		}

		public void Purge(P4 p4, string workingDirectory)
		{
			if (p4.Client != null && p4.Client != string.Empty)
			{
				Log.Info(string.Format("Attempting to Delete Perforce Client Spec [{0}]", p4.Client));
				DeleteClientSpec(p4);
			}
			Log.Info(string.Format("Attempting to Delete Working Directory [{0}]", workingDirectory));
			new IoService().DeleteIncludingReadOnlyObjects(workingDirectory);
		}

		private void DeleteClientSpec(P4 p4)
		{
			ProcessResult result = executor.Execute(infoCreator.CreateProcessInfo(p4, "client -d " + p4.Client));
			if (result.ExitCode != ProcessResult.SUCCESSFUL_EXIT_CODE)
			{
				throw new CruiseControlException(string.Format("Failed to Initialize client (exit code was {0}).\r\nStandard output was: {1}\r\nStandard error was {2}", result.ExitCode, result.StandardOutput, result.StandardError));
			}
		}
	}
}
