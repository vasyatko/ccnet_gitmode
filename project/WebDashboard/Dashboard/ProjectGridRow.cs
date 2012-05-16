using System.Drawing;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.WebDashboard.Resources;

namespace ThoughtWorks.CruiseControl.WebDashboard.Dashboard
{
    public class ProjectGridRow
    {
        private readonly ProjectStatus status;
        private readonly IServerSpecifier serverSpecifier;
        private readonly string url;
        private readonly string parametersUrl;

        public ProjectGridRow(ProjectStatus status, IServerSpecifier serverSpecifier,
            string url, string parametersUrl, Translations translations)
        {
            this.status = status;
            this.serverSpecifier = serverSpecifier;
            this.url = url;
            this.parametersUrl = parametersUrl;
        }

        public string Name
        {
            get { return status.Name; }
        }

        public string ServerName
        {
            get { return serverSpecifier.ServerName; }
        }

        public string Category
        {
            get { return status.Category; }
        }

        public string BuildStatus
        {
            get { return status.BuildStatus.ToString(); }
        }

        public string BuildStatusHtmlColor
        {
            get { return CalculateHtmlColor(status.BuildStatus); }
        }

        public string LastBuildDate
        {
            get { return DateUtil.FormatDate(status.LastBuildDate); }
        }

        public string NextBuildTime
        {
            get
            {
                if (status.NextBuildTime == System.DateTime.MaxValue)
                {
                    return "Force Build Only";
                }
                else
                {
                    return DateUtil.FormatDate(status.NextBuildTime);
                }
            }
        }

        public string LastBuildLabel
        {
            get { return (status.LastBuildLabel != null ? status.LastBuildLabel : "no build available"); }
        }

        public string Status
        {
            get { return status.Status.ToString(); }
        }

        public string Activity
        {
            get { return status.Activity.ToString(); }
        }

        public string CurrentMessage
        {
            get { return status.CurrentMessage; }
        }

        public string Breakers
        {
            get
            {
                return GetMessageText(Message.MessageKind.Breakers);
            }
        }

        public string FailingTasks
        {
            get
            {
                return GetMessageText(Message.MessageKind.FailingTasks);
            }
        }

        public string Fixer
        {
            get
            {
                return GetMessageText(Message.MessageKind.Fixer);
            }
        }

        public string Url
        {
            get { return url; }
        }


        public string Queue
        {
            get { return status.Queue; }
        }


        public int QueuePriority
        {
            get { return status.QueuePriority; }
        }


        public string StartStopButtonName
        {
            get { return (status.Status == ProjectIntegratorState.Running) ? "StopBuild" : "StartBuild"; }
        }

        public string StartStopButtonValue
        {
            get { return (status.Status == ProjectIntegratorState.Running) ? "Stop" : "Start"; }
        }

        public string ForceAbortBuildButtonName
        {
            get { return (status.Activity != ProjectActivity.Building) ? "ForceBuild" : "AbortBuild"; }
        }

        public string ForceAbortBuildButtonValue
        {
            get { return (status.Activity != ProjectActivity.Building) ? "Force" : "Abort"; }
        }

        public bool AllowForceBuild
        {
            get { return serverSpecifier.AllowForceBuild; }
        }

        public bool AllowStartStopBuild
        {
            get { return serverSpecifier.AllowStartStopBuild; }
        }

        private string CalculateHtmlColor(IntegrationStatus integrationStatus)
        {
            if (integrationStatus == IntegrationStatus.Success)
            {
                return Color.Green.Name;
            }
            else if (integrationStatus == IntegrationStatus.Unknown)
            {
                return Color.Blue.Name;
            }
            else
            {
                return Color.Red.Name;
            }
        }

        public string BuildStage
        {
            get
            {
                string CurrentBuildStage = status.BuildStage;

                if (CurrentBuildStage.Length == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return CurrentBuildStage;
                }
            }
        }

        public string ParametersUrl
        {
            get { return parametersUrl; }
        }


        private string GetMessageText(Message.MessageKind messageType)
        {
            foreach (Message m in status.Messages)
            {
                if (m.Kind == messageType)
                {
                    return m.Text;
                }
            }
            return string.Empty;

        }
    }
}
