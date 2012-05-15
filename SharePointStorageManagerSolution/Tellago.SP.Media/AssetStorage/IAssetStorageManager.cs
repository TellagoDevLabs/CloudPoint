using System;
namespace Tellago.SP.Media.AssetStorage
{
    public interface IAssetStorageManager
    {
        void Delete(string fileUrl);
        string Save(System.IO.FileInfo file);
        string Save(string fileName, System.IO.Stream fileStream);
    }
}
