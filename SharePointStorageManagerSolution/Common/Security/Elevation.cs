using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SPSM.Common.Security
{
    public static class Elevation
    {
        public static void OnWeb(SPWeb unelevatedWeb, Action<SPWeb> action)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                using (var site = new SPSite(unelevatedWeb.Site.ID))
                {
                    using (var elevatedWeb = site.OpenWeb(unelevatedWeb.ID))
                    {
                        action(elevatedWeb);
                    }
                }
            });
        }

        public static void OnItem(SPListItem unelevatedItem, Action<SPListItem> action)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                using (var site = new SPSite(unelevatedItem.Web.Site.ID))
                {
                    using (var web = site.OpenWeb(unelevatedItem.Web.ID))
                    {
                        SPList list = web.Lists[unelevatedItem.ParentList.ID];
                        var safeItem = list.GetItemById(unelevatedItem.ID);
                        action(safeItem);
                    }
                }
            });
        }
    }
}
