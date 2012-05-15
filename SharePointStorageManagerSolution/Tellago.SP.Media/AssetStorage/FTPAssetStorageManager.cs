using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Tellago.SP.Media.AssetStorage
{
    class FTPAssetStorageManager : IAssetStorageManager
    {
        string ftpServerUrl;
        string pullBaseAddress;
        string username;
        string password;

        public FTPAssetStorageManager(string pushServerUrl,string pullBaseAddress,string username,string password)
        {
            this.ftpServerUrl = pushServerUrl.EndsWith("/")?pushServerUrl:String.Concat(pushServerUrl,"/");
            this.pullBaseAddress = pullBaseAddress.EndsWith("/")?pullBaseAddress:String.Concat(pullBaseAddress,"/");
            this.username = username;
            this.password = password;
        }

        public void Delete(string fileUrl)
        {
            Uri fileUri = new Uri(fileUrl);
            string fileName = fileUri.Segments[fileUri.Segments.Length - 1];
            Uri ftpUri = new Uri(ftpServerUrl + fileName);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUri);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential(username, password);            
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
           // Console.WriteLine("Delete status: {0}", response.StatusDescription);
            response.Close();
        }

        public string Save(System.IO.FileInfo file)
        {
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open))
            {
                return Save(file.Name, fileStream);
            }
        }

        public string Save(string fileName, System.IO.Stream fileStream)
        {
            string fileUrl = ftpServerUrl + fileName;
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fileUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(username, password);
            using (Stream ftpStream = request.GetRequestStream())
            {
                FileUtils.CopyStream(fileStream, ftpStream);
            }
            return pullBaseAddress + fileName;
        }
    }
}
