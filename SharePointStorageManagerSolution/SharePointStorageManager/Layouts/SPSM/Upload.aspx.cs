﻿using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.IO;
using Microsoft.SharePoint.Utilities;
using SPSM.Common.Logging;
using System.Threading;
using System.Web.UI;
using SPSM.Common.Media.Model;
using SPSM.Common.AssetStorage;
using SPSM.Common.Media.Validators;

namespace SPSM.Common.Media.Layouts.SPSM
{
    public partial class Upload : LayoutsPageBase
    {
        private Logger logger = new Logger();
        private MediaRepository mediaRepository = new MediaRepository();
        protected MediaConfig mediaConfig = MediaConfig.FromConfigStore();

        protected void Page_Load(object sender, EventArgs e)
        {
            FileExtensionValidator.ValidationExpression = mediaConfig.FileExtensionValidationExpression;
            ThumbnailFileExtensionValidator.ValidationExpression = mediaConfig.ImageFileExtensionValidationExpression;
            PosterFileExtensionValidator.ValidationExpression = mediaConfig.ImageFileExtensionValidationExpression;

            InputFormSection2.Visible = false;//Hide thumbnail and poster image upload for Portal
        }
        
        protected void btnOk_Click(object sender, EventArgs e)
        {
            if (FileToUpload.PostedFile == null || String.IsNullOrEmpty(FileToUpload.PostedFile.FileName))
                return;

            PostFile();
           
        }

        private void PostFile()
        {
            bool thumbUploaded = false, posterUploaded = false;
            string destFolder = mediaConfig.TempLocationFolder;
            var originFileInfo = new FileInfo(FileToUpload.PostedFile.FileName);
            try
            {
                SPWeb web = SPContext.Current.Web;
                MediaAsset asset = MediaAsset.FromFile(originFileInfo, SPContext.Current.Web.Url, mediaConfig);

                using (SPLongOperation longOperation = new SPLongOperation(this.Page))
                {
                    longOperation.Begin();

                    asset.FileSize = FileToUpload.PostedFile.ContentLength;

                    var validator = ValidatorFactory.GetValidator(asset.MediaType, mediaConfig);
                    validator.ValidateFileSize(asset.FileSize);
                    IAssetStorageManager storage = AssetStorageFactory.GetStorageManager("Media", web.Url);
                    string newFileUniqueNameWithoutExtension = Guid.NewGuid().ToString();
                    string newFileUniqueName = String.Concat(newFileUniqueNameWithoutExtension, originFileInfo.Extension);
                            
                    //upload optional images
                    if (ThumbnailInput.PostedFile != null && !String.IsNullOrEmpty(ThumbnailInput.PostedFile.FileName))
                    {
                        string thumbFileName = MediaConfig.GetThumbnailFileName(newFileUniqueNameWithoutExtension, ThumbnailInput.PostedFile.FileName);
                        asset.Thumbnail = storage.Save(thumbFileName, ThumbnailInput.PostedFile.InputStream);
                        thumbUploaded = true;
                    }
                    if (PosterInput.PostedFile != null && !String.IsNullOrEmpty(PosterInput.PostedFile.FileName))
                    {
                        string posterFileName = MediaConfig.GetPosterFileName(newFileUniqueNameWithoutExtension, PosterInput.PostedFile.FileName);
                        asset.Poster = storage.Save(posterFileName, PosterInput.PostedFile.InputStream);
                        posterUploaded = true;
                    }
                    //upload principal file
                    if (asset.MediaType == MediaType.Image)
                    {
                        if (! thumbUploaded)//no thumb image was uploaded
                        {
                            //generate thumb & save
                            ImageProcessor imgProc = new ImageProcessor();
                            using (MemoryStream thumbStream = new MemoryStream())
                            {
                                string thumbFileName = MediaConfig.GetThumbnailFileName(newFileUniqueNameWithoutExtension);
                                imgProc.GenerateThumbnail(FileToUpload.PostedFile.InputStream, thumbStream);
                                thumbStream.Position = 0;
                                asset.Thumbnail = storage.Save(thumbFileName, thumbStream);
                            }
                            FileToUpload.PostedFile.InputStream.Position = 0;
                        }
                        //save to final location
                        asset.Location = storage.Save(newFileUniqueName, FileToUpload.PostedFile.InputStream);
                        asset.Status = ProcessingStatus.Success;
                    }
                    else
                    {
                        var tempFileInfo = new FileInfo(Path.Combine(destFolder, newFileUniqueName));

                        SPSecurity.RunWithElevatedPrivileges(delegate()
                        {
                            //save to file system
                            
                            FileToUpload.PostedFile.SaveAs(tempFileInfo.FullName);
                            asset.TempLocation = tempFileInfo.FullName;
                            asset.Duration = MediaLengthCalculator.GetMediaLength(tempFileInfo);
                        });

                        try
                        {
                            validator.ValidateLength(asset.Duration);
                        }
                        catch (MediaTooLargeException)
                        {
                            FileUtils.Delete(tempFileInfo);
                            throw;
                        }
                        if (asset.MediaType == MediaType.Audio)
                        {
                            asset.Location = storage.Save(newFileUniqueName, FileToUpload.PostedFile.InputStream);
                            FileUtils.Delete(tempFileInfo);
                            asset.TempLocation = String.Empty;
                            if (!thumbUploaded)//no thumb image was uploaded, upload default
                            {
                                string thumbnailFileName = MediaConfig.GetThumbnailFileName(newFileUniqueNameWithoutExtension, mediaConfig.DefaultAudioThumbnail);
                                SPFile thumbImg = web.GetFile(mediaConfig.DefaultAudioThumbnail);
                                asset.Thumbnail = storage.Save(thumbnailFileName, thumbImg.OpenBinaryStream());
                            }
                            if (!posterUploaded)//no poster image was uploaded, upload default
                            {
                                string posterFileName = MediaConfig.GetPosterFileName(newFileUniqueNameWithoutExtension, mediaConfig.DefaultAudioPoster);
                                SPFile thumbImg = web.GetFile(web.Url + mediaConfig.DefaultAudioPoster);
                                asset.Poster = storage.Save(posterFileName, thumbImg.OpenBinaryStream());                                
                            }
                            asset.Status = ProcessingStatus.Success;                            
                        }
                        else if (asset.MediaType == MediaType.Video && !mediaConfig.EncodeVideoFlag && thumbUploaded && posterUploaded)
                        {
                            //video doesn't need encoding, nor generation of thumb and poster images => upload file and set status as processed.
                            asset.Location = storage.Save(newFileUniqueName, FileToUpload.PostedFile.InputStream);
                            FileUtils.Delete(tempFileInfo);
                            asset.TempLocation = String.Empty;
                            asset.Status = ProcessingStatus.Success;
                        }
                        
                    }
                    string editFormUrl =  mediaRepository.Insert(web, mediaConfig.MediaAssetsListName, asset);
                    longOperation.End(editFormUrl);
                }
            }
            catch (ThreadAbortException) { /* Thrown when redirected */}
            catch (Exception ex)
            {
                logger.LogToOperations(ex, SPSMCategories.Media, Microsoft.SharePoint.Administration.EventSeverity.Error,
                    "Error uploading file to TRS-MediaAssets. FileName: '{0}'.", originFileInfo.Name);

                SPUtility.TransferToErrorPage(ex.Message);
            }
        }        
   
    }
}
