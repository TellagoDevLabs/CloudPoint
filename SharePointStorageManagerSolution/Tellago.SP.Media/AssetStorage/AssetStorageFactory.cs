using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tellago.SP.Media.Model;
using Tellago.SP.Common.Configuration;
//using Tellago.SP.Media.Config;

namespace Tellago.SP.Media.AssetStorage
{
    public class AssetStorageFactory
    {
        //private static IConfigStore configStore = new ConfigStore();

        static public IAssetStorageManager GetStorageManager(string configCategory,string webUrl)
        {
            ConfigRepository configStore = new ConfigRepository(webUrl);
            var mediaConfig = configStore.GetAllConfigFromCategory(configCategory);
            string storageMethod = mediaConfig[MediaConfigKeys.AssetStorageMethod.ToString()];
            if ("SPLibrary".Equals(storageMethod, StringComparison.InvariantCultureIgnoreCase))
            {
                string mediaLibraryName = mediaConfig[MediaConfigKeys.AssetStorageLibraryName.ToString()];
                return new SPLibraryAssetStorageManager(webUrl, mediaLibraryName);
            }
            else if ("FileSystem".Equals(storageMethod, StringComparison.InvariantCultureIgnoreCase))
            {
                string storageFolderPath = mediaConfig[MediaConfigKeys.AssetStoragePushFolderPath.ToString()];
                string storageBaseAddress = mediaConfig[MediaConfigKeys.AssetStoragePullBaseAddress.ToString()];
                return new FileSystemAssetStorageManager(storageFolderPath,storageBaseAddress);
            }   
            else if ("FTP".Equals(storageMethod, StringComparison.InvariantCultureIgnoreCase))
            {
                string ftpServerUrl = mediaConfig[MediaConfigKeys.AssetStorageFTPServerUrl.ToString()];
                string ftpServerUsername = mediaConfig[MediaConfigKeys.AssetStorageFTPServerUsername.ToString()];
                string ftpServerPassword = mediaConfig[MediaConfigKeys.AssetStorageFTPServerPassword.ToString()];
                string ftpServerPullAdress = mediaConfig[MediaConfigKeys.AssetStorageFTPServerPullAddress.ToString()];
                return new FTPAssetStorageManager(ftpServerUrl,ftpServerPullAdress,ftpServerUsername,ftpServerPassword);
            }
            else if ("AmazonS3".Equals(storageMethod, StringComparison.InvariantCultureIgnoreCase))
            {
                string amazonBucketName = mediaConfig[MediaConfigKeys.AssetStorageAmazonBucketName.ToString()];
                string amazonKeyName = mediaConfig[MediaConfigKeys.AssetStorageAmazonKeyName.ToString()];
                string amazonAccessKeyID = mediaConfig[MediaConfigKeys.AssetStorageAmazonAccessKeyID.ToString()];
                string amazonAccessSecretKeyID = mediaConfig[MediaConfigKeys.AssetStorageAmazonAccessSecretKeyID.ToString()];

                return new AmazonS3AssetStorageManager(amazonBucketName, amazonKeyName, amazonAccessKeyID, amazonAccessSecretKeyID);
            }
            else if ("AzureBlob".Equals(storageMethod, StringComparison.InvariantCultureIgnoreCase))
            {
                string azureAccountName = mediaConfig[MediaConfigKeys.AssetStorageAzureAccountName.ToString()];
                string azureAccountKey = mediaConfig[MediaConfigKeys.AssetStorageAzureAccountKey.ToString()];
                string azureBlobEnpoint = mediaConfig[MediaConfigKeys.AssetStorageAzureBlobEndpoint.ToString()];
                string azureContainer = mediaConfig[MediaConfigKeys.AssetStorageAzureContainer.ToString()];

                return new AzureStorageManager(azureAccountName, azureAccountKey, azureBlobEnpoint, azureContainer);
            }
            throw new ArgumentException(String.Format("Incorrect configuration Value '{0}' in ConfigStore for category '{1}' and key '{2}'. Supported options are: '{3}'",
               storageMethod, configCategory, MediaConfigKeys.AssetStorageMethod.ToString(), "AmazonS3|FileSystem|FTP|SPLibrary"));
        }
       
    }

}
