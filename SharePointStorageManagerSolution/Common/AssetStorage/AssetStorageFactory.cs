using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SPSM.Common.Configuration;

namespace SPSM.Common.AssetStorage
{
    public class AssetStorageFactory
    {
        private const string StorageMethodConfigKey = "AssetStorageMethod";
        private const string LibraryNameConfigKey = "AssetStorageLibraryName";
        private const string PushFolderPathConfigKey = "AssetStoragePushFolderPath";
        private const string PushBaseAddresConfigKey = "AssetStoragePullBaseAddress";
        private const string FTPServerUrlConfigKey = "AssetStorageFTPServerUrl";
        private const string FTPServerUsernameConfigKey = "AssetStorageFTPServerUsername";
        private const string FTPServerPasswordConfigKey = "AssetStorageFTPServerPassword";
        private const string FTPServerPullAddressConfigKey = "AssetStorageFTPServerPullAddress";
        private const string AmazonBucketNameConfigKey = "AssetStorageAmazonBucketName";
        private const string AmazonKeyNameConfigKey = "AssetStorageAmazonKeyName";
        private const string AmazonAccessKeyIDConfigKey = "AssetStorageAmazonAccessKeyID";
        private const string AmazonAccessSecretKeyIDConfigKey = "AssetStorageAmazonAccessSecretKeyID";
        private const string AmazonMakePublicKey = "AssetStorageAmazonMakePublic";
        private const string AmazonSignExpireInMinutesKey = "AssetStorageAmazonSignExpireInMinutes";

        static public IAssetStorageManager GetStorageManager(string configCategory, string webUrl)
        {
            var configHelper = new ConfigHelper(webUrl);
            string storageMethod = configHelper.GetValue(configCategory, StorageMethodConfigKey);
            if ("SPLibrary".Equals(storageMethod, StringComparison.InvariantCultureIgnoreCase))
            {
                string mediaLibraryName = configHelper.GetValue(configCategory, LibraryNameConfigKey);
                return new SPLibraryAssetStorageManager(webUrl, mediaLibraryName);
            }
            else if ("FileSystem".Equals(storageMethod, StringComparison.InvariantCultureIgnoreCase))
            {
                string storageFolderPath = configHelper.GetValue(configCategory, PushFolderPathConfigKey);
                string storageBaseAddress = configHelper.GetValue(configCategory, PushBaseAddresConfigKey);
                return new FileSystemAssetStorageManager(storageFolderPath, storageBaseAddress);
            }
            else if ("FTP".Equals(storageMethod, StringComparison.InvariantCultureIgnoreCase))
            {
                string ftpServerUrl = configHelper.GetValue(configCategory, FTPServerUrlConfigKey);
                string ftpServerUsername = configHelper.GetValue(configCategory, FTPServerUsernameConfigKey);
                string ftpServerPassword = configHelper.GetValue(configCategory, FTPServerPasswordConfigKey);
                string ftpServerPullAdress = configHelper.GetValue(configCategory, FTPServerPullAddressConfigKey);
                return new FTPAssetStorageManager(ftpServerUrl, ftpServerPullAdress, ftpServerUsername, ftpServerPassword);
            }
            else if ("AmazonS3".Equals(storageMethod, StringComparison.InvariantCultureIgnoreCase))
            {
                string amazonBucketName = configHelper.GetValue(configCategory, AmazonBucketNameConfigKey);
                string amazonKeyName = configHelper.GetValue(configCategory, AmazonKeyNameConfigKey);
                string amazonAccessKeyID = configHelper.GetValue(configCategory, AmazonAccessKeyIDConfigKey);
                string amazonAccessSecretKeyID = configHelper.GetValue(configCategory, AmazonAccessSecretKeyIDConfigKey);
                string amazonMakePublic = configHelper.GetValue(configCategory, AmazonMakePublicKey);
                string amazonSignExpireInMinutes = configHelper.GetValue(configCategory, AmazonSignExpireInMinutesKey);
               
                return new AmazonS3AssetStorageManager(amazonBucketName, amazonKeyName, amazonAccessKeyID,
                                                       amazonAccessSecretKeyID, amazonMakePublic, amazonSignExpireInMinutes, webUrl );
            }
            throw new ArgumentException(String.Format("Incorrect configuration Value '{0}' in ConfigStore for category '{1}' and key '{2}'. Supported options are: '{3}'",
               storageMethod, configCategory, StorageMethodConfigKey, "AmazonS3|FileSystem|FTP|SPLibrary"));
        }

    }

    /// <summary>
    /// Hack because ConfigStore doesn't work on timer jobs because it relies on app.config settings. Must fix that!!!
    /// </summary>
    class ConfigHelper
    {
        private ConfigRepository configrepo;

        public ConfigHelper(string webUrl)
        {
            configrepo = new ConfigRepository(webUrl);
        }

        public string GetValue(string configCategory, string configKey)
        {
            try
            {
                return ConfigStore.GetValue(configCategory, configKey);
            }
            catch (TypeInitializationException ex)
            {
                return configrepo.GetConfigValue(configCategory, configKey);
            }
        }
    }
}
