﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.Remote;
using ThoughtWorks.CruiseControl.Remote.Messages;

namespace ThoughtWorks.CruiseControl.CCTrayLib.Security
{
    [Extension(DisplayName="WinLogin authentication")]
    public class WinLoginAuthentication
        : IAuthenticationMode
    {
        private string settings;

        public string Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public bool Configure(IWin32Window owner)
        {
            MessageBox.Show("There are no configurable settings for this authentication", "WinLogin authentication", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return true;
        }

        public bool Validate()
        {
            return true;
        }

        public LoginRequest GenerateCredentials()
        {
            LoginRequest credentials = new LoginRequest(Environment.UserName);
            credentials.AddCredential(LoginRequest.DomainCredential, Environment.UserDomainName);
            return credentials;
        }
    }
}
