using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPSM.Common.Media.Validators
{
    class ImageValidator : IMediaValidator
    {
        private MediaConfig config;
        
        public ImageValidator(MediaConfig config)
        {
            this.config = config;
        }

        #region IMediaValidator Members

        public void ValidateLength(TimeSpan length)
        {

        }

        public void ValidateFileSize(int fileSizeInBytes)
        {
            if (config.MaxImageSizeInMegaBytes != 0 && fileSizeInBytes > config.MaxImageSizeInBytes)
            {
                throw new MediaTooLargeException(String.Format("Image file size of '{0}' exceeds maximum size of '{1}' bytes", fileSizeInBytes, config.MaxImageSizeInBytes));
            }
        }

        #endregion
    }
}
