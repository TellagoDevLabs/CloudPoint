using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SPSM.Common.Logging;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;

namespace SPSM.Common.AssetStorage
{
    public static class FileUtils
    {
        public static void Delete(string filePath)
        {
            if (!String.IsNullOrEmpty(filePath))
            {
                FileInfo fi = new FileInfo(filePath);
                Delete(fi);
            }
        }

        public static void Delete(FileInfo fileInfo)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                fileInfo.Refresh();
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
            });
        }

        public static void TryDelete(FileInfo fi)
        {
            if (fi != null)
            {
                try
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        fi.Refresh();
                        if (fi.Exists)
                        {
                            fi.Delete();
                        }
                    });
                }
                catch (Exception ex)
                {
                    new Logger().LogToOperations(ex, SPSMCategories.Media, EventSeverity.Warning, "Could not delete file '{0}'", fi.FullName);
                }
            }
        }


        /// <summary>
        /// Copies the contents of input to output. Doesn't close either stream.
        /// </summary>
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

       
    }
}
