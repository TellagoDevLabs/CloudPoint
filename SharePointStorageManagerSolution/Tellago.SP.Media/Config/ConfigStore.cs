using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tellago.SP.Media.Config
{
    class ConfigStore : IConfigStore
    {                
        public string GetValue(string category, string key)
        {
            if ("Media".Equals(category,StringComparison.InvariantCultureIgnoreCase))
            {
                switch (key)
                {
                    case ConfigKeys.StorageMethodConfigKey: return "SPLibrary";
                    case ConfigKeys.LibraryNameConfigKey: return "MediaAssetsLib";
                    case ConfigKeys.PushFolderPathConfigKey: return @"\\server1\media";
                    case ConfigKeys.PushBaseAddresConfigKey: return "http://host/virtualDir/";
                    case ConfigKeys.FTPServerUrlConfigKey: return "myftpserver";
                    case ConfigKeys.FTPServerUsernameConfigKey: return "ftpuser";
                    case ConfigKeys.FTPServerPasswordConfigKey: return "ftppassword";
                    case ConfigKeys.FTPServerPullAddressConfigKey: return "http://ftpserver/media";
                    case ConfigKeys.AmazonBucketNameConfigKey: return "myProjectBucket";
                    case ConfigKeys.AmazonKeyNameConfigKey: return "media";
                    case ConfigKeys.AmazonAccessKeyIDConfigKey: return "someAmazonKeyID";
                    case ConfigKeys.AmazonAccessSecretKeyIDConfigKey: return "someAmazonSecretKeyID";
                }
            }
            return null;
        }
    }

    public class ConfigKeys
    {
        public const string StorageMethodConfigKey = "AssetStorageMethod";
        public const string LibraryNameConfigKey = "AssetStorageLibraryName";
        public const string PushFolderPathConfigKey = "AssetStoragePushFolderPath";
        public const string PushBaseAddresConfigKey = "AssetStoragePullBaseAddress";
        public const string FTPServerUrlConfigKey = "AssetStorageFTPServerUrl";
        public const string FTPServerUsernameConfigKey = "AssetStorageFTPServerUsername";
        public const string FTPServerPasswordConfigKey = "AssetStorageFTPServerPassword";
        public const string FTPServerPullAddressConfigKey = "AssetStorageFTPServerPullAddress";
        public const string AmazonBucketNameConfigKey = "AssetStorageAmazonBucketName";
        public const string AmazonKeyNameConfigKey = "AssetStorageAmazonKeyName";
        public const string AmazonAccessKeyIDConfigKey = "AssetStorageAmazonAccessKeyID";
        public const string AmazonAccessSecretKeyIDConfigKey = "AssetStorageAmazonAccessSecretKeyID";
        public const string MediaAssetsListName = "MediaAssetsListName";
        /*
        config.MediaAssetsListName = "MediaAssets";
            config.TempLocationFolder = @"c:\TempMedia\";
            config.EncoderExePath = @"c:\Tellago\SPMedia\Tellago.SP.Media.Encoder\bin\Debug\Tellago.SP.Media.Encoder.exe";
            config.SupportedAudioFormats = "mp3";
            config.SupportedImageFormats = "jpeg, jpg, png, gif, bmp";
            config.SupportedVideoFormats = "avi, wmv, mov, mp4";
            config.MaxImageSizeInMegaBytes = 5;
            config.MaxAudioSizeInMegaBytes = 15;
            config.MaxVideoSizeInMegaBytes = 15;
            config.MaxAudioLengthMinutes = 60;
            config.MaxVideoLengthMinutes = 30;
            config.EncodeVideoFlag = false ;
            config.DefaultAudioThumbnail = "/Style%20Library/Tellago.SP.Media/images/audioDefaultThumb.png";
            config.DefaultAudioPoster = "/Style%20Library/Tellago.SP.Media/images/audioDefaultPoster.png";
         * */
    }
}
