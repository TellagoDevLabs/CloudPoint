using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.SharePoint.Administration;
using Tellago.SP.Media.Model;
using Tellago.SP.Logging;

namespace Tellago.SP.Media.Backend
{
    public class VideoProcessor
    {
        private const string ProcessedDirectoryName = "Processed";
        private int MaxWaitingProcessMillisecs = new TimeSpan(0, 15, 0).Milliseconds; 
        private MediaConfig config;
        private Logger logger = new Logger();

        public VideoProcessor(MediaConfig config)
        {
            this.config = config;
        }


        public void EncodeVideo(FileInfo inputFile, out FileInfo encodedVideo, out FileInfo thumbnail, out FileInfo poster)
        {
            DirectoryInfo processedSubdir = GetProcessedDir(inputFile);
            string encodedFilePath = null;
            if (config.EncodeVideoFlag)
            {
                string encodedFileName = String.Concat(Path.GetFileNameWithoutExtension(inputFile.Name), ".mp4");
                encodedFilePath = Path.Combine(processedSubdir.FullName, encodedFileName);
            }
            else
            {
                //if no encoding is needed we leave the file as is, but call the encoder to generate thumbnail & poster images 
                encodedFilePath = inputFile.FullName;
            }
            string thumbFileName = MediaConfig.GetThumbnailFileName(Path.GetFileNameWithoutExtension(inputFile.Name));
            string posterFileName = MediaConfig.GetPosterFileName(Path.GetFileNameWithoutExtension(inputFile.Name));
            
            string thumbnailFilePath = Path.Combine(processedSubdir.FullName, thumbFileName);
            string posterFilePath = Path.Combine(processedSubdir.FullName, posterFileName);

            string args = config.EncodeVideoFlag?
                String.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\"", inputFile.FullName, thumbnailFilePath, posterFilePath, encodedFilePath) :
                String.Format("\"{0}\" \"{1}\" \"{2}\"", inputFile.FullName, thumbnailFilePath, posterFilePath);

            ExecuteMediaProcess(args);

            encodedVideo = new FileInfo(encodedFilePath);
            thumbnail = new FileInfo(thumbnailFilePath);
            poster = new FileInfo(posterFilePath);

        }
        
        private void ExecuteMediaProcess(string args)
        {
            logger.LogToOperations(LogCategories.Media, EventSeverity.Verbose, "Starting process '{0}' with args '{1}'", config.EncoderExePath, args);
            
            ProcessStartInfo startInfo = new ProcessStartInfo(config.EncoderExePath);
            startInfo.Arguments = args;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;

            Process process = new Process();
            process.StartInfo = startInfo;
            DateTime start = DateTime.Now;
            process.Start();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit(MaxWaitingProcessMillisecs);

            if (process.ExitCode != 0)
            {
                throw new MediaProcessingException(String.Format("Video encoder process returned with exit code '{0}'. Error was: '{1}'", process.ExitCode, error));
            }
            TimeSpan encodingTime = DateTime.Now - start;
            logger.LogToOperations(LogCategories.Media, EventSeverity.Verbose, "Process '{0}' with args '{1}' completed on '{2}'", config.EncoderExePath, args, encodingTime);

        }

        private static DirectoryInfo GetProcessedDir(FileInfo inputFile)
        {
            DirectoryInfo processedSubdir;
            var subdirs = inputFile.Directory.GetDirectories(ProcessedDirectoryName, SearchOption.TopDirectoryOnly);
            if (subdirs.Length == 0)
            {
                processedSubdir = inputFile.Directory.CreateSubdirectory(ProcessedDirectoryName);
            }
            else
            {
                processedSubdir = subdirs[0];
            }
            return processedSubdir;
        }

 
        
    }
}
