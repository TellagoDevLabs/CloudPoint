using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SPSM.Common.Media.Model;

namespace SPSM.Common.Media.Validators
{
    public class ValidatorFactory
    {
        public static IMediaValidator GetValidator(MediaType mediaType, MediaConfig config)
        {
            switch (mediaType)
            {
                case MediaType.Audio: return new AudioValidator(config);
                case MediaType.Video: return new VideoValidator(config);
                case MediaType.Image: return new ImageValidator(config);
                default: return null;
            }
        }
    }
}
