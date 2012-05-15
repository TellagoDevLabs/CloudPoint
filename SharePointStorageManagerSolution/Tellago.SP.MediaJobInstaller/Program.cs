using System;
using System.Linq;
using Microsoft.SharePoint;
using System.Text;
using Microsoft.SharePoint.Administration;
using Tellago.SP.Media.Backend;

namespace Tellago.SP.MediaJobInstaller
{
    class Program
    {
        private const string MediaTimerJobName = "tellago-job-media-processing";
        
        static void Main(string[] args)
        {
            if (args.Length != 3 && args.Length != 1)
            {
                throw new Exception("Bad parameters - " + Usage);
            }
            string operation = args[0];
            if ("i".Equals(operation, StringComparison.InvariantCultureIgnoreCase))
            {
                if (args.Length != 3)
                {
                    throw new Exception("Bad parameters - " + Usage);
                }
                string serverName = args[1];
                string webUrl = args[2];
                string webRelativeUrl = new Uri(webUrl).PathAndQuery;
                Console.WriteLine("Installing timer job '{0}' on server '{1}' for site '{2}'", MediaTimerJobName, serverName, webUrl);
                DeleteJob(MediaTimerJobName);
                InstallJob(webUrl, MediaTimerJobName, serverName);
            }
            else if ("u".Equals(operation, StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Uninstalling timer job '{0}'", MediaTimerJobName);
                DeleteJob(MediaTimerJobName);
            }
            else
            {
                throw new Exception("Invalid Operation " + Usage);
            }
            Console.WriteLine("done!");
        }

        private static string Usage
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine("USAGE");
                sb.AppendLine("For installing:");
                sb.AppendLine(" Tellago.SP.MediaJobInstaller.exe i <serverName> <webUrl>");
                sb.AppendLine("For uninstalling:");
                sb.AppendLine(" Tellago.SP.MediaJobInstaller.exe u ");
                return sb.ToString();
            }
        }

        private static void InstallJob(string webUrl, string jobName, string serverName)
        {
            SPServer appServer = null;
            foreach (var server in SPServer.Local.Farm.Servers)
            {
                if (server.Name.Equals(serverName, StringComparison.InvariantCultureIgnoreCase))
                {
                    appServer = server;
                    break;
                }
            }
            if (appServer == null)
            {
                throw new Exception(String.Format("Server '{0}' was not found in the farm", serverName));
            }


            var job = new MediaProcessingTimerJob(jobName, appServer);

            job.Properties.Add("webUrl", webUrl);

            var schedule = new SPMinuteSchedule();
            schedule.BeginSecond = 0;
            schedule.EndSecond = 59;
            schedule.Interval = 15;
            job.Schedule = schedule;
            job.Update();
        }

        private static void DeleteJob(string jobName)
        {
            //delete from timer service
            foreach (SPJobDefinition job in SPFarm.Local.TimerService.JobDefinitions)
            {
                if (job.Name == jobName)
                {
                    job.Delete();
                    Console.WriteLine(MediaTimerJobName + " DELETED from timer service");
                }

            }
        }

    }
}
