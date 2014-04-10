using BlueWind.Crawler.Core;
using BlueWind.Crawler.Manga.Domain;
using BlueWind.Crawler.Manga.Site.Manga24h;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

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
                    using (var context = ServiceLocator.Current.GetInstance<MangaDataContext>())
                    {

                        if (context.Set<MangaSite>().Where(n => n.Name == siteName).Count() != 0)
                        {
                            site = context.Set<MangaSite>().Single(n => n.Name == siteName);
                        }
                        else
                        {
                            context.Set<MangaSite>().Add(site);
                            context.SaveChanges();
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

        public static MangaCrawlParameter GetParameters(IDictionary<string,string> args)
        {
            var arguments = new MangaCrawlParameter();
            if (args != null)
            {
                if (args.Count() > 0)
                {
                    bool isValid = true;
                    BuiltInSites builtInSite = 0;
                    ProgressStatus status = 0;
                    if (args.ContainsKey("site"))
                        isValid &= Enum.TryParse<BuiltInSites>(args["site"], out builtInSite);
                    arguments.Site = builtInSite;
                    if (args.ContainsKey("status"))
                        isValid &= Enum.TryParse<ProgressStatus>(args["status"], out status);
                    arguments.Status = status;

                    if (args.ContainsKey("cs")) arguments.ConnectionString = args["cs"];
                    if (args.ContainsKey("clean")) arguments.IsNeedClean = true;
                    if (args.ContainsKey("updateinfo")) arguments.IsUpdateInfo = true;
                    if (args.ContainsKey("updateviewcount")) arguments.IsUpdateViewCount = true;
                    if (args.ContainsKey("updatechapters")) arguments.IsUpdateChapters = true;
                    if (args.ContainsKey("scan")) arguments.IsScan = true;
                    if (args.ContainsKey("updateseries")) arguments.IsUpdateSeries = true;
                    if (args.ContainsKey("deep")) arguments.IsDeepScan = true;
                    if (args.ContainsKey("usedb")) arguments.UseDb = true;
                    arguments.IsValid = isValid;
                }
            }
            return arguments;
        }
        private class ParamComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return x.Split('=').FirstOrDefault() == y.Split('=').FirstOrDefault();
            }

            public int GetHashCode(string obj)
            {
                return obj.Split('=').FirstOrDefault().GetHashCode();
            }
        }

        public static MangaCrawlParameter GetParameters(string iniFileName, bool isSelfHost=true)
        {
            var iniFilePath = Directory.GetParent(Assembly.GetExecutingAssembly().FullName).FullName + "\\" + iniFileName;
            IEnumerable<string> parameters = null;
            Dictionary<string, string> paramDic = null;

            if (isSelfHost)
            {
                if (File.Exists(iniFilePath))
                {
                    parameters = File.ReadAllText(iniFilePath).Split('-').Select(m => m.Trim().Trim('\0', '\t', '\n', '\r')).Distinct(new ParamComparer());
                }

            }
            else
            {
                parameters = ((String)System.Configuration.ConfigurationManager.AppSettings["Parameters"]).Split('-').Select(m=>m.Trim().Trim('\0','\t','\n','\r')).Distinct(new ParamComparer());
            }
            if(parameters!=null)
            paramDic = parameters.ToDictionary(m => m.Split('=').FirstOrDefault(), m =>
            {
                var tmp = m.Split('=').Skip(1);
                if (tmp.Count() > 0)
                {
                    if (tmp.Count() > 0)
                        return tmp.Aggregate((part1, part2) => part1 + "=" + part2);
                    else
                        return tmp.FirstOrDefault();
                }
                else
                    return null;
            });

            return MangaCrawler.GetParameters(paramDic);
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