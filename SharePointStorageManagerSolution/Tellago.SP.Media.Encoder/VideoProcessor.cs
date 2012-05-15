using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Expression.Encoder;
using System.IO;
using Microsoft.Expression.Encoder.Profiles;
using System.Drawing.Imaging;
using System.Configuration;

namespace Tellago.SP.Media.Encoder
{
    public class VideoProcessor
    {
        public static void EncodeVideo(FileInfo inputFile, FileInfo outputFile)
        {
            DirectoryInfo outDir = outputFile.Directory;
            if (!outDir.Exists)
            {
                outDir.Create();
            }
            MediaItem mediaItem = new MediaItem(inputFile.FullName);
            int bitrate = GetBitrate(mediaItem);
                    
            using (Job job = new Job())
            {
                job.OutputDirectory = outDir.FullName;
                job.CreateSubfolder = false;
                job.MediaItems.Add(mediaItem);
                
                SetProfile(mediaItem);
                
                // We can also use some of the presets. In that case comment the SetProfile line above for something like the one below:
                
                //    //H264VimeoSD preset settings: Output Format: MP4. Container: MP4. Video Codec: H.264 - Main. 
                //    //Video size: 640, 480. Video Bitrate: 2500 Kbps. Video Encoding: CBR SinglePass. 
                //    //Audio Codec: AAC. Audio Channels: Stereo. Audio Bitrate: 128 Kbps. Audio Encoding: CBR Single Pass
                //    job.ApplyPreset(Presets.H264VimeoSD);
                
                job.Encode();
            }
            if (!outputFile.FullName.Equals(mediaItem.ActualOutputFileFullPath, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception(String.Format("The output file specified: '{0}' does not match the actual output file '{1}'", outputFile.FullName, mediaItem.ActualOutputFileFullPath));
            }
        }

        private static void SetProfile(MediaItem mediaItem)
        {
            int videoBitrate = Convert.ToInt32(ConfigurationManager.AppSettings["videoBitrate"]);
            int audioBitrate = Convert.ToInt32(ConfigurationManager.AppSettings["audioBitrate"]);
            int width = Convert.ToInt32(ConfigurationManager.AppSettings["videoWidth"]);
            int height = Convert.ToInt32(ConfigurationManager.AppSettings["videoHeight"]);
           
            int originalVideoBitrate = GetBitrate(mediaItem) - audioBitrate;
            if (originalVideoBitrate > 0 && originalVideoBitrate < videoBitrate)
            {
                videoBitrate = originalVideoBitrate;
            }
            
            var size = mediaItem.MainMediaFile.VideoStreams[0].VideoSize;
            if (size.Width > width || size.Height > height)
            {
                size = new System.Drawing.Size(width, height);
            }
            Console.WriteLine("Size: " + size.Width + "x" + size.Height);
            mediaItem.OutputFormat = new MP4OutputFormat();
            mediaItem.OutputFormat.VideoProfile = new MainH264VideoProfile()
            {                
                Size = size,
                Bitrate = new ConstantBitrate(videoBitrate,false)                            
            };
            Console.WriteLine("Video Bitrate: " + videoBitrate);
            
            mediaItem.OutputFormat.AudioProfile = new AacAudioProfile() 
            {
                Bitrate = new ConstantBitrate(audioBitrate)
            };
            Console.WriteLine("Audio Bitrate: " + mediaItem.OutputFormat.AudioProfile.Bitrate.BitrateValue);
            mediaItem.OverlayTransparentBackground = true;
        }

        private static int GetBitrate(MediaItem mediaItem)
        {
            long fileSize = mediaItem.MainMediaFile.FileSize;
            int bitrate = (int)Math.Round((double)fileSize * 8 / mediaItem.FileDuration.TotalSeconds / 1024);//bitrate = "filesize * 8 (bits/byte) / duration (seconds)"
            return bitrate;
        }
        public static void GenerateThumbnail(FileInfo mediaFile, string thumbnailFilePath, int width, int height)
        {
            int thumbWidth = width;
            int thumbHeight = height;
            var video = new MediaItem(mediaFile.FullName);
            var videoSize = video.MainMediaFile.VideoStreams[0].VideoSize;

            if (videoSize.Width > videoSize.Height)
            {
                thumbHeight = Decimal.ToInt32(((Decimal)videoSize.Height / videoSize.Width) * thumbWidth);
                if (thumbHeight > height)
                {
                    thumbHeight = height;
                }
            }
            else
            {
                thumbWidth = Decimal.ToInt32(((Decimal)videoSize.Width / videoSize.Height) * thumbHeight);
                if (thumbWidth > width)
                {
                    thumbWidth = width;
                }
            }


            using (var bitmap = video.MainMediaFile.GetThumbnail(
                new TimeSpan(0, 0, 5),
                new System.Drawing.Size(thumbWidth, thumbHeight)))
            {
                bitmap.Save(thumbnailFilePath,ImageFormat.Jpeg);
            }
        }


        internal static void GenerateMetadata(FileInfo inputFile, string metadataFilePath)
        {
            var media = new MediaItem(inputFile.FullName);
            TimeSpan duration = media.FileDuration;
            File.WriteAllText(metadataFilePath, duration.ToString());
        }
    }
}
