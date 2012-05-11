using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPSM.Common.Media
{
    [Serializable]
    public class MediaTooLargeException : Exception
    {
        public MediaTooLargeException() { }
        public MediaTooLargeException(string message) : base(message) { }
        public MediaTooLargeException(string message, Exception inner) : base(message, inner) { }
        protected MediaTooLargeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
