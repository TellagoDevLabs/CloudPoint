using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using SPSM.Common.Configuration;

namespace SPSM.Common.Media
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
        DefaultAudioPoster
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
            
        public static MediaConfig FromConfigStore()
        {
            var config = new MediaConfig();
            string[] configKeys = Enum.GetNames(typeof(MediaConfigKeys));
            List<ConfigIdentifier> configIdentifiers = new List<ConfigIdentifier>(configKeys.Length);
            foreach(var key in configKeys)
            {
                configIdentifiers.Add(new ConfigIdentifier("Media",key));
            }
            Dictionary<ConfigIdentifier, string> values = ConfigStore.GetMultipleValues(configIdentifiers);

            config.MediaAssetsListName = GetConfigValue(configIdentifiers, values, MediaConfigKeys.MediaAssetsListName);
            config.TempLocationFolder = GetConfigValue(configIdentifiers, values, MediaConfigKeys.TempLocationFolder);
            config.EncoderExePath = GetConfigValue(configIdentifiers, values, MediaConfigKeys.EncoderExePath);
            config.SupportedAudioFormats = GetConfigValue(configIdentifiers, values, MediaConfigKeys.SuportedAudioFormats);
            config.SupportedImageFormats = GetConfigValue(configIdentifiers, values, MediaConfigKeys.SupportedImageFormats);
            config.SupportedVideoFormats = GetConfigValue(configIdentifiers, values, MediaConfigKeys.SupportedVideoFormats);
            config.MaxImageSizeInMegaBytes = Convert.ToInt32(values.FirstOrDefault(v => v.Key.Key == MediaConfigKeys.MaxImageSizeInMB.ToString()).Value);
            config.MaxAudioSizeInMegaBytes = Convert.ToInt32(values.FirstOrDefault(v => v.Key.Key == MediaConfigKeys.MaxAudioSizeInMB.ToString()).Value);
            config.MaxVideoSizeInMegaBytes = Convert.ToInt32(values.FirstOrDefault(v => v.Key.Key == MediaConfigKeys.MaxVideoSizeInMB.ToString()).Value);
            config.MaxAudioLengthMinutes = Convert.ToInt32(values.FirstOrDefault(v => v.Key.Key == MediaConfigKeys.MaxAudioLengthMinutes.ToString()).Value);
            config.MaxVideoLengthMinutes = Convert.ToInt32(values.FirstOrDefault(v => v.Key.Key == MediaConfigKeys.MaxVideoLengthMinutes.ToString()).Value);
            config.EncodeVideoFlag = Convert.ToBoolean(values.FirstOrDefault(v => v.Key.Key == MediaConfigKeys.EncodeVideoFlag.ToString()).Value);
            config.DefaultAudioThumbnail = GetConfigValue(configIdentifiers, values, MediaConfigKeys.DefaultAudioThumbnail);
            config.DefaultAudioPoster = GetConfigValue(configIdentifiers, values, MediaConfigKeys.DefaultAudioPoster);
            return config;
       }

        public static MediaConfig FromConfigRepository(string webFullUrl)
        {
            ConfigRepository repo = new ConfigRepository(webFullUrl);
            var values = repo.GetAllConfigFromCategory("Media");

            var config = new MediaConfig();
            config.MediaAssetsListName = values[MediaConfigKeys.MediaAssetsListName.ToString()];
            config.TempLocationFolder = values[MediaConfigKeys.TempLocationFolder.ToString()];
            config.EncoderExePath = values[ MediaConfigKeys.EncoderExePath.ToString()];
            config.SupportedAudioFormats = values[ MediaConfigKeys.SuportedAudioFormats.ToString()];
            config.SupportedImageFormats = values[MediaConfigKeys.SupportedImageFormats.ToString()];
            config.SupportedVideoFormats = values[MediaConfigKeys.SupportedVideoFormats.ToString()];
            config.MaxImageSizeInMegaBytes = Convert.ToInt32(values[ MediaConfigKeys.MaxImageSizeInMB.ToString()]);
            config.MaxAudioSizeInMegaBytes = Convert.ToInt32(values[ MediaConfigKeys.MaxAudioSizeInMB.ToString()]);
            config.MaxVideoSizeInMegaBytes = Convert.ToInt32(values[ MediaConfigKeys.MaxVideoSizeInMB.ToString()]);
            config.MaxAudioLengthMinutes = Convert.ToInt32(values[MediaConfigKeys.MaxAudioLengthMinutes.ToString()]);
            config.MaxVideoLengthMinutes = Convert.ToInt32(values[ MediaConfigKeys.MaxVideoLengthMinutes.ToString()]);
            config.EncodeVideoFlag = Convert.ToBoolean(values[MediaConfigKeys.EncodeVideoFlag.ToString()]);
            config.DefaultAudioThumbnail = values[MediaConfigKeys.DefaultAudioThumbnail.ToString()];
            config.DefaultAudioPoster = values[MediaConfigKeys.DefaultAudioPoster.ToString()];
            return config;
        }
        private static string GetConfigValue(List<ConfigIdentifier> configIdentifiers, Dictionary<ConfigIdentifier, string> configValues, MediaConfigKeys key)
        {
            ConfigIdentifier configId = configIdentifiers.First(id => id.Key == key.ToString());
            try
            {
                return configValues[configId];
            }
            catch (KeyNotFoundException ex)
            {
                // throw more meaningful exception and trace..
                string sError = string.Format("No value found in Config Store for categ: '{0}', key '{1}'",configId.Category, configId.Key);
                throw new ArgumentException(sError, "ConfigID",ex);
            }
        }

        private static int GetConfigIntValue(List<ConfigIdentifier> configIdentifiers, Dictionary<ConfigIdentifier, string> configValues, MediaConfigKeys key)
        {
            ConfigIdentifier configId = configIdentifiers.First(id => id.Key == key.ToString());
            try
            {
                string val = configValues[configId];
                if (String.IsNullOrEmpty(val))
                {
                    return 0;
                }
                try
                {
                    return Convert.ToInt32(val);
                }
                catch (Exception ex)
                {
                    string sError = string.Format("Incorrect value for Config key '{0}.{1}'. Value was: '{2}' but must be an integer.", configId.Category, configId.Key, val);
                    throw new ArgumentException(sError,ex);
                }
            }
            catch (KeyNotFoundException)
            {
                // throw more meaningful exception and trace..
                string sError = string.Format("No value found in Config Store for key '{0}.{1}'.", configId.Category, configId.Key);
                throw new ArgumentException(sError, "ConfigID");
            }
        }


        private static string[] CommaSeparatedValuesToList(string commaSeparatedValues)
        {
            if (String.IsNullOrEmpty(commaSeparatedValues))
            {
                return new string[0];
            }
            string[] vals = commaSeparatedValues.Split(",".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);
            for (int i=0;i<vals.Length;i++)
            {
                vals[i] = vals[i].Trim().ToLower();
            }
            return vals;
        }

        public static string GetThumbnailFileName(string newFileUniqueNameWithoutExtension)
        {
            return GetThumbnailFileName(newFileUniqueNameWithoutExtension, null);
        }

        public static string GetThumbnailFileName(string newFileUniqueNameWithoutExtension,string originalThumbnailFileName)
        {
            string ext  = String.IsNullOrEmpty(originalThumbnailFileName)?".jpg": Path.GetExtension(originalThumbnailFileName);
            return String.Concat(newFileUniqueNameWithoutExtension, "_thumb",ext);
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
                return String.Format(@"^.*\.({0})$",fileExtensions); 
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

        private void AppendFileFormats(StringBuilder sb,IList<string> validFormats)
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
