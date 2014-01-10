using System.Collections.Generic;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Publishing.Rendering;
using Tridion.ContentManager.Security;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace TridionTemplates
{
    public class AddUserInfoToPublishInstruction : ITemplate
    {
        /// <summary>
        /// Transforms the specified engine.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="package">The package.</param>
        public void Transform(Engine engine, Package package)
        {
            if (engine.PublishingContext.PublicationTarget == null) return;

            TemplatingLogger log = TemplatingLogger.GetLogger(GetType());
            List<Group> groups = new List<Group>();

            if (engine.PublishingContext.PublicationTarget.TargetTypes.Count > 0)
            {
                Session session = new Session();
                TargetType currentTargetType = (TargetType)session.GetObject(engine.PublishingContext.PublicationTarget.TargetTypes[0].Id);

                foreach (AccessControlEntry accessControlEntry in currentTargetType.AccessControlList.AccessControlEntries)
                {
                    if (accessControlEntry.Trustee is Group)
                    {
                        groups.Add((Group)accessControlEntry.Trustee);
                    }
                    if (accessControlEntry.Trustee is User)
                    {
                        log.Error("Target type " + currentTargetType.Title + " contains a security entry for user " + accessControlEntry.Trustee.Title + ". This is incorrect, only groups should be in this list.");
                    }
                }
            }

            XmlDocument document = new XmlDocument();
            foreach (Group group in groups)
            {
                XmlElement groupInfo = document.CreateElement("GroupInfo");
                groupInfo.SetAttribute("Name", group.Title);
                IEnumerable<User> usersIngroup = GetUsersInGroup(group);
                foreach (User user in usersIngroup)
                {
                    XmlElement userInfo = document.CreateElement("UserInfo");
                    userInfo.SetAttribute("Name", user.Title);
                    userInfo.SetAttribute("Email", user.Description);
                    groupInfo.AppendChild(userInfo);
                }
                engine.PublishingContext.RenderedItem.AddInstruction(InstructionScope.Global, groupInfo);
            }
        }

        /// <summary>
        /// Gets all the users in a group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        private IEnumerable<User> GetUsersInGroup(Group group)
        {
            List<User> result = new List<User>();

            foreach (Trustee trustee in group.GetGroupMembers())
            {
                if (trustee is User)
                {
                    result.Add((User)trustee);
                }
                else
                {
                    result.AddRange(GetUsersInGroup((Group)trustee));
                }
            }
            return result;
        }
    }
}
