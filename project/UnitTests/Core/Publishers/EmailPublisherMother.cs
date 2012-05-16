using System.Xml;
using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core.Publishers;
using ThoughtWorks.CruiseControl.Core.Util;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Publishers
{
	public class EmailPublisherMother
	{
		public static XmlDocument ConfigurationXml
		{
			get 
			{
				return XmlUtil.CreateDocument(
@"    	<email from=""ccnet@thoughtworks.com"" mailhost=""smtp.telus.net"" mailport=""26""
                mailhostUsername=""mailuser"" mailhostPassword=""mailpassword""
                projectUrl=""http://localhost/ccnet"" includeDetails=""false"" subjectPrefix=""CCNET:"">
            <modifierNotificationTypes>
                <NotificationType>failed</NotificationType>
                <NotificationType>fixed</NotificationType>
            </modifierNotificationTypes>
    		<users>
    		 	<user name=""buildmaster"" group=""buildmaster"" address=""servid@telus.net""/>
    		 	<user name=""orogers"" group=""developers"" address=""orogers@thoughtworks.com""/>
    		 	<user name=""manders"" group=""developers"" address=""mandersen@thoughtworks.com""/>
    		 	<user name=""dmercier"" group=""developers"" address=""dmercier@thoughtworks.com""/>
    		 	<user name=""rwan"" group=""developers"" address=""rwan@thoughtworks.com""/>
                <user name=""owjones"" group=""successdudes"" address=""oliver.wendell.jones@example.com""/>
    		</users>
    		<groups>
    			<group name=""developers""> 
                    <notifications>
                       <NotificationType>Change</NotificationType>
                     </notifications> 
                </group>
    			
                <group name=""buildmaster""> 
                    <notifications>
                       <NotificationType>Always</NotificationType>
                    </notifications> 
                </group>
                
                <group name=""successdudes""> 
                   <notifications>
                     <NotificationType>Success</NotificationType>
                   </notifications> 
                </group>
    		</groups>
			
            <converters>
				<regexConverter find=""$"" replace=""@TheCompany.com""/>
			</converters>

            <subjectSettings>
                <subject buildResult=""StillBroken"" value=""Nice try but no cigare on fixing ${CCNetProject}"" />
            </subjectSettings>
    	</email>");
			}
		}

		public static EmailPublisher Create()
		{
			return Create(ConfigurationXml.DocumentElement);
		}

		public static EmailPublisher Create(XmlNode node)
		{
			return NetReflector.Read(node) as EmailPublisher;
		}
	}
}
