using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace Tellago.SP.Common.Configuration
{
    /// <summary>
    /// Exception which occurs either when the config store is incorrectly configured or the requested config 
    /// item cannot be found.
    /// </summary>
    [Serializable]
    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException()
            : base()
        {

        }

        public InvalidConfigurationException(string Message)
            : base(Message)
        {

        }

        public InvalidConfigurationException(string Message, Exception InnerException)
            : base(Message, InnerException)
        {

        }
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

    }
}
