using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Tellago.SP.Common.Configuration;

namespace Tellago.SP.Media.Model
{
    public enum MediaConfigKeys
    {
        MediaAssetsListName,
        TempLocationFolder,
        EncoderExePath,
        SupportedImageFormats,
        SupportedVideoFormats,
        SuportedAudioFormats,
        MaxImageSizeInMB,
        MaxAudioSizeInMB,
        MaxVideoSizeInMB,
        MaxAudioLengthMinutes,
        MaxVideoLengthMinutes,
        EncodeVideoFlag,
        DefaultAudioThumbnail,
        DefaultAudioPoster,
        DefaultVideoThumbnail,
        DefaultVideoPoster,
        DefaultImageThumbnail,
        AssetStorageMethod,
        AssetStorageLibraryName,
        AssetStoragePushFolderPath,
        AssetStoragePullBaseAddress,
        AssetStorageFTPServerUrl,
        AssetStorageFTPServerUsername,
        AssetStorageFTPServerPassword,
        AssetStorageFTPServerPullAddress,
        AssetStorageAmazonBucketName,
        AssetStorageAmazonKeyName,
        AssetStorageAmazonAccessKeyID,
        AssetStorageAmazonAccessSecretKeyID,
        AssetStorageAzureAccountName,
        AssetStorageAzureAccountKey,
        AssetStorageAzureBlobEndpoint,
        AssetStorageAzureContainer
    }

    public class MediaConfig
    {
        public string MediaAssetsListName { get; set; }
        public string TempLocationFolder { get; set; }
        public string EncoderExePath { get; set; }
        public string SupportedImageFormats { get; set; }
        public string SupportedVideoFormats { get; set; }
        public string SupportedAudioFormats { get; set; }
        public int MaxImageSizeInMegaBytes { get; set; }
        public int MaxAudioSizeInMegaBytes { get; set; }
        public int MaxVideoSizeInMegaBytes { get; set; }
        public int MaxAudioLengthMinutes { get; set; }
        public int MaxVideoLengthMinutes { get; set; }
        public bool EncodeVideoFlag { get; set; }
        public string DefaultAudioThumbnail { get; set; }
        public string DefaultAudioPoster { get; set; }

        public static MediaConfig FromConfigStore(string webUrl)
        {
            ConfigRepository configRepo = new ConfigRepository(webUrl);
            var mediaConfigs = configRepo.GetAllConfigFromCategory("Media");
            
            var config = new MediaConfig();

            config.MediaAssetsListName = mediaConfigs[MediaConfigKeys.MediaAssetsListName.ToString()];
            config.TempLocationFolder = mediaConfigs[MediaConfigKeys.TempLocationFolder.ToString()];
            config.EncoderExePath = mediaConfigs[MediaConfigKeys.EncoderExePath.ToString()];
            config.SupportedAudioFormats = mediaConfigs[MediaConfigKeys.SuportedAudioFormats.ToString()];
            config.SupportedImageFormats = mediaConfigs[MediaConfigKeys.SupportedImageFormats.ToString()];
            config.SupportedVideoFormats = mediaConfigs[MediaConfigKeys.SupportedVideoFormats.ToString()];
            config.MaxImageSizeInMegaBytes = Convert.ToInt32(mediaConfigs[MediaConfigKeys.MaxImageSizeInMB.ToString()]);
            config.MaxAudioSizeInMegaBytes = Convert.ToInt32(mediaConfigs[MediaConfigKeys.MaxAudioSizeInMB.ToString()]);
            config.MaxVideoSizeInMegaBytes = Convert.ToInt32(mediaConfigs[MediaConfigKeys.MaxVideoSizeInMB.ToString()]);
            config.MaxAudioLengthMinutes = Convert.ToInt32(mediaConfigs[ MediaConfigKeys.MaxAudioLengthMinutes.ToString()]);
            config.MaxVideoLengthMinutes = Convert.ToInt32(mediaConfigs[ MediaConfigKeys.MaxVideoLengthMinutes.ToString()]);
            config.EncodeVideoFlag = Convert.ToBoolean(mediaConfigs[MediaConfigKeys.EncodeVideoFlag.ToString()]);
            config.DefaultAudioThumbnail = mediaConfigs[MediaConfigKeys.DefaultAudioThumbnail.ToString()];
            config.DefaultAudioPoster = mediaConfigs[MediaConfigKeys.DefaultAudioPoster.ToString()];
            return config;
        }

