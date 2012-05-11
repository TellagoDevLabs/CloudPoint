using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using SPSM.Common.Logging;
using Microsoft.SharePoint.Administration;
using Microsoft.WindowsAPICodePack.Shell;

namespace SPSM.Common.Media
{
    public class MediaLengthCalculator
    {
        public static TimeSpan GetMediaLength(FileInfo inputFile)
        {
            ShellFile so = ShellFile.FromFilePath(inputFile.FullName);
            double nanoseconds;
            double.TryParse(so.Properties.System.Media.Duration.Value.ToString(),
            out nanoseconds);
            double milliseconds = 0;
            if (nanoseconds > 0)
            {
                milliseconds = Convert100NanosecondsToMilliseconds(nanoseconds);
            }
            return TimeSpan.FromMilliseconds(milliseconds);
        }

        public static double Convert100NanosecondsToMilliseconds(double nanoseconds)
        {
            // One million nanoseconds in 1 millisecond, 
            // but we are passing in 100ns units...
            return nanoseconds * 0.0001;
        }
        
    }
}
