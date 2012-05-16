﻿using Exortech.NetReflector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.Core.Security
{
    /// <summary>
    /// <para>
    /// Provides an in-memory cache for sessions that is backed by a store on disk.
    /// </para>
    /// <para>
    /// This cache will store the sessions details in memory for quick-access. Whenever a session is changed it also writes a copy of the
    /// details to a file on the disk. Then when the security manager is restarted it loads all the sessions from disk.
    /// </para>
    /// </summary>
    /// <title>File Based Security Cache</title>
    /// <version>1.5</version>
    /// <key name="type">
    /// <description>The type of security cache to use.</description>
    /// <value>fileBasedCache</value>
    /// </key>
    /// <example>
    /// <code>
    /// &lt;cache type="fileBasedCache" duration="10" mode="sliding" location="C:\sessions\"/&gt;
    /// </code>
    /// </example>
    [ReflectorType("fileBasedCache")]
    public class FileBasedSessionCache
        : SessionCacheBase
    {
		private readonly IFileSystem fileSystem;
		private readonly IExecutionEnvironment executionEnvironment;
        private string storeLocation;

		public FileBasedSessionCache() : this(new SystemIoFileSystem(), new ExecutionEnvironment(), new SystemClock())
		{ }

		public FileBasedSessionCache(IFileSystem fileSystem, IExecutionEnvironment executionEnvironment, IClock clock) : base(clock)
		{
			this.fileSystem = fileSystem;
			this.executionEnvironment = executionEnvironment;

			storeLocation = Path.Combine(this.executionEnvironment.GetDefaultProgramDataFolder(ApplicationType.Server), "sessions");
			fileSystem.EnsureFolderExists(storeLocation);
		}

        /// <summary>
        /// The location where the backing files are stored. If this is a relative folder, it will be relative to the program data folder for
        /// CruiseControl.NET.
        /// </summary>
        /// <version>1.5</version>
        /// <default>Sessions</default>
        [ReflectorProperty("location", Required = false)]
        public virtual string StoreLocation
        {
            get { return storeLocation; }
            set
            {
				string path = executionEnvironment.EnsurePathIsRooted(value);

				if (!string.IsNullOrEmpty(path))
					fileSystem.EnsureFolderExists(path);

            	storeLocation = path;
            }
        }

        public override void Initialise()
        {
            base.Initialise();

            DirectoryInfo storeDirectory = new DirectoryInfo(storeLocation);
            if (storeDirectory.Exists)
            {
                FileInfo[] sessions = storeDirectory.GetFiles("*.session");
                XmlDocument sessionXml = new XmlDocument();
                foreach (FileInfo sessionFile in sessions)
                {
                    try
                    {
                        sessionXml.Load(sessionFile.FullName);
                        string sessionToken = sessionXml.DocumentElement.GetAttribute("token");
                        string userName = sessionXml.DocumentElement.GetAttribute("userName");
                        string expiryTime = sessionXml.DocumentElement.GetAttribute("expiry");
                        SessionDetails session = new SessionDetails(userName, DateTime.Parse(expiryTime));
                        foreach (XmlElement value in sessionXml.SelectNodes("//value"))
                        {
                            string valueKey = value.GetAttribute("key");
                            session.Values[valueKey] = value.InnerText;
                        }
                        AddToCacheInternal(sessionToken, session);
                    }
                    catch { }   // Ignore any exceptions - just assume that the session is invalid
                }
            }
            else
            {
                storeDirectory.Create();
            }
        }

        public override string AddToCache(string userName)
        {
            string sessionToken = base.AddToCache(userName);
            SaveSession(sessionToken);
            return sessionToken;
        }

        public override void RemoveFromCache(string sessionToken)
        {
            base.RemoveFromCache(sessionToken);
            string sessionFile = GenerateFileName(sessionToken);
            if (File.Exists(sessionFile)) File.Delete(sessionFile);
        }

        public override void StoreSessionValue(string sessionToken, string key, object value)
        {
            base.StoreSessionValue(sessionToken, key, value);
            SaveSession(sessionToken);
        }

        protected virtual void SaveSession(string sessionToken)
        {
            SessionDetails details = RetrieveSessionDetails(sessionToken);
            string sessionFile = GenerateFileName(sessionToken);
            XmlDocument sessionXml = new XmlDocument();
            XmlElement sessionRoot;
            XmlElement valuesNode;
            if (File.Exists(sessionFile))
            {
                sessionXml.Load(sessionFile);
                sessionRoot = sessionXml.DocumentElement;
            }
            else
            {
                sessionRoot = sessionXml.CreateElement("session");
                sessionRoot.SetAttribute("token", sessionToken);
                sessionXml.AppendChild(sessionRoot);
            }
            sessionRoot.SetAttribute("userName", details.UserName);
            sessionRoot.SetAttribute("expiry", details.ExpiryTime.ToString("o"));

            // Wipe the existing values
            valuesNode = sessionXml.SelectSingleNode("values") as XmlElement;
            if (valuesNode != null) valuesNode.ParentNode.RemoveChild(valuesNode);

            // Add the values
            valuesNode = sessionXml.CreateElement("values");
            sessionRoot.AppendChild(valuesNode);
            foreach (string key in details.Values.Keys)
            {
                XmlElement valueNode = sessionXml.CreateElement("value");
                valueNode.SetAttribute("key", key);
                object keyValue = details.Values[key];
                valueNode.SetAttribute("type", keyValue.GetType().Name);
                valueNode.InnerText = keyValue.ToString();
                valuesNode.AppendChild(valueNode);
            }

            sessionXml.Save(sessionFile);
        }

        protected virtual string GenerateFileName(string sessionToken)
        {
            string sessionFile = Path.Combine(storeLocation, sessionToken + ".session");
            return sessionFile;
        }
    }
}
