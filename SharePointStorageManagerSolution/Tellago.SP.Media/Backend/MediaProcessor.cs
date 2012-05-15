using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.IO;
using Microsoft.SharePoint.Administration;
using System.Collections.Specialized;
using Tellago.SP.Media.Model;
using Tellago.SP.Media.AssetStorage;
using Microsoft.SharePoint.Utilities;
using Tellago.SP.Logging;

namespace Tellago.SP.Media.Backend
{
    public class MediaProcessor
    {
        private MediaRepository mediaRepository;
        private VideoProcessor videoProcessor;
        private IAssetStorageManager storageManager;
        private MediaConfig config;
        //EmailManager emailManager = new EmailManager();
        private string webFullUrl;
        private Logger logger = new Logger();

        public MediaProcessor(string webFullUrl, MediaConfig config)
        {
            mediaRepository = new MediaRepository(webFullUrl, config.MediaAssetsListName);
            storageManager = AssetStorageFactory.GetStorageManager("Media", webFullUrl);
            videoProcessor = new VideoProcessor(config);
            this.config = config;
            this.webFullUrl = webFullUrl;
        }

        public void ProcessMedia()
        {

                var mediaToProcess = mediaRepository.GetMediaToProcess();
                foreach (MediaAsset asset in mediaToProcess)
                {
                    string newLocationUrl = String.Empty;
                    string thumbnailUrl = String.Empty;
                    string posterUrl = String.Empty;
                    string format = null;
                    FileInfo encodedVideo=null, thumbnail=null, poster=null;
                    FileInfo assetFile = new FileInfo(asset.TempLocation);
                    try
                    {
                        //process media
                        if (!assetFile.Exists)
                        {
                            throw new MediaProcessingException(String.Format("Asset Temp File '{0}' does not exist.", assetFile.FullName));
                        }

                        videoProcessor.EncodeVideo(assetFile, out encodedVideo, out thumbnail, out poster);
                        format = MediaAsset.GetMediaFormat(encodedVideo);
                        //upload to definite location
                        newLocationUrl = storageManager.Save(encodedVideo);
                        thumbnailUrl = storageManager.Save(thumbnail);
                        posterUrl = storageManager.Save(poster);

                        //update data in assets list
                        mediaRepository.UpdateMediaAsset(asset.Id, format, null, newLocationUrl, thumbnailUrl, posterUrl, ProcessingStatus.Success, null);

                        SendEmail(asset.UploaderEmail, asset.Id, asset.Title,null);
                    }
                    catch (Exception ex)
                    {
                        SendEmail(asset.UploaderEmail, asset.Id, asset.Title, ex);
                        logger.LogToOperations(ex, LogCategories.Media, EventSeverity.Error, "Error processing asset with Id: '{0}' and TempLocation: '{1}'", asset.Id, asset.TempLocation);
                        try
                        {
                            mediaRepository.UpdateMediaAsset(asset.Id, format, asset.TempLocation, newLocationUrl, thumbnailUrl, posterUrl, ProcessingStatus.Error, DateTime.Now);
                        }
                        catch (Exception ex2)
                        {
                            logger.LogToOperations(ex2, LogCategories.Media, EventSeverity.Error, "Error updating ProcessingResult to Error for asset with Id: '{0}' and TempLocation: '{1}'", asset.Id, asset.TempLocation);
                        }
                    }
                    finally
                    {
                        FileUtils.TryDelete(encodedVideo);
                        FileUtils.TryDelete(thumbnail);
                        FileUtils.TryDelete(poster);
                        FileUtils.TryDelete(assetFile);
                    }
                }

        }

     
        private bool SendEmail(string recipient,int itemId,string title,Exception ex)
        {
            string subject = null;
            string body = null;
            if (ex != null)
            {
                subject = String.Format("The video '{0}' faild to process", title);
                body = String.Format("The video with id '{0}' and title '{1}' failed to process. Error: '{2}'", itemId, title, ex.ToString());
            }
            else 
            {
                subject = String.Format("The video '{0}' was successfully processed", title);
                body = String.Format("The video with id '{0}' and title '{1}' was successfully processed.", itemId, title);
            }
            StringDictionary headers = new StringDictionary();
            headers.Add("from", "admin@sharepoint.com");
            headers.Add("to", recipient);
            headers.Add("subject", subject);
            using (SPSite site = new SPSite(webFullUrl))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    var success = SPUtility.SendEmail(web, headers, body);
                    return success;  
                }
            }
        }

        
    }


}
