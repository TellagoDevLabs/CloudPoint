using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint;
using Tellago.SP.Media.Model;
using Tellago.SP.Logging;

namespace Tellago.SP.Media.Backend
{
    public class MediaProcessingTimerJob : SPServerJobDefinition
    {
        private Logger logger = new Logger();

        public MediaProcessingTimerJob()
            : base()
        {
        }
        
        public MediaProcessingTimerJob(string name,SPServer server):base(name,server)
        {
            this.Title = "Tellago-MediaProcessingTimerJob";
        }

        public override void Execute(SPJobState jobState)
        {
            string webUrl = String.Empty;
            try
            {
                if (this.Properties["webUrl"] == null)
                {
                    throw new ArgumentNullException("Properties['webUrl']","The 'webUrl' property is missing from timer job instance. Install job again.");
                }
                webUrl = this.Properties["webUrl"].ToString();
                var mediaProcessor = new MediaProcessor(webUrl, MediaConfig.FromConfigStore(webUrl));
                logger.LogToOperations(LogCategories.Media, EventSeverity.Information, "Executing Tellago-MediaProcessingTimerJob in webUrl '{0}' (Server: '{1}', Service '{2}', WebApplication: '{3}')", webUrl, Server, Service, WebApplication);
                mediaProcessor.ProcessMedia();
            }
            catch (Exception ex)
            {
                logger.LogToOperations(ex, LogCategories.Media, EventSeverity.Error, "Error executing TRS-MediaProcessingTimerJob in web '{0}'", webUrl);

                throw;
            }
        }
    }
}
