using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace ThoughtWorks.CruiseControl.Core.Sourcecontrol
{
	/// <summary>
	/// Implementation of IHistoryParser to handle StarTeam output that
	/// describes modifications within the version control system.
	/// </summary>
	public class StarTeamHistoryParser : IHistoryParser
	{
		private readonly IStarTeamRegExProvider starTeamRegExProvider;
		internal static readonly string FolderInfoSeparator = "Folder: ";
		internal static readonly string FileHistorySeparator = "----------------------------";

//		DateTimeFormatInfo dfi;
		public CultureInfo Culture = CultureInfo.CurrentCulture;

		public StarTeamHistoryParser(IStarTeamRegExProvider starTeamRegExProvider)
		{
			this.starTeamRegExProvider = starTeamRegExProvider;
			// Create DateTimeFormatInfo
//			dfi = new DateTimeFormatInfo();
//			dfi.AMDesignator = "AM";
//			dfi.PMDesignator = "PM";
//			dfi.MonthDayPattern = @"M/d/yy h:mm:ss tt";
		}

		/// <summary>
		/// Method implementaion for IHistoryParser
		/// </summary>
		/// <param name="starTeamLog"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
		/// <returns></returns>
		public Modification[] Parse(TextReader starTeamLog, DateTime from, DateTime to)
		{
			Regex folderRegex = new Regex(starTeamRegExProvider.FolderRegEx);
			Regex fileRegex = new Regex(starTeamRegExProvider.FileRegEx);
			Regex historyRegex = new Regex(starTeamRegExProvider.FileHistoryRegEx);

			// Temporary holder of Modification objects
            var modList = new List<Modification>();

			// Read conetent of the stream as a string
			// ASSUMPTION: entire log fits into the available memory
			String s = starTeamLog.ReadToEnd();

			// Append folder info separator at the end of the string so
			// that the regular expression engine does not miss the last
			// folder's information. This is required because of the way
			// the expression FolderRegEx is constructed.
			s += FolderInfoSeparator;

			// Parse the whole content to separate the info about each
			// folder and the files it has
			for (Match mFolder = folderRegex.Match(s); mFolder.Success; mFolder = mFolder.NextMatch())
			{
				// Working folder
				String folder = mFolder.Result("${working_directory}");

				// Scan changes for each file in the folder
				for (Match mFile = fileRegex.Match(mFolder.Value); mFile.Success; mFile = mFile.NextMatch())
				{
					// Create a Modification object for the current file
					Modification mod = new Modification();

					// Set the modification attributes
					mod.FolderName = folder;
					mod.FileName = mFile.Result("${file_name}");
					mod.Type = mFile.Result("${file_status}");

					// Substring that contains file history. Append a new line 
					// followed by the FileHistorySeparator so that the parse
					// engine can extract the comments for the last history
					String fileHistory = mFile.Result("${file_history}") + "\n" +
					                     FileHistorySeparator;

					// Only get the first match which describes the 
					// most recent changes
					Match mHistory = historyRegex.Match(fileHistory);
					if (mHistory.Success)
					{
						mod.EmailAddress = "N/A";
						mod.UserName = mHistory.Result("${author_name}");

						// date_string looks like "12/9/02 10:33:36 AM"
						// ASSUMPTION: StarTeam server and this application
						// runs in the same TIMEZONE
						mod.ModifiedTime = DateTime.Parse(mHistory.Result("${date_string}"), Culture.DateTimeFormat);
						mod.Comment = mHistory.Result("${change_comment}");
					}
					modList.Add(mod);
				}
			}
			return modList.ToArray();
		}
	}
}