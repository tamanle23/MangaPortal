using BlueWind.Crawler.Core;
using BlueWind.Crawler.Manga;
using BlueWind.Crawler.Manga.Site.Manga24h;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Text;
using BlueWind.Common;
using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.ServiceLocation;
using BlueWind.Crawler.Manga.Domain;
using BlueWind.Common.Utility;

namespace ScanManga
{
    internal class Program
    {
        public static string LogPath;
        static Program()
        {
            container = new UnityContainer();
            var unityServiceLocator = new UnityServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => unityServiceLocator);
            container.RegisterType<SitesContext>("SitesContext");

        }
        public static UnityContainer container;
        private static void Main(string[] args)
        {

            LogPath = String.Format("ApplicationSessionLog_{0:yyyyMMddHHmmss}.log", DateTime.Now);
            Logger.LogAction = LogCallback;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Logger.LogAction = (message, level, ex) =>
            {
                Console.WriteLine(message);
            };
            container.RegisterInstance<MangaCrawlParameter>(MangaCrawler.GetParameters(args));
            MangaSettings.Default["ConnectionString"] = "data source="+System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Manga.sdf;Persist Security Info=False;";
            MangaSettings.Default["ConnectionString"] = "data source=.\\SQLEXPRESS;database=Manga;multipleactiveresultsets=True;Integrated Security=true";

            MangaCrawler.Crawl();
            Console.WriteLine(String.Format("======== {0}", watch.Elapsed.TotalHours));
            Console.ReadLine();
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.BeginWrite("Unhandled Exception",LogLevel.Error,(Exception)e.ExceptionObject);
        }
        static void LogCallback(string message, LogLevel level, Exception ex)
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
                    WriteException(ex);
                    break;
                case LogLevel.Info:
                    Console.WriteLine(message);
                    break;
                case LogLevel.Warn:
                    break;
                case LogLevel.Debug:
                    break;

            }
            if (level == LogLevel.Error)
            {
            }


        }
        private static void WriteException(Exception ex)
        {
            int count = 0;
            File.AppendAllText(LogPath, "\n" + ex.Message + "\n" + ex.StackTrace);
            if (ex.InnerException != null)
            {
                count++;
                StringBuilder lineHead = new StringBuilder("");
                for (int i = 0; i < count; i++)
                {
                    lineHead.Append("\t");
                }
                File.AppendAllText(LogPath, "\n" + lineHead.ToString() + "== InnerException");
                File.AppendAllText(LogPath, "\n" + lineHead.ToString() + "==== Message:\t" + ex.InnerException.Message);
                File.AppendAllText(LogPath, "\n" + lineHead.ToString() + "==== Trace:\t" + ex.InnerException.StackTrace.Replace("\n", "\n" + lineHead));
            }
        }
    }
}