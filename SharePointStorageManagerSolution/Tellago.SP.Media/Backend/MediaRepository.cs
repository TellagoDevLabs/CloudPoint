using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Client;
using Tellago.SP.Media.Model;

namespace Tellago.SP.Media.Backend
{
    internal class MediaRepository
    {
        private string mediaAssetsList;
        private string webFullUrl;

        
        public MediaRepository(string webFullUrl,string mediaAssetsList)
        {
            this.webFullUrl = webFullUrl;
            this.mediaAssetsList = mediaAssetsList;
        }

        public IEnumerable<MediaAsset> GetMediaToProcess()
        {
            ClientContext ctx = new ClientContext(webFullUrl);
            Web web = ctx.Web;

            
            List list = web.Lists.GetByTitle(mediaAssetsList);
                  
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml =
                         @"<View>
                            <Query>
                              <Where>                                
                                <Eq>
                                    <FieldRef Name='MediaProcStatus'/>
                                    <Value Type='Text'>Pending</Value>
                                </Eq>                                
                              </Where>
                            </Query>
                            <RowLimit>100</RowLimit>
                          </View>";
            ListItemCollection listItems = list.GetItems(camlQuery);
            ctx.Load(
                    listItems,
                    items => items
                        .Include(
                            item => item.Id,
                            item => item["Title"],
                            item => item.ContentType,
                            item => item["MediaTempLocation"],
                            item => item["Author"]));
            ctx.ExecuteQuery();
            foreach (ListItem item in listItems)
            {
                MediaAsset asset = new MediaAsset();
                asset.Id = item.Id;
                asset.Title = item["Title"].ToString();
                asset.TempLocation = item["MediaTempLocation"] != null ? item["MediaTempLocation"].ToString() : String.Empty;
                if (item.ContentType.Name.Contains(MediaType.Audio.ToString()))
                {
                    asset.MediaType = MediaType.Audio;
                }
                else if (item.ContentType.Name.Contains(MediaType.Image.ToString()))
                {
                    asset.MediaType = MediaType.Image;
                }
                else
                {
                    asset.MediaType = MediaType.Video;
                }
                var user = item.FieldValues["Author"] as FieldUserValue;
                asset.UploaderEmail = GetUserEmail(user.LookupId);
                yield return asset;              

            }

        }

        public void UpdateMediaAsset(int assetId, string format, string tempLocation, string newLocationUrl, string thumbnailUrl, string posterUrl, ProcessingStatus status,DateTime? expirationDate)
        {
            ClientContext clientContext = new ClientContext(webFullUrl);
            List list = clientContext.Web.Lists.GetByTitle(mediaAssetsList);
            ListItem item = list.GetItemById(assetId);
            if (!String.IsNullOrEmpty(format))
            {
                item["MediaFormat"] = format;
            }
            item["MediaTempLocation"] = tempLocation;
            item["MediaLocation"] = newLocationUrl;
            item["MediaThumbnail"] = thumbnailUrl;
            item["MediaPoster"] = posterUrl;
            item["MediaProcStatus"] = Enum.GetName(typeof(ProcessingStatus), status);
            if (expirationDate.HasValue)
            {
                item["MediaExpirationDate"] = expirationDate.Value;
            }
            item.Update();

            clientContext.ExecuteQuery(); 
        }

        protected string GetUserEmail(int lookupId)
        {
            try
            {
                using (ClientContext clientContext = new ClientContext(webFullUrl))
                {
                    ListItem userInfo = clientContext.Web.SiteUserInfoList.GetItemById(lookupId);
                    clientContext.Load(userInfo);
                    clientContext.ExecuteQuery();
                    var email = userInfo["EMail"];
                    if (email != null)
                    {
                        return email.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new MediaProcessingException(String.Format("Could not get email from user with id '{0}' ", lookupId),ex);       
            }
            return String.Empty;

        }
    }
}
