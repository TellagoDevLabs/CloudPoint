using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tellago.SP.Media.Model.Validators
{
    class ValidatorFactory
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
