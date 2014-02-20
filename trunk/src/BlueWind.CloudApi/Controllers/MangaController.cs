using BlueWind.CloudApi.Attributes;
using BlueWind.CloudApi.Models;
using BlueWind.CloudApi.Utility;
using BlueWind.Common;
using BlueWind.Crawler.Core;
using BlueWind.Crawler.Manga;
using BlueWind.Crawler.Manga.Domain;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;

namespace BlueWind.CloudApi.Controllers
{
    [AllowCrossSite]
    public class MangaController : System.Web.Http.ApiController
    {
        private static string connectionString;
        private static SitesContext MangaDataContext;

        private static MangaSite MangaSite { get; set; }

        private static int numberOfSeries;
        private static int numberOfParts;
        private static DbConnection connection;
        private static DbCommand command;

        static MangaController()
        {
            Refresh();
        }

        public MangaController()
        {
        }

        public static void Refresh()
        {
        }

        public static void Initialize()
        {
            using (var context = ServiceLocator.Current.GetInstance<SitesContext>())
            {
                numberOfSeries = context.Series.Count();
            }
        }

        private static object _lock = new object();

        [HttpGet]
        [ApiCacheActionFilter(2000, 1000, false)]
        public object GetNumberOfSeries()
        {
            object result = 0;
            result = numberOfSeries;
            return result;
        }

        [HttpGet]
        [ApiCacheActionFilter(200, 1000, false)]
        public object GetPart(int part, int number = 10, string site = null)
        {
            return GetPartDirectly(part, number, site);
        }

        [HttpGet]
        [ApiCacheActionFilter(200, 1000, false)]
        public object GetPartDirectly(int part, int number, string site)
        {
            object result = null;
            try
            {
                if (number > 100)
                    number = 10;
                if (numberOfSeries > 0 & numberOfSeries < number)
                    numberOfParts = 1;
                else
                    if (numberOfSeries % number == 0)
                        numberOfParts = numberOfSeries / number;
                    else
                        numberOfParts = numberOfSeries / number + 1;

                if (part > numberOfParts) part = numberOfParts;
                using (var context = ServiceLocator.Current.GetInstance<SitesContext>())
                {
                    result = context.Series.OrderByDescending(n => n.ViewCount + n.SourceViewCount)
                        .Skip((part - 1) * number)
                        .Take(number)
                        .Select(series
                            => new
                              {
                                  Id = series.Id,
                                  Name = series.Name,
                                  Thumbnail = series.Thumbnail80,
                                  NumberOfChapters = series.MangaChapters.Count(),
                              }).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Write("Error", LogLevel.Error, ex);
            }
            return result;
        }

        [HttpGet]
        [ApiCacheActionFilter(200, 1000, false)]
        public object Search(string keywords)
        {
            object result = FullTextSearch(keywords);
            return result;
        }

        [HttpGet]
        [ApiCacheActionFilter(200, 1000, false)]
        public object NormalSearch(string keywords)
        {
            if (keywords == null || keywords == "") return null;
            object result = null;
            using (var context = ServiceLocator.Current.GetInstance<SitesContext>())
            {
                var keywordArray = keywords.Split('_').Select(n => n.ToLower());
                var query = (from series in context.Sites.Find(1).MangaSeries
                             where keywordArray.All(word => series.Name.ToLower().RemoveVietnameseSign().Contains(word))
                             select new
                             {
                                 Id = series.Id,
                                 Name = series.Name,
                                 Thumbnail = series.Thumbnail80,
                                 Overview = series.Overview,
                                 NumberOfChapters = series.MangaChapters.Count()
                             }).Take(100);
                result = query.ToArray();
            }
            return result;
        }

