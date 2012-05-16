using System;
using System.Collections;
using System.Collections.Generic;
using ThoughtWorks.CruiseControl.Remote;
using System.Xml;

namespace ThoughtWorks.CruiseControl.Core
{
	public interface IIntegrationResult
	{
		// Project configuration properties
		string ProjectName { get; }
		string ProjectUrl { get; set;}
		string WorkingDirectory { get; set; }
		string ArtifactDirectory { get; set;}
		string BaseFromArtifactsDirectory(string pathToBase);
		string BaseFromWorkingDirectory(string pathToBase);
        string BuildLogDirectory { get; set;}   

        /// <summary>
        ///  The parameters used.
        /// </summary>
        List<NameValuePair> Parameters { get; set; }

		// Current integration state
		BuildCondition BuildCondition { get; }
		string Label { get; set; }
		IntegrationStatus Status { get; set; }
		DateTime StartTime { get; set; }
		DateTime EndTime { get; }
		TimeSpan TotalIntegrationTime { get; }
		bool Failed { get; }
		bool Fixed { get; }
		bool Succeeded { get; }
		void MarkStartTime();
		void MarkEndTime();
		bool IsInitial();
		IntegrationRequest IntegrationRequest { get; }
        
		IntegrationStatus LastIntegrationStatus { get; }
        // Users who contributed modifications to a series of failing builds:
        ArrayList FailureUsers { get; }             // This should really be a Set but sets are not available in .NET 1.1
		DateTime LastModificationDate { get; }
		string LastChangeNumber { get; }
		IntegrationSummary LastIntegration { get; }

		// Last successful integration state
		string LastSuccessfulIntegrationLabel { get; }

		// Current integration artifacts
		IList TaskResults { get; }
		Modification[] Modifications { get; set; }
		Exception ExceptionResult { get; set; }
		string TaskOutput { get; }
		void AddTaskResult(string result);
        void AddTaskResult(ITaskResult result);
		bool HasModifications();
		bool ShouldRunBuild();

        /// <summary>
        /// Any error that occurred during the get modifications stage of source control.
        /// </summary>
        /// <remarks>
        /// If there is no error then this property will be null.
        /// </remarks>
        Exception SourceControlError { get; set; }

        #region HasSourceControlError
        /// <summary>
        /// Gets or sets a value indicating whether there was a source control error.
        /// </summary>
        bool HasSourceControlError { get; }
        #endregion

        #region LastBuildStatus
        /// <summary>
        /// The last status from a build that progressed pass any source control checks.
        /// </summary>
        IntegrationStatus LastBuildStatus { get; set; }
        #endregion

        IDictionary IntegrationProperties { get; }
        Util.BuildProgressInformation BuildProgressInformation { get; }

        #region Clone()
        /// <summary>
        /// Clones this integration result.
        /// </summary>
        /// <returns>Returns a clone of the result.</returns>
        IIntegrationResult Clone();
        #endregion

        #region Merge()
        /// <summary>
        /// Merges another result.
        /// </summary>
        /// <param name="value">The result to merge.</param>
        void Merge(IIntegrationResult value);
        #endregion

        #region SourceControlData
        /// <summary>
        /// Extended source control data.
        /// </summary>
        /// <remarks>
        /// It is up to the individual source control providers to decide what to store in here.
        /// </remarks>
        List<NameValuePair> SourceControlData { get; }
        #endregion
    }
}
