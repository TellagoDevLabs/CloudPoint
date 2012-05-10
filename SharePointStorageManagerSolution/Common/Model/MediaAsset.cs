using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace SPSM.Common.Media.Model
{
    public enum MediaType
	{    
        Audio,
        Image,
        Video
	}
   
    public enum ProcessingStatus
    {
        Pending,
        Success,
        Error
    }

    public class MediaAsset
    {
        public MediaType MediaType  { get; set; }
        public ProcessingStatus Status { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string TempLocation { get; set; }
        public string OriginatingWeb { get; set; }
        public string Format { get; set; }
        public string Location { get; set; }
        public string Thumbnail { get; set; }
        public string Poster { get; set; }
        public int FileSize { get; set; }
        public TimeSpan Duration { get; set; }

        public static MediaAsset FromFile(FileInfo fileInfo, string originatingWeb, MediaConfig config)
        {
            MediaAsset item = new MediaAsset();
            item.MediaType = GetMediaTypeFromFileExtension(fileInfo,config);
            item.Title = Path.GetFileNameWithoutExtension(fileInfo.FullName);
            item.OriginatingWeb = originatingWeb;
            item.Format = GetMediaFormat(fileInfo);
            return item;
        }

        public static string GetMediaFormat(FileInfo mediaFile)
        {
            return mediaFile.Extension.TrimStart(".".ToCharArray()).ToLower();
        }

        public static MediaType GetMediaTypeFromFileExtension(FileInfo fileInfo, MediaConfig config)
        {
            string ext = fileInfo.Extension.TrimStart(".".ToCharArray()).ToLower();
            if (config.SupportedImageFilesArray.Contains(ext,StringComparer.InvariantCultureIgnoreCase))
            {
                return MediaType.Image;
            }
            if (config.SupportedVideoFilesArray.Contains(ext, StringComparer.InvariantCultureIgnoreCase))
            {
                return MediaType.Video;
            }
            if (config.SupportedAudioFilesArray.Contains(ext, StringComparer.InvariantCultureIgnoreCase))
            {
                return MediaType.Audio;
            }
            throw new ArgumentException(String.Format("The file extension '{0}' is not supported", ext), "fileInfo");
        }

    }
}