        [HttpGet]
        [ApiCacheActionFilter(200, 1000, false)]
        public object FuzzySearch(string keywords, double stdScore = 0.4)
        {
            if (keywords == null || keywords == "") return null;
            object result = null;
            using (var context = ServiceLocator.Current.GetInstance<SitesContext>())
            {
                keywords = keywords.Replace("_", "");
                var query = (from series in context.Sites.Find(1).MangaSeries
                             let distance = keywords.GetLevenshteinDistance(series.Name)
                             let length = Math.Max(series.Name.Length, keywords.Length)
                             let score = 1.0 - (double)distance / length
                             where score > stdScore
                             select new
                             {
                                 Id = series.Id,
                                 Name = series.Name,
                                 Thumbnail = series.Thumbnail80,
                                 Overview = series.Overview,
                                 NumberOfChapters = series.MangaChapters.Count()
                             }).Take(100);
                result = query.ToArray();
            }
            return result;
        }

        [HttpGet]
        [ApiCacheActionFilter(200, 1000, false)]
        public object FullTextSearch(string keywords)
        {
            if (keywords == null || keywords == "") return null;
            object result = null;
            using (var context = ServiceLocator.Current.GetInstance<SitesContext>())
            {
                keywords = keywords.Replace("'", " ").Replace("_", " ");
                var results = context.ObjContext().ExecuteStoreQuery<int>(String.Format("Select top 20 Id from MangaSeries where FREETEXT(FullText,'{0}')", keywords));
                if (results != null)
                {
                    result = (from series in context.Series
                              where results.Contains(series.Id)
                              select new
                              {
                                  Id = series.Id,
                                  Name = series.Name,
                                  Overview = series.Overview,
                                  Thumbnail = series.Thumbnail80,
                                  NumberOfChapters = series.MangaChapters.Count()
                              }).ToList();
                }
            }
            return result;
        }

        [HttpGet]
        [ApiCacheActionFilter(10 * 60, 60, false)]
        public object GetTopNew50()
        {
            object result = null;
            using (var context = ServiceLocator.Current.GetInstance<SitesContext>())
            {
                var query = from series in context.Series
                            orderby series.LastUpdatedSource descending
                            select new
                     {
                         Id = series.Id,
                         Thumbnail = series.Thumbnail80,
                         Name = series.Name,
                         Date = series.LastUpdatedSource
                     };

                result = query.Take(50).ToList();
            }
            return result;
        }

