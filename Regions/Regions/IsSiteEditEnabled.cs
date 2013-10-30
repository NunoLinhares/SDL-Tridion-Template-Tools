using System;
using System.Text;
using System.Xml;

namespace Sdl.Tridion.Community.Regions
{

    public static class PublicationTargetExtensions 
    {
        public static bool IsSiteEditEnabled(this PublicationTarget target)
        {
            try
            {
                if (target == null)
                    return true;
                ApplicationData appData = target.LoadApplicationData("SiteEdit");
                string data = Encoding.UTF8.GetString(appData.Data);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(data);

                XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());
                nsManager.AddNamespace("se", "http://www.sdltridion.com/2011/SiteEdit");

                XmlNode seNode = xmlDoc.SelectSingleNode("/se:configuration/se:PublicationTarget/se:EnableSiteEdit",
                                                         nsManager);
                if (seNode != null && seNode.InnerText.Equals("true"))
                    return true;
                return false;

            }
            catch (Exception)
            {
                //AAngel: Quick fix to make the pages publishable to live, probably can be done better. They were failing when the target is not Site Edit enabled
                return false;
                
            }
            
        }
    }
}
