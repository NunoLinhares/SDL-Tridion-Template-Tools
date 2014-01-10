using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Publishing;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.ContentManager.Workflow;

namespace TridionTemplates
{
    [TcmTemplateTitle("Get Item version in workflow")]
    public class GetItemInWorkflow : ITemplate
    {
        private TemplatingLogger _log;
        private Engine _engine;
        public void Transform(Engine engine, Package package)
        {
            _log = TemplatingLogger.GetLogger(GetType());
            _engine = engine;

            string itemName = string.Empty;
            if (package.GetByName(Package.ComponentName) != null) itemName = Package.ComponentName;
            if (package.GetByName(Package.PageName) != null) itemName = Package.PageName;

            if (string.IsNullOrEmpty(itemName))
            {
                _log.Debug("Could not determine template type, exiting");
                return;
            }

            VersionedItem item = (VersionedItem)engine.GetObject(package.GetByName(itemName));
            if (item.LockType.HasFlag(LockType.InWorkflow))
            {
                CurrentMode mode = GetCurrentMode();
                VersionedItem w = item.GetVersion(0);
                switch (mode)
                {
                    case CurrentMode.CmePreview:
                    case CurrentMode.TemplateBuilder:
                    case CurrentMode.SessionPreview:
                        // return workflow object without comparing to Publication Target Minimum Approval Status
                        package.Remove(package.GetByName(itemName));
                        if (itemName.Equals(Package.ComponentName))
                        {
                            package.PushItem(Package.ComponentName, package.CreateTridionItem(ContentType.Component, w));
                        }
                        else if (itemName.Equals(Package.PageName))
                        {
                            package.PushItem(Package.PageName, package.CreateTridionItem(ContentType.Page, w));
                        }
                        break;

                    case CurrentMode.Publish:
                        PublicationTarget target = _engine.PublishingContext.PublicationTarget;
                        ApprovalStatus targetStatus = target.MinApprovalStatus;
                        ApprovalStatus contentStatus = null;
                        if (w is Component)
                        {
                            contentStatus = ((Component)w).ApprovalStatus;
                        }
                        else if (w is Page)
                        {
                            contentStatus = ((Page)w).ApprovalStatus;
                        }
                        if (contentStatus == null)
                        {
                            _log.Debug("Could not determine approval status of content. Exiting.");
                            return;
                        }
                        bool mustUpdate = false;
                        if (targetStatus == null)
                            mustUpdate = true;
                        else
                        {
                            if (contentStatus.Position > targetStatus.Position)
                                mustUpdate = true;
                        }
                        
                        if (mustUpdate)
                        {
                            package.Remove(package.GetByName(itemName));
                            if (itemName.Equals(Package.ComponentName))
                            {
                                package.PushItem(Package.ComponentName, package.CreateTridionItem(ContentType.Component, w));
                            }
                            else if (itemName.Equals(Package.PageName))
                            {
                                package.PushItem(Package.PageName, package.CreateTridionItem(ContentType.Page, w));
                            }
                        }
                        break;
                }
            }

        }

        private CurrentMode GetCurrentMode()
        {
            RenderMode renderMode = _engine.RenderMode;
            if (renderMode == RenderMode.Publish) return CurrentMode.Publish;


            if (renderMode == RenderMode.PreviewDynamic)
            {
                if (_engine.PublishingContext.PublicationTarget == null) return CurrentMode.TemplateBuilder;
                PublicationTarget target = _engine.PublishingContext.PublicationTarget;
                if (target.Id.Equals(TcmUri.UriNull)) return CurrentMode.CmePreview;
                return CurrentMode.SessionPreview;
            }
            return CurrentMode.Unknown;
        }

        private enum CurrentMode
        {
            TemplateBuilder,
            CmePreview,
            SessionPreview,
            Publish,
            Unknown
        }
    }
}