        private static string[] CommaSeparatedValuesToList(string commaSeparatedValues)
        {
            if (String.IsNullOrEmpty(commaSeparatedValues))
            {
                return new string[0];
            }
            string[] vals = commaSeparatedValues.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < vals.Length; i++)
            {
                vals[i] = vals[i].Trim().ToLower();
            }
            return vals;
        }

        public static string GetThumbnailFileName(string newFileUniqueNameWithoutExtension)
        {
            return GetThumbnailFileName(newFileUniqueNameWithoutExtension, null);
        }

        public static string GetThumbnailFileName(string newFileUniqueNameWithoutExtension, string originalThumbnailFileName)
        {
            string ext = String.IsNullOrEmpty(originalThumbnailFileName) ? ".jpg" : Path.GetExtension(originalThumbnailFileName);
            return String.Concat(newFileUniqueNameWithoutExtension, "_thumb", ext);
        }

        public static string GetPosterFileName(string newFileUniqueNameWithoutExtension)
        {
            return GetPosterFileName(newFileUniqueNameWithoutExtension, null);
        }

        public static string GetPosterFileName(string newFileUniqueNameWithoutExtension, string originalThumbnailFileName)
        {
            string ext = String.IsNullOrEmpty(originalThumbnailFileName) ? ".jpg" : Path.GetExtension(originalThumbnailFileName);
            return String.Concat(newFileUniqueNameWithoutExtension, "_poster", ext);
        }


        public string[] SupportedImageFilesArray { get { return CommaSeparatedValuesToList(SupportedImageFormats); } }
        public string[] SupportedVideoFilesArray { get { return CommaSeparatedValuesToList(SupportedVideoFormats); } }
        public string[] SupportedAudioFilesArray { get { return CommaSeparatedValuesToList(SupportedAudioFormats); } }

        public int MaxImageSizeInBytes { get { return MaxImageSizeInMegaBytes * 1024 * 1024; } }
        public int MaxAudioSizeInBytes { get { return MaxAudioSizeInMegaBytes * 1024 * 1024; } }
        public int MaxVideoSizeInBytes { get { return MaxVideoSizeInMegaBytes * 1024 * 1024; } }

        /// <summary>
        /// Returns the Regex Validation Expression for the ASP.NET FileExtensionValidator, taken the configured supported formats
        /// For example: "^.*\.(jpeg|jpg|png|gif|bmp|avi|wmv|mov|flv|mp4|mp3|JPEG|JPG|PNG|GIF|BMP|AVI|WMV|MOV|FLV|MP4|MP3)$"
        /// </summary>
        public string FileExtensionValidationExpression
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                AppendFileFormats(sb, SupportedImageFilesArray);
                AppendFileFormats(sb, SupportedVideoFilesArray);
                AppendFileFormats(sb, SupportedAudioFilesArray);
                string fileExtensions = sb.ToString().TrimEnd("|".ToCharArray());
                return String.Format(@"^.*\.({0})$", fileExtensions);
            }
        }

        public string ImageFileExtensionValidationExpression
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                AppendFileFormats(sb, SupportedImageFilesArray);
                string fileExtensions = sb.ToString().TrimEnd("|".ToCharArray());
                return String.Format(@"^.*\.({0})$", fileExtensions);
            }
        }

        private void AppendFileFormats(StringBuilder sb, IList<string> validFormats)
        {
            if (validFormats != null)
            {
                foreach (var format in validFormats)
                {
                    //have to put both lower and upper case since asp.net regex validator doesn't support case insensitive matching
                    sb.Append(String.Format("{0}|{1}|", format.ToLower(), format.ToUpper()));
                }
            }
        }
    }
}
