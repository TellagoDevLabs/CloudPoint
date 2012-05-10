using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPSM.Common.Media.Validators
{
    class VideoValidator : IMediaValidator
    {
        private MediaConfig config;
        public VideoValidator(MediaConfig config)
        {
            this.config = config;
        }
        public void ValidateLength(TimeSpan length)
        {
            if (length.TotalMinutes > config.MaxVideoLengthMinutes)
            {
                throw new MediaTooLargeException(String.Format("Video length of '{0}' exceeds the maximum length of '{1}' minutes.",length,config.MaxVideoLengthMinutes));
            }
        }

        public void ValidateFileSize(int fileSizeInBytes)
        {
            if (config.MaxVideoSizeInMegaBytes != 0 && fileSizeInBytes > config.MaxVideoSizeInBytes)
            {
                throw new MediaTooLargeException(String.Format("Video file size of '{0}' exceeds maximum size of '{1}' bytes", fileSizeInBytes, config.MaxVideoSizeInBytes));
            }
        }
    }
}
