using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPSM.Common.Media.Validators
{
    class AudioValidator : SPSM.Common.Media.Validators.IMediaValidator
    {
        private MediaConfig config;
        public AudioValidator(MediaConfig config)
        {
            this.config = config;
        }
        
        #region IMediaValidator Members

        public void ValidateLength(TimeSpan length)
        {
            if (length.TotalMinutes > config.MaxAudioLengthMinutes)
            {
                throw new MediaTooLargeException(String.Format("Audio length of '{0}' exceeds the maximum length of '{1}' minutes.", length, config.MaxAudioLengthMinutes));
            }
        }

        public void ValidateFileSize(int fileSizeInBytes)
        {
            if (config.MaxAudioSizeInMegaBytes != 0 && fileSizeInBytes > config.MaxAudioSizeInBytes)
            {
                throw new MediaTooLargeException(String.Format("Audio file size of '{0}' exceeds maximum size of '{1}' bytes", fileSizeInBytes, config.MaxAudioSizeInBytes));
            }
        }

        #endregion
    }
}
