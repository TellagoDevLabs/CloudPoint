using System;
namespace SPSM.Common.AssetStorage
{
    public interface IAssetStorageManager
    {
        void Delete(string fileUrl);
        string Save(System.IO.FileInfo file);
        string Save(string fileName, System.IO.Stream fileStream);
    }
}
