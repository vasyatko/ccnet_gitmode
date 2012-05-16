﻿
using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;

namespace ThoughtWorks.CruiseControl.Core.Util
{
    public class LdapHelper : ILdapService
    {
        //Ldap explanation
        //http://www.computerperformance.co.uk/Logon/LDAP_attributes_active_directory.htm#Hall_of_fame_LDAP_attribute_-_DN__distinguished_name_

        /// <summary>
        /// Default constructor
        /// </summary>
        public LdapHelper()
            : this(null, null, null)
        { }

        /// <summary>
        /// Extented constructor
        /// </summary>
        /// <param name="domainName"></param>
        public LdapHelper(string domainName)
            : this(domainName, null, null)
        { }

        /// <summary>
        /// Extended constructor
        /// </summary>
        /// <param name="domainName"></param>
        /// <param name="logonUser"></param>
        /// <param name="logOnPassword"></param>
        public LdapHelper(string domainName, string logonUser, string logOnPassword)
        {
            DomainName = domainName;
            LdapLogonUserName = logonUser;
            LdapLogonPassword = logOnPassword;

            LdapFieldMailAddress = "mail";
            LdapFieldSurName = "sn";
            LdapFieldName = "name";
            LdapFieldCommonName = "cn";
            LdapFieldGivenName = "givenname";
            LdapFieldDisplayName = "displayname";
            LdapFieldMailNickName = "mailnickname";
        }


        /// <summary>
        /// The domain to use, get the information from, authenticate users
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// the username to use for logging into the ldap service
        /// this is NOT the username to retrieve information from
        /// </summary>
        public string LdapLogonUserName { get; set; }

        /// <summary>
        /// the password of the LdapLogonUser
        /// </summary>
        public string LdapLogonPassword { get; set; }

        /// <summary>
        /// A simple SMTP address
        /// Default value : mail
        /// <example>
        /// mail = John.Wayne@texas.com
        /// </example>
        /// </summary>
        public string LdapFieldMailAddress { get; set; }

        /// <summary>
        /// This would be referred to as last name or surname.
        /// Default value : sn
        /// </summary>
        /// <example>
        /// SN = Thomas
        /// </example>
        public string LdapFieldSurName { get; set; }


        /// <summary>
        /// Exactly the same as CN
        /// Default value : name
        /// <example>
        /// name = Guy Thomas
        /// </example>
        /// </summary>
        public string LdapFieldName { get; set; }

        /// <summary>
        /// This LDAP attribute is made up from givenName joined to SN.
        /// Default value : CN
        /// <example>
        /// CN=Guy Thomas
        /// </example>
        /// </summary>
        public string LdapFieldCommonName { get; set; }

        /// <summary>
        /// Firstname also called Christian name
        /// Default value : GivenName
        /// </summary>
        private string LdapFieldGivenName { get; set; }

        /// <summary>
        /// If you script this property, be sure you understand which field you are configuring.
        /// DisplayName can be confused with CN or description.
        /// Default Value : displayname
        /// <example>
        /// displayName = Guy Thomas.
        /// </example>
        /// </summary>
        public string LdapFieldDisplayName { get; set; }

        /// <summary>
        /// Normally this is the same value as the sAMAccountName, but could be different if you wished. Needed for mail enabled contacts.
        /// Default value : MailNickName
        /// <example>
        /// MailNickName = Johny
        /// </example>
        /// </summary>
        public string LdapFieldMailNickName { get; set; }



        public LdapUserInfo RetrieveUserInformation(string userNameToRetrieveFrom)
        {
            System.DirectoryServices.DirectorySearcher LdapSearcher = new System.DirectoryServices.DirectorySearcher();
            System.DirectoryServices.SearchResult LdapResult = default(System.DirectoryServices.SearchResult);

            string filter = "(&(objectClass=user)(SAMAccountName=" + userNameToRetrieveFrom + "))";


            System.DirectoryServices.DirectoryEntry Ldap = default(System.DirectoryServices.DirectoryEntry);

            try
            {

                if (LdapLogonUserName == null)
                {
                    Ldap = new System.DirectoryServices.DirectoryEntry("LDAP://" + DomainName);
                }
                else
                {
                    Ldap = new System.DirectoryServices.DirectoryEntry("LDAP://" + DomainName, LdapLogonUserName, LdapLogonPassword);
                }
            }
            catch (Exception e)
            {
                Util.Log.Trace(e.ToString());
                throw new Exception("Problem connecting to LDAP service", e);
            }

            LdapSearcher.SearchRoot = Ldap;
            LdapSearcher.SearchScope = SearchScope.Subtree;

            LdapSearcher.PropertiesToLoad.Add(LdapFieldMailAddress);
            LdapSearcher.PropertiesToLoad.Add(LdapFieldName);
            LdapSearcher.PropertiesToLoad.Add(LdapFieldSurName);
            LdapSearcher.PropertiesToLoad.Add(LdapFieldCommonName);
            LdapSearcher.PropertiesToLoad.Add(LdapFieldGivenName);
            LdapSearcher.PropertiesToLoad.Add(LdapFieldDisplayName);
            LdapSearcher.PropertiesToLoad.Add(LdapFieldMailNickName);

            LdapSearcher.Filter = filter;
            LdapResult = LdapSearcher.FindOne();
            LdapSearcher.Dispose();

            LdapUserInfo result = new LdapUserInfo();

            if ((LdapResult != null))
            {
                result.CommonName = (string)LdapResult.GetDirectoryEntry().Properties[LdapFieldCommonName].Value;
                result.DisplayName = (string)LdapResult.GetDirectoryEntry().Properties[LdapFieldDisplayName].Value;
                result.GivenName = (string)LdapResult.GetDirectoryEntry().Properties[LdapFieldGivenName].Value;
                result.MailAddress = (string)LdapResult.GetDirectoryEntry().Properties[LdapFieldMailAddress].Value;
                result.MailNickName = (string)LdapResult.GetDirectoryEntry().Properties[LdapFieldMailNickName].Value;
                result.Name = (string)LdapResult.GetDirectoryEntry().Properties[LdapFieldName].Value;
                result.SurName = (string)LdapResult.GetDirectoryEntry().Properties[LdapFieldSurName].Value;
            }

            return result;
        }

        /// <summary>
        /// Attempts to authenticate the supplied user credentials using DirectoryServices.
        /// </summary>
        /// <param name="userName">The user name to be authenticated.</param>
        /// <param name="password">Password, if needed, for the given user name.</param>
        /// <param name="domainName">Domain name (path) of the domain providing the directory service.</param>
        /// <returns>True if the supplied credentials are valid on the given domain, false otherwise.</returns>
        public bool Authenticate(string userName, string password, string domainName)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(domainName))
                return false;

            // Only supporting SECURE authentication for now, could later support SSL.  
            AuthenticationTypes atAuthentType = AuthenticationTypes.Secure;  
            
            bool isAuthenticUser = false;

            using (DirectoryEntry deDirEntry = new DirectoryEntry("LDAP://" + domainName, userName, password, atAuthentType))
             {
                // This seems a tad 'hacky', but is the only way I can see to validate a user. 
                // It relies on an exception being thrown when attempting to access a data member
                // in the DirectoryEntry instance if the user credentials were invalid.  

                try
                {
                    if ( string.IsNullOrEmpty(deDirEntry.Name) ) // throws exception for invalid user cridentials. 
                        isAuthenticUser = false;

                    else
                        isAuthenticUser = true;
                }
                catch 
                {
                    isAuthenticUser = false;
                }
             }
            
            return isAuthenticUser;
        }
    }
}
