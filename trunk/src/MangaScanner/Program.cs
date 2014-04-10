using BlueWind.Crawler.Manga;
using BlueWind.Crawler.Manga.Domain;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using ProjectX.Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MangaScanner
{
    internal class Program
    {
        public static UnityContainer container;
        public static string LogPath;
        static Program()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;
            container = new UnityContainer();
            var unityServiceLocator = new UnityServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => unityServiceLocator);
            container.RegisterType<MangaDataContext>("SitesContext");
            LogPath = String.Format("ApplicationSessionLog_{0:yyyyMMddHHmmss}.log", DateTime.Now);
            Logger.LogAction = LogCallback;
            Logger.LogAction = (message, level, ex) =>
            {
                Console.WriteLine(message);
            };
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Write(LogLevel.Error, "Unhandled Exception", (Exception)e.ExceptionObject);
        }

        private static void LogCallback(string message, LogLevel level, Exception ex)
        {
            if (!File.Exists(LogPath))
            {
                File.AppendAllText(LogPath, "===== Manga Portal Server Log File =====");
            }
            switch (level)
            {
                case LogLevel.Error:
                    Console.WriteLine(message);
                    File.WriteAllText(LogPath, "\n" + String.Format("---------- {0} {1} ----------", message, DateTime.Now.ToLongDateString()));
                    File.AppendAllText(LogPath, "\r\n" + String.Format("{0} [{1}] {2}", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"), level, message));
                    File.AppendAllText(LogPath, GetExceptionText(ex));
                    break;

                case LogLevel.UserDefining:
                    Console.WriteLine(message);
                    break;
            }
        }
      
        private static void Main(string[] args)
        {
            container.RegisterInstance<MangaCrawlParameter>(MangaCrawler.GetParameters("mangascanner.ini"));
            //MangaSettings.Default["ConnectionString"] = "data source=" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Manga.sdf;Persist Security Info=False;";
            //MangaSettings.Default["ConnectionString"] = "data source=.\\SQLEXPRESS;database=Manga;multipleactiveresultsets=True;Integrated Security=true";
            //var taskSchedule = new PriorityScheduler(System.Threading.ThreadPriority.Lowest);
            //var taskFactory = new TaskFactory(taskSchedule);
            //var task = taskFactory.StartNew(() =>
            //{
                var timespan = MangaCrawler.Crawl();
                Console.WriteLine(String.Format("======== {0}", timespan.TotalHours));
                Console.ReadLine();
            //});
            //task.Wait();
        }

        private static string GetExceptionText(Exception ex, int count = 0)
        {
            StringBuilder builder = new StringBuilder();
            if (ex != null)
            {
                var lineHead = "".PadLeft(count, '\t');
                builder.Append("\r\n" + lineHead + "== Exception");
                builder.Append("\r\n" + lineHead + "==== Message:\t" + ex.Message);
                if (ex.StackTrace != null)
                    builder.Append("\r\n" + lineHead + "==== Trace:\t" + ex.StackTrace.Replace("\n", "\n" + lineHead));
                if (ex.InnerException != null)
                {
                    builder.Append(GetExceptionText(ex.InnerException, count + 1));
                }
            }
            return builder.ToString();
        }
    }
}