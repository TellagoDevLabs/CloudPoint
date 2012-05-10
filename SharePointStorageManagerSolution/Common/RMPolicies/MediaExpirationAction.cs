using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.RecordsManagement.PolicyFeatures;
using Microsoft.SharePoint;
using SPSM.Common.Logging;
using SPSM.Common.AssetStorage;
using Microsoft.SharePoint.Administration;

namespace SPSM.Common.Media.RMPolicies
{
    public class MediaExpirationAction : IExpirationAction
    {
        private Logger logger = new Logger();

        #region IExpirationAction Members

        public void OnExpiration(SPListItem item, System.Xml.XmlNode parametersData, DateTime expiredDate)
        {
            try
            {
                DeleteMediaFiles(item);
                item.Delete();
            }
            catch (Exception ex)
            {
                logger.LogToOperations(ex, SPSMCategories.Media, EventSeverity.Error, "Error executing MediaExpirationAction on item with ID '{0}' and Title: '{1}'", item.ID, item.Title);
            }
        }

        #endregion

        public static void DeleteMediaFiles(SPListItem item)
        {
            IAssetStorageManager storageManager = AssetStorageFactory.GetStorageManager("Media", item.Web.Url);

            string tempLocation = item[new Guid(MediaAssetsColumns.TempLocation)] != null ? item[new Guid(MediaAssetsColumns.TempLocation)].ToString() : null;
            string mediaLocation = item[new Guid(MediaAssetsColumns.Location)] != null ? item[new Guid(MediaAssetsColumns.Location)].ToString() : null;
            string thumbLocation = item[new Guid(MediaAssetsColumns.Thumbnail)] != null ? item[new Guid(MediaAssetsColumns.Thumbnail)].ToString() : null;
            string posterLocation = item[new Guid(MediaAssetsColumns.DetailThumbnail)] != null ? item[new Guid(MediaAssetsColumns.DetailThumbnail)].ToString() : null;

            if (!String.IsNullOrEmpty(tempLocation))
            {
                FileUtils.Delete(tempLocation);
            }
            if (! String.IsNullOrEmpty(mediaLocation))
            {
                string location = new SPFieldUrlValue(mediaLocation).Url;
                storageManager.Delete(location);
            }

            if ( ! String.IsNullOrEmpty(thumbLocation))
            {
                string thumbnail = new SPFieldUrlValue(thumbLocation).Url;
                storageManager.Delete(thumbnail);
            }

            if (!String.IsNullOrEmpty(posterLocation))
            {
                string thumbnail = new SPFieldUrlValue(posterLocation).Url;
                storageManager.Delete(thumbnail);
            }
        }

    }
}
