using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPSM.Common.Media
{
    [Serializable]
    public class MediaProcessingException : Exception
    {
        public MediaProcessingException() { }
        public MediaProcessingException(string message) : base(message) { }
        public MediaProcessingException(string message, Exception inner) : base(message, inner) { }
        protected MediaProcessingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