        [HttpGet]
        [ApiCacheActionFilter(1000, 1000, false)]
        public object GetChapter(int series, int chapter = 0)
        {
            object result = "";
            try
            {
                if (series == 0 & chapter == 0)
                    result = "";
                else
                    if (series == 0 & chapter > 0)
                    {
                        result = "";
                    }
                    else
                    {
                        using (var context = ServiceLocator.Current.GetInstance<SitesContext>())
                        {
                            if (chapter != 0)
                            {
                                var list = from page in context.ImageCaches
                                           where page.HomeChapter.Id == chapter
                                           select page;
                                context.Database.ExecuteSqlCommand("Update MangaChapters set ViewCount=ViewCount+1 where Id={0}", chapter);
                                result = list.Select(n => n.Url).ToList();
                            }
                            else
                            {
                                var mangaSeries = context.Series.Find(series);
                                context.Entry<MangaSeries>(mangaSeries)
                                    .Collection(n => n.MangaChapters)
                                    .Query()
                                    .OrderByDescending(SortChapterByName)
                                    .AsQueryable()
                                    .Load();
                                result = mangaSeries.MangaChapters
                                    .Select(
                                    n => new
                                    {
                                        Id = n.Id,
                                        Name = n.Name,
                                        Date = n.UpdatedDate
                                    }).OrderByDescending(n => n.Date).ToList();
                            }
                        }
                    }
            }
            catch (System.Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        [HttpGet]
        [ApiCacheActionFilter(300, 60, false)]
        public string GetImage(string imageUrl)
        {
            return Images[imageUrl];
        }

        [HttpGet]
        [ApiCacheActionFilter(400, 2000, false)]
        public object GetSeriesInfo(int id)
        {
            object result = null;
            using (var context = ServiceLocator.Current.GetInstance<SitesContext>())
            {
                var series = context.Series.Find(id);
                result = new
                {
                    Id = series.Id,
                    Name = series.Name,
                    Thumbnail = series.Thumbnail200,
                    Overview = series.Overview,
                    NumberOfChapters = context.Entry<MangaSeries>(series).Collection(m => m.MangaChapters).Query().Count(),
                };
            }
            return result;
        }

        internal static string SortChapterByName(MangaChapter arg)
        {
            var matches = new Regex(@"(([0-9])*[0-9])").Matches(arg.Name);
            if (matches.Count < 1) return "";

            return arg.Name.Substring(0, arg.Name.IndexOf(matches[0].Value)) + matches.Cast<Match>().Select(n => n.Value.PadLeft(3, '0')).Aggregate((m, n) => m + n);
        }

        internal static int SortSeriesByViewCount(MangaSeries arg)
        {
            return arg.ViewCount + arg.SourceViewCount;
        }

        //POST http://localhost:5798/api/manga HTTP/1.1
        //Host: localhost:5798
        //Content-Type: application/json
        //Content-Length: 35
        //Expect: 100-continue

        //{"Site":1,"Series":0,"Chapter":0,"SearchValue":""}
        [HttpPost]
        public object Post([FromBody]MangaPostContent value)
        {
            object result = null;
            if (value != null)
            {
                try
                {
                    using (var context = ServiceLocator.Current.GetInstance<SitesContext>())
                    {
                        if (value.Series == 0 & value.Chapter == 0)
                            result = context.Sites.SingleOrDefault(n => n.Id == value.Site).MangaSeries.Where(n => n.Name.Contains(value.SearchValue) || n.Tag.Contains(value.SearchValue)).Select(n => new { Id = n.Id, Name = n.Name });
                        else
                            if (value.Chapter != 0)
                            {
                                result = context.Sites
                                    .SingleOrDefault(n => n.Id == value.Site).MangaSeries
                                    .SingleOrDefault(n => n.Id == value.Series).MangaChapters
                                    .SingleOrDefault(n => n.Id == value.Chapter).ImageCaches
                                    .Select(n => n.Url);
                            }
                            else
                            {
                                result = context.Sites
                                    .SingleOrDefault(n => n.Id == value.Site).MangaSeries
                                    .SingleOrDefault(n => n.Id == value.Series).MangaChapters
                                    .Select(n => new { Id = n.Id, name = n.Name });
                            }
                    }
                }
                catch (System.Exception ex)
                {
                    result = ex.Message;
                }
            }
            return result;
        }

        private static volatile object callCommandLock = new object();

        [HttpPost]
        public object CallCommand([FromBody]dynamic parameter)
        {
            object result = null;
            if (!Bootstrapper.CheckPassword(parameter.Key))
            {
                result = false;
            }
            else
            {
                if (Monitor.TryEnter(callCommandLock, 0))
                {
                    try
                    {
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        result = ex.Message;
                    }
                    finally
                    {
                        Monitor.Exit(callCommandLock);
                    }
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        public void Put(int id, [FromBody]string value)
        {
        }

        public void Delete(int id)
        {
        }



        internal static Dictionary<string, string> Images { get; set; }
    }

    public class RegexSearch
    {
        //FuzzySearch _fuzz = new FuzzySearch("Foo McFoo");

        //var objects = from x in db.Foo
        //              where _fuzz.IsMatch(x.Name)
        //              select x;
        private string key;

        private IEnumerable<string> _searchTerms;
        private Regex _searchPattern;

        public RegexSearch(string searchTerm)
        {
            key = searchTerm;
            _searchTerms = searchTerm.Split(new Char[] { ' ' });
            _searchPattern = new Regex("(?i)(?=.*" + String.Join(")(?=.*" + ")", _searchTerms));
        }

        public bool IsMatch(string value)
        {
            if (key == value)
                return true;
            if (value.Contains(key))
                return true;
            if (_searchPattern.IsMatch(value))
                return true;
            return false;
        }
    }
}