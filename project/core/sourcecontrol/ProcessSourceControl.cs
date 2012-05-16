using System;
using System.IO;
using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core.Util;
using System.Collections.Generic;

namespace ThoughtWorks.CruiseControl.Core.Sourcecontrol
{
	public abstract class ProcessSourceControl 
        : SourceControlBase
	{
		protected ProcessExecutor executor;
		protected IHistoryParser historyParser;
		private Timeout timeout = Timeout.DefaultTimeout;

		public ProcessSourceControl(IHistoryParser historyParser) : this(historyParser, new ProcessExecutor())
		{
		}

		public ProcessSourceControl(IHistoryParser historyParser, ProcessExecutor executor)
		{
			this.executor = executor;
			this.historyParser = historyParser;
		}

		protected ProcessExecutor ProcessExecutor
		{
			get { return executor; }
		}

        /// <summary>
        /// Sets the timeout period for the source control operation. See <link>Timeout Configuration</link> for details. 
        /// </summary>
        /// <version>1.0</version>
        /// <default>10 minutes</default>
		[ReflectorProperty("timeout", typeof (TimeoutSerializerFactory), Required = false)]
		public Timeout Timeout
		{
			get { return timeout; }
			set { timeout = (value == null) ? Timeout.DefaultTimeout : value; }
		}

		protected Modification[] GetModifications(ProcessInfo info, DateTime from, DateTime to)
		{
			ProcessResult result = Execute(info);
			return ParseModifications(result, from, to);
		}

		protected ProcessResult Execute(ProcessInfo processInfo)
		{
			processInfo.TimeOut = Timeout.Millis;
			ProcessResult result = executor.Execute(processInfo);

			if (result.TimedOut)
			{
				throw new CruiseControlException("Source control operation has timed out.");
			}
			else if (result.Failed)
			{
                var message = string.Format(
                    "Source control operation failed: {0}. Process command: {1} {2}",
                    result.StandardError,
                    processInfo.FileName,
                    processInfo.PublicArguments);
				throw new CruiseControlException(message);
			}
			else if (result.HasErrorOutput)
			{
				Log.Warning(string.Format("Source control wrote output to stderr: {0}", result.StandardError));
			}
			return result;
		}

		protected Modification[] ParseModifications(ProcessResult result, DateTime from, DateTime to)
		{
			return ParseModifications(new StringReader(result.StandardOutput), from, to);
		}

        protected Modification[] ParseModifications(ProcessResult result, string lastRevision)
        {
            var mods = ParseModifications(new StringReader(result.StandardOutput), DateTime.MinValue, DateTime.MaxValue);
            if (!string.IsNullOrEmpty(lastRevision))
            {
                var actualModes = new List<Modification>();
                foreach (var mod in mods)
                {
                    if (mod.ChangeNumber != lastRevision) actualModes.Add(mod);
                }
                mods = actualModes.ToArray();
            }
            return mods;
        }

		protected Modification[] ParseModifications(TextReader reader, DateTime from, DateTime to)
		{
			return historyParser.Parse(reader, from, to);
		}

		public override void GetSource(IIntegrationResult result)
		{
		}

        public override void Initialize(IProject project)
		{
		}

        public override void Purge(IProject project)
		{
		}

        // rw issue
        /// <summary>
        /// Converts the comment (or parts from it) into an url pointing to the issue for this build. See <link>IssueUrlBuilder</link> for 
        /// more details.
        /// </summary>
        /// <version>1.4</version>
        /// <default>None</default>
        [ReflectorProperty("issueUrlBuilder", InstanceTypeKey = "type", Required = false)]
        public IModificationUrlBuilder IssueUrlBuilder;

        protected void FillIssueUrl(Modification[] modifications)
        {
            if ((IssueUrlBuilder != null) && (modifications != null))
            {
                IssueUrlBuilder.SetupModification(modifications);
            }
        }
	}
}