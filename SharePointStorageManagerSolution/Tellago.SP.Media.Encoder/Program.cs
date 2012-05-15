using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace Tellago.SP.Media.Encoder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3 && args.Length != 4)
            {
                var usage = new StringBuilder();
                usage.AppendLine("Incorrect params. Usage:");
                usage.AppendLine("  TRS.Common.Media.Encoder.exe <inputFilePath> <thumbnailFilePath> <posterFilePath> [<outputFilePath>]");
                usage.AppendLine(String.Format("Recived '{0}' args instead.",args.Length));
                throw new Exception(usage.ToString());
            }
            string inputFilePath = args[0];
            string thumbnailFilePath = args[1];
            string posterFilePath = args[2];
            FileInfo inputFile = new FileInfo(inputFilePath);
            FileInfo mediaFile = inputFile;
            if (args.Length == 4)
            {
                string outputFilePath = args[3];
                FileInfo outputFile = new FileInfo(outputFilePath);
                VideoProcessor.EncodeVideo(inputFile, outputFile);
               // mediaFile = outputFile;
            }
            int thumbWidth = Convert.ToInt32(ConfigurationManager.AppSettings["thumbWidth"]);
            int thumbHeight = Convert.ToInt32(ConfigurationManager.AppSettings["thumbHeight"]);
            int posterWidth = Convert.ToInt32(ConfigurationManager.AppSettings["posterWidth"]);
            int posterHeight = Convert.ToInt32(ConfigurationManager.AppSettings["posterHeight"]);
           
            VideoProcessor.GenerateThumbnail(mediaFile, thumbnailFilePath, thumbWidth, thumbHeight);
            VideoProcessor.GenerateThumbnail(mediaFile, posterFilePath, posterWidth, posterHeight);
            
        }

        
        
    }
}
