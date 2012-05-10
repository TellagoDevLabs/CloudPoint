using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.IO;
using System.Collections;
using SPSM.Common.Media.Model;
using SPSM.Common.Security;

namespace SPSM.Common.Media
{   
    
    public class MediaAssetsColumns
    {
        public const string TempLocation = "{482E5650-CFD1-4585-B05D-DE65B588921D}";
        public const string Location = "{0C8EA650-FA42-47E1-9942-7928F5C6321F}";
        public const string OriginatingWeb = "{C3AFB835-1AD3-4B31-974C-BFC4E7071A0D}";
        public const string Format = "{B46D3B0A-0C7B-4C3C-963E-E8C3F5155B96}";
        public const string Thumbnail = "{E4C56391-FADD-492A-8EEB-78ACF66856BB}";
        public const string ProcStatus="{5F0BC128-5D75-4DC5-AB70-312C6D425F9F}";
        public const string Length = "{DE38F937-8578-435e-8CD3-50BE3EA59253}";
        public const string DetailThumbnail = "{CA3E1C63-AB3E-423E-820F-D88615D09E77}";
    }

    public class MediaRepository
    {       

        public string Insert(SPWeb web, string listName, MediaAsset asset)
        {
            var mediaList = web.Lists.TryGetList(listName);
            if (mediaList == null)
            {
                throw new ArgumentException(String.Format("Media List '{0}' was not found on web '{1}'",listName,web.Url));
            }
            var item = mediaList.Items.Add();
            item["ContentTypeId"] = GetContentTypeId(mediaList,asset.MediaType.ToString());
            item["Title"] = asset.Title;
            item[new Guid(MediaAssetsColumns.TempLocation)] = asset.TempLocation;
            item[new Guid(MediaAssetsColumns.OriginatingWeb)] = asset.OriginatingWeb;
            item[new Guid(MediaAssetsColumns.Format)] = asset.Format;
            item[new Guid(MediaAssetsColumns.ProcStatus)] = Enum.GetName(typeof(ProcessingStatus), asset.Status);
            if (!String.IsNullOrEmpty(asset.Location))
            {
                item[new Guid(MediaAssetsColumns.Location)] = asset.Location;
            }
            if (!String.IsNullOrEmpty(asset.Thumbnail))
            {
                item[new Guid(MediaAssetsColumns.Thumbnail)] = asset.Thumbnail;
            }
            if (!String.IsNullOrEmpty(asset.Poster))
            {
                item[new Guid(MediaAssetsColumns.DetailThumbnail)] = asset.Poster;
            }
            if ((asset.MediaType == MediaType.Audio || asset.MediaType == MediaType.Video) && asset.Duration != TimeSpan.MinValue)
            {
                item[new Guid(MediaAssetsColumns.Length)] = asset.Duration.TotalSeconds;
            }

            item.Update();

            SetItemPermissions(item, web.CurrentUser, SPRoleType.Contributor);
            
            string editForm = String.Format("{0}?ID={1}&ContentTypeId={2}&upl=true", mediaList.DefaultEditFormUrl, item.ID, item.ContentTypeId.ToString());
            return editForm;
        }

        /// <summary>
        /// Gives Contributor permissions to item creator (this is needed for example for Team Site Visitors to be able to delete its own media items)
        /// </summary>
        private void SetItemPermissions(SPListItem unelevatedItem, SPPrincipal userOrGroup, SPRoleType roleType)
        {
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                using (var site = new SPSite(unelevatedItem.Web.Site.ID))
                {
                    using (var web = site.OpenWeb(unelevatedItem.Web.ID))
                    {
                        SPList list = web.Lists[unelevatedItem.ParentList.ID];
                        var safeItem = list.GetItemById(unelevatedItem.ID);
                        web.AllowUnsafeUpdates = true;
                        try
                        {
                            if (!safeItem.HasUniqueRoleAssignments)
                            {
                                safeItem.BreakRoleInheritance(true);
                                web.AllowUnsafeUpdates = true;
                            }
                            SPRoleAssignment roleAssignment = new SPRoleAssignment(userOrGroup);
                            SPRoleDefinition roleDefinition = safeItem.Web.RoleDefinitions.GetByType(roleType);
                            roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                            safeItem.RoleAssignments.Add(roleAssignment);
                            safeItem.Update();
                        }
                        finally
                        {
                            web.AllowUnsafeUpdates = false;                          
                        }
                    }
                }
            });
        }

        private object GetContentTypeId(SPList mediaList,string mediaType)
        {
            foreach (SPContentType ct in mediaList.ContentTypes)
            {
                
                if (!String.IsNullOrEmpty(ct.Name) &&  ct.Name.Contains(mediaType))
                {
                    return ct.Id;
                }
            }
            throw new ArgumentException(String.Format("No content type containing '{0}' was found on list '{1}'", mediaType.ToString(), mediaList.Title));
        }

      
    }
}
