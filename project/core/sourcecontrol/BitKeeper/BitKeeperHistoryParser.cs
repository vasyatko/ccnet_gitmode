using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace ThoughtWorks.CruiseControl.Core.Sourcecontrol.BitKeeper
{
	public class BitKeeperHistoryParser : IHistoryParser
	{
        private enum HistoryType
        {
            Unknown,
            Pre40NonVerbose,
            Pre40Verbose,
            Post40Verbose
        };

		/// <summary>
		/// This is the keyword that precedes a change set in the bk log information.
		/// </summary>
		private static readonly string BK_CHANGESET_LINE = "ChangeSet";

		private string currentLine = string.Empty;
        private HistoryType fileHistory = HistoryType.Unknown;

		public Modification[] Parse(TextReader bkLog, DateTime from, DateTime to)
		{
			// Read to the first ChangeSet. The first entry in the log
			// information will begin with this line. If no ChangeSet file
			// lines are found then there is nothing to do.
			currentLine = ReadToNotPast(bkLog, BK_CHANGESET_LINE, null);
			fileHistory = DetermineHistoryType();

            var mods = new List<Modification>();
			while (currentLine != null)
			{
				// Parse the ChangeSet entry and read till next ChangeSet
				Modification mod;
                if (fileHistory == HistoryType.Pre40Verbose)
                    mod = ParsePre40VerboseEntry(bkLog);
                else if (fileHistory == HistoryType.Pre40NonVerbose)
                    mod = ParsePre40NonVerboseEntry(bkLog);
                else
                    mod = ParsePost40VerboseEntry(bkLog);

				// Add all the modifications to the local list.
				mods.Add(mod);

				// Read to the next non-blank line.
				currentLine = bkLog.ReadLine();
			}

			return mods.ToArray();
		}

		private Modification ParsePre40VerboseEntry(TextReader bkLog)
		{
			// Example: "ChangeSet\n1.201 05/09/08 14:52:49 user@host. +1 -0\nComments"
			Regex regex = new Regex(@"(?<version>[\d.]+)\s+(?<datetime>\d{2,4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2})\s+(?<username>\S+).*");

			currentLine = currentLine.TrimStart(new char[2] {' ', '\t'});
			string filename = ParseFileName(currentLine);
			string folder = ParseFolderName(currentLine);

			// Get the next line with change info
			currentLine = bkLog.ReadLine();

			return ParseModification(regex, filename, folder, bkLog);
		}

		private Modification ParsePre40NonVerboseEntry(TextReader bkLog)
		{
			// Example: "ChangeSet@1.6, 2005-10-06 12:58:40-07:00, user@host.(none)\n  Remove file in subdir."
			Regex regex = new Regex(@"ChangeSet@(?<version>[\d.]+),\s+(?<datetime>\d{2,4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}[-+]\d{2}:\d{2}),\s+(?<username>\S+).*");

			return ParseModification(regex, "ChangeSet",string.Empty, bkLog);
		}

        private Modification ParsePost40VerboseEntry(TextReader bkLog)
        {
            // Example: "ChangeSet\n1.201 05/09/08 14:52:49 user@host. +1 -0\nComments"
            Regex regex = new Regex(@"(?<filename>.+)@(?<version>[\d.]+),\s+(?<datetime>\d{2,4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}[-+]\d{2}:\d{2}),\s+(?<username>\S+).*");

            currentLine = currentLine.TrimStart(new char[2] { ' ', '\t' });

            Match match = regex.Match(currentLine);
            if (!match.Success)
                throw new Exception("Unable to parse line: " + currentLine);

            string filename = ParseFileName(match.Result("${filename}"));
            string folder = ParseFolderName(match.Result("${filename}"));


            return ParseModification(regex, filename, folder, bkLog);
        }

		private Modification ParseModification(Regex regex, string filename, string folder, TextReader bkLog)
		{
			Match match = regex.Match(currentLine);
			if (!match.Success)
				throw new Exception("Unable to parse line: " + currentLine);
	
			Modification mod = new Modification();
			mod.FileName = filename;
			mod.FolderName = folder;
			mod.ModifiedTime = ParseDate(match.Result("${datetime}"));
			mod.Type = "Modified";
			mod.UserName = match.Result("${username}");
			mod.Version = match.Result("${version}");
	
			// Read all lines of the comment and flatten them
			mod.Comment = ParseComment(bkLog);

			// Determine the modification type
			if (mod.FileName == "ChangeSet")
			{
				// Only ChangeSets should have a file name of ChangeSet
				mod.Type = "ChangeSet";
			}
			else if (mod.Comment.IndexOf("Delete: ") != -1
				&& mod.FolderName.IndexOf("BitKeeper/deleted") == 0)
			{
				string fullFilePath = mod.Comment.Substring(mod.Comment.IndexOf("Delete: ") + 8);

				// Deleted files have the name of the BitKeeper/deleted file,
				// but we would like the name of the original file that was deleted
				mod.Type = "Deleted";
				mod.FileName = ParseFileName(fullFilePath);
				mod.FolderName = ParseFolderName(fullFilePath);
			}
			else if (mod.Comment.IndexOf("BitKeeper file") != -1 || mod.Version == "1.0")
			{
				// Added files have a comment that starts with "BitKeeper file" in pre-4.0 versions
                // In post-4.0 versions, the only way to tell is by the revision number being "1.0"
				mod.Type = "Added";
			}

			else if (mod.Comment.IndexOf("Rename: ") != -1)
			{
				// Renamed files have a comment that starts with "Rename: "
				mod.Type = "Renamed";
			}
			return mod;
		}

		private string ReadToNotPast(TextReader reader, string startsWith, string notPast)
		{
			currentLine = reader.ReadLine();
			while (currentLine != null && !currentLine.StartsWith(startsWith))
			{
				if ((notPast != null) && currentLine.StartsWith(notPast))
				{
					return null;
				}
				currentLine = reader.ReadLine();
			}
			return currentLine;
		}

		private DateTime ParseDate(string date)
		{
			string sep = (fileHistory == HistoryType.Pre40Verbose) ? "/" : "-";

			// BK is funny - we can't guarantee that the year will be two or four digits,
			// so we have to check how many digits we got and deal with it
			int firstSep = date.IndexOf(sep);
			string dateFormat = (firstSep == 4) ? "yyyy" : "yy";
			dateFormat += string.Format("'{0}'MM'{0}'dd HH:mm:ss", sep);
			if (fileHistory != HistoryType.Pre40Verbose)
				dateFormat += "zzz";

			return DateTime.ParseExact(date, dateFormat, DateTimeFormatInfo.InvariantInfo);
		}

		private string ParseComment(TextReader bkLog)
		{
			// All the text from now to the next blank line constitutes the comment
			string message = string.Empty;
			bool multiLine = false;

			// We don't trim the newly read line because blank comments lines
			// start with two or three spaces, while a blank line between changes
			// or changesets starts with no spaces.  Thus, we can handle blank comment
			// lines only if we treat the lines that start with spaces a "non-empty";
			// therefore, no trimming here!
			currentLine = bkLog.ReadLine();
			while (currentLine != null && currentLine.Length != 0)
			{
				if (multiLine)
				{
					message += Environment.NewLine;
				}
				else
				{
					multiLine = true;
				}
				message += currentLine;

				// Go to the next line.
				currentLine = bkLog.ReadLine();
			}
			return message;
		}

		/// <summary>
		/// Called on first ChangeSet line to determine if this is verbose or non-verbose output
		/// </summary>
		private HistoryType DetermineHistoryType()
		{
			if (currentLine == null)
				return HistoryType.Unknown;

            if (currentLine.StartsWith("ChangeSet@") && (currentLine.IndexOf("+") != -1))
                return HistoryType.Post40Verbose;
            if (currentLine.StartsWith("ChangeSet@"))
                return HistoryType.Pre40NonVerbose;
            return HistoryType.Pre40Verbose;
		}

		private string ParseFileName(string workingFileName)
		{
			int lastSlashIndex = workingFileName.LastIndexOf("/");
			return workingFileName.Substring(lastSlashIndex + 1);
		}

		private string ParseFolderName(string workingFileName)
		{
			int lastSlashIndex = workingFileName.LastIndexOf("/");
			string folderName = string.Empty;
			if (lastSlashIndex != -1)
			{
				folderName = workingFileName.Substring(0, lastSlashIndex);
			}
			return folderName;
		}
	}
}
