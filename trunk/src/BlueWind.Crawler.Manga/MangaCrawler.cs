using BlueWind.Common;
using BlueWind.Crawler.Core;
using BlueWind.Crawler.Manga.Domain;
using BlueWind.Crawler.Manga.Site.Manga24h;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BlueWind.Crawler.Manga
{
    internal class MangaCrawler
    {

        public static TimeSpan Crawl()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            MangaCrawlParameter arguments = ServiceLocator.Current.GetInstance<MangaCrawlParameter>();
            if (arguments.IsNeedClean)
            {
                var series = Directory.GetDirectories("Sites\\" + arguments.Site, "*", SearchOption.TopDirectoryOnly);
                foreach (var serie in series)
                {
                    if (Directory.GetDirectories(serie, "*", SearchOption.TopDirectoryOnly).Length == 0)
                    {
                        var files = Directory.GetFiles(serie, "*.log", SearchOption.TopDirectoryOnly);
                        if (files.Length > 0)
                            File.Delete(files[0]);
                    }
                }
            }
            MangaSite site = null;
            switch (arguments.Site)
            {
                case BuiltInSites.Manga24h:
                    site = new Manga24hSite();
                    break;

                default:
                    return TimeSpan.Zero;
            }
            if (site != null)
            {
                string siteName = arguments.Site.ToString();
                if (arguments.UseDb)
                {
                    using (var context = ServiceLocator.Current.GetInstance<SitesContext>())
                    {

                        if (context.Set<MangaSite>().Where(n => n.Name == siteName).Count() != 0)
                        {
                            site = context.Set<MangaSite>().Single(n => n.Name == siteName);
                        }
                        else
                        {
                            context.Set<MangaSite>().Add(site);
                            context.Save();
                        }
                        if (context.IsFirstInitialization)
                            arguments.IsDeepScan = true;
                        context.Entry(site).State = EntityState.Detached;
                    }
                }
                if(arguments.IsUpdateSeries)
                {
                    site.UpdateSeries();
                }
                if (arguments.IsScan)
                {
                    site.Scan();
                }
                if (arguments.IsUpdateInfo)
                {
                    site.UpdateInfo();
                }
                if (arguments.IsUpdateViewCount)
                {
                    site.UpdateViewCount();
                }
                if (arguments.IsUpdateChapters)
                {
                    site.UpdateChapters();
                }
            }
            watch.Stop();
            return watch.Elapsed;
        }

        public static MangaCrawlParameter GetParameters(string[] args)
        {
            var arguments = new MangaCrawlParameter();
            if (args.Length > 1)
            {
                bool isValid = true;
                BuiltInSites builtInSite = 0;
                ProgressStatus status = 0;
                isValid &= Enum.TryParse<BuiltInSites>(args[0], out builtInSite);
                arguments.Site = builtInSite;
                isValid &= Enum.TryParse<ProgressStatus>(args[1], out status);
                arguments.Status = status;

                if (args.Contains("/clean")) arguments.IsNeedClean = true;
                if (args.Contains("/updateinfo")) arguments.IsUpdateInfo = true;
                if (args.Contains("/updateviewcount")) arguments.IsUpdateViewCount = true;
                if (args.Contains("/updatechapters")) arguments.IsUpdateChapters = true;
                if (args.Contains("/scan")) arguments.IsScan = true;
                if (args.Contains("/updateseries")) arguments.IsUpdateSeries = true;
                if (args.Contains("/deep")) arguments.IsDeepScan = true;
                if (args.Contains("/usedb")) arguments.UseDb = true;
                arguments.IsValid = isValid;
            }
            return arguments;
        }

    }
}



//Callback =
//   (obj) =>
//   {
//       if (obj != null)
//       {
//           if (obj.GetType() == typeof(string))
//           {
//               try
//               {
//                   if (command != null)
//                   {
//                       connection.Open();
//                       foreach (var commandText in ((string)obj).Split(';').Where(n => n != ""))
//                       {
//                           command.CommandText = commandText;
//                           command.ExecuteNonQuery();
//                       }
//                       return true;
//                   }
//               }
//               catch (Exception ex)
//               {
//                   Logger.Write("Error",LogLevel.Error,ex);
//               }
//               finally
//               {
//                   connection.Close();
//               }
//           }
//       }
//       bool result = context.Save();
//       return result;
//   };