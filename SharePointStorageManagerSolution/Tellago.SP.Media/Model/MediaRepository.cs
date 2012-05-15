using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace Tellago.SP.Media.Model
{
    public class MediaAssetsColumns
    {
        public const string TempLocation = "{5FEBB2A5-975A-4202-9590-8A1821A860DF}";
        public const string Location = "{DF7E5058-FC7C-4999-ABDF-D88571A9614F}";
        public const string Format = "{DD1B5921-B7FF-463D-A6D2-954C7B09AC06}";
        public const string Thumbnail = "{17124F84-8BD7-4DC5-8411-EA832612EAD9}";
        public const string ProcStatus = "{07597702-95E9-4E9B-A41C-9DEF98A71986}";
        public const string Length = "{DE38F937-8578-435e-8CD3-50BE3EA59253}";
        public const string Poster = "{38B9AC3A-B312-4103-976E-1E69083C7502}";
    }

    class MediaRepository
    {

        public void Insert(SPList mediaList, MediaAsset asset, out int id, out string contentTypeId)
        {

            var item = mediaList.Items.Add();
            item["ContentTypeId"] = GetContentTypeId(mediaList, asset.MediaType.ToString());
            item["Title"] = asset.Title;
            item[new Guid(MediaAssetsColumns.TempLocation)] = asset.TempLocation;
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
                item[new Guid(MediaAssetsColumns.Poster)] = asset.Poster;
            }
            if ((asset.MediaType == MediaType.Audio || asset.MediaType == MediaType.Video) && asset.Duration != TimeSpan.MinValue)
            {
                item[new Guid(MediaAssetsColumns.Length)] = asset.Duration.TotalSeconds;
            }
            item.Update();

            id = item.ID;
            contentTypeId = item.ContentTypeId.ToString();
        }

        private object GetContentTypeId(SPList mediaList, string mediaType)
        {
            foreach (SPContentType ct in mediaList.ContentTypes)
            {

                if (!String.IsNullOrEmpty(ct.Name) && ct.Name.Contains(mediaType))
                {
                    return ct.Id;
                }
            }
            throw new ArgumentException(String.Format("No content type containing '{0}' was found on list '{1}'", mediaType.ToString(), mediaList.Title));
        }


    }
}
