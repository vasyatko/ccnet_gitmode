using System.Net;
using System.Net.Mail;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.Core.Publishers
{
    public class EmailGateway
    {
        private string mailhostUsername = null;
        private PrivateString mailhostPassword = null;
        private SmtpClient smtpServer;

        public EmailGateway()
        {
            smtpServer = new SmtpClient();
        }

        public virtual string MailHost
        {
            get { return smtpServer.Host; }
            set { smtpServer.Host = value; }
        }

        public virtual int MailPort
        {
            get { return smtpServer.Port; }
            set { smtpServer.Port = value; }
        }

        public bool UseSSL
        {
            get { return smtpServer.EnableSsl ; }
            set { smtpServer.EnableSsl = value; }
        }

        public string MailHostUsername
        {
            get { return mailhostUsername; }
            set { mailhostUsername = value; }
        }

        public PrivateString MailHostPassword
        {
            get { return mailhostPassword; }
            set { mailhostPassword = value; }
        }

        public virtual void Send(MailMessage mailMessage)
        {
            if (MailHostUsername != null && MailHostPassword != null)
            {
                smtpServer.Credentials = new NetworkCredential(mailhostUsername, mailhostPassword.PrivateValue);
            }
            smtpServer.Send(mailMessage);
        }
    }
}