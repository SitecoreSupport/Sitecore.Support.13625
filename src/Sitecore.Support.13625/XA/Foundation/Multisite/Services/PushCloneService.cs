namespace Sitecore.Support.XA.Foundation.Multisite.Services
{
  using Sitecore;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.XA.Foundation.Multisite.Services;
  using System.Collections.Generic;
  using System.Linq;

  public class PushCloneService : Sitecore.XA.Foundation.Multisite.Services.PushCloneService, IPushCloneService
  {
    private readonly IPushCloneCoordinatorService _coordinatorService;

    public PushCloneService(IPushCloneCoordinatorService pushCloneCoordinatorService) : base(pushCloneCoordinatorService)
    {
      _coordinatorService = pushCloneCoordinatorService;
    }

    public new void AddVersion(Item item)
    {
      Item parent = item.Parent;
      Item latestVersion = item.Versions.GetLatestVersion();
      ItemUri uri = latestVersion.Uri;
      IEnumerable<Item> cloneItem = GetCloneItem(latestVersion);
      IList<Item> list = (cloneItem as IList<Item>) ?? cloneItem.ToList();
      if (!list.Any() && parent.HasClones)
      {
        foreach (Item item4 in GetCloneItem(parent))
        {
          if (_coordinatorService.ShouldProcess(item4))
          {
            Item item2 = item.CloneTo(item4);
            CopyWorkflow(item, item2);
            ProtectItem(item2);
          }
        }
      }
      else
      {
        foreach (Item item5 in list)
        {
          if (!_coordinatorService.ShouldProcess(item5))
          {
            #region Modified code
            continue;
            #endregion
          }
          Item item3 = item5.Database.GetItem(item5.ID, latestVersion.Language).Versions.AddVersion();
          item3.Editing.BeginEdit();
          item3[FieldIDs.Source] = uri.ToString();
          item3[FieldIDs.SourceItem] = uri.ToString(false);
          item3.Editing.EndEdit();
        }
      }
    }
  }
}