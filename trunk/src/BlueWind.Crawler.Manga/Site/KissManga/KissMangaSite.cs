using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BlueWind.Crawler.Core;
using System.Globalization;
using BlueWind.Crawler.Manga.Domain;

namespace BlueWind.Crawler.Manga.Site.KissManga
{
    internal class KissMangaSite : MangaSite
    {
        public KissMangaSite()
            : base()
        {
            this.Name = "KissManga";
            this.SiteUri = "http://KissManga.com";
            this.Language = "en-US";
        }
        public string MangaListUri
        {
            get
            {
                return SiteUri + "/manga/status";
            }
        }
        public override void GetSeries(ProgressStatus progressStatus)
        {

            if (MangaSeries == null || MangaSeries.Count < 1)
            {
                MangaSeries = new List<MangaSeries>();
                HtmlDocument doc = new HtmlDocument();
                int i = 0;
                int max = 0;
                if (progressStatus == ProgressStatus.Unknown)
                {
                    i = 1;
                    max = (int)ProgressStatus.Suspended + 1;
                }
                else
                {
                    i = (int)progressStatus;
                    max = (int)progressStatus + 1;
                }
                for (; i < max; i++)
                {
                    doc.LoadHtml(HttpUtility.GetResponseString(MangaListUri + "/" + i));
                    var query = doc.DocumentNode.SelectNodes("//div[@class='row mangalist']//div[@class='pagination'][1]//a");
                    if (query != null)
                    {
                        var node = query.LastOrDefault();
                        var hrefValue = node != null ? node.Attributes["href"].Value : "";
                        int totalPages = Int32.Parse(hrefValue.Substring(hrefValue.LastIndexOf('/') + 1));

                        for (int j = 1; j <= totalPages; j++)
                        {
                            MangaSeries series = null;
                            doc.LoadHtml(HttpUtility.GetResponseString(MangaListUri + "/" + i + "/page/" + j));
                            query = doc.DocumentNode.SelectNodes("//div[@class='row mangalist']//div[@class='descr']");
                            foreach (var _node in query)
                            {
                                node = _node.ChildNodes["a"];
                                series = new KissMangaSeries(SiteUri + "/" + node.Attributes["href"].Value, node.InnerText, _node.ChildNodes[5].ChildNodes[1].InnerText, this, (ProgressStatus)i);
                                series.HomeSite = this;
                                MangaSeries.Add(series);
                            }
                        }
                    }
                }
            }
        }
        public override void UpdateInfo(Func<object, bool> callback)
        {
            List<string> removeStrings = new List<string>(File.ReadAllLines("RemoveStrings.txt"));
            HtmlDocument doc = new HtmlDocument();
            foreach (var series in MangaSeries)
            {
                doc.LoadHtml(HttpUtility.GetResponseString(series.SiteUri));
                if (series.ThumbnailBuffer == null)
                {
                    var query = doc.DocumentNode.SelectSingleNode("//div[@class='span2']//img[@class='img-rounded'][1]");
                    if (query != null)
                    {
                        Stream stream = HttpUtility.GetResponse(query.Attributes["src"].Value);
                        if (stream != null)
                        {
                            series.ThumbnailBuffer = stream.GetThumbnail(80,80);
                            if (callback != null)
                                callback(null);
                        }
                    }
                }
                if (series.Status != (byte)ProgressStatus.Completed)
                {
                    var query = doc.DocumentNode.SelectSingleNode("//div[@class='span5']//ul[@class='mangainfo']//span[@class='info_tinhtrang'][1]");
                    switch (query.InnerText)
                    {
                        case "Hoàn Thành":
                            series.Status = (byte)ProgressStatus.Completed;
                            break;
                        case "Đang Tiến Hành":
                            series.Status = (byte)ProgressStatus.Ongoing;
                            break;
                        case "Tạm Ngưng":
                            series.Status = (byte)ProgressStatus.Suspended;
                            break;
                        default:
                            series.Status = (byte)ProgressStatus.Unknown;
                            break;
                    }
                    if (callback != null)
                        callback(null);
                }
                if (series.Overview == null || series.Overview == "")
                {
                    var query = doc.DocumentNode.SelectNodes("//div[@class='span8']/p//p");

                    if (query != null)
                    {
                        if (query.Count > 1)
                        {
                            var builder = new StringBuilder(System.Web.HttpUtility.HtmlDecode(query.Skip(1).Select(n => n.InnerText).Aggregate(
                                (n, m) =>
                                {
                                    return n + "\n" + m;
                                })));
                            foreach (string item in removeStrings)
                            {
                                builder.Replace(item, "");
                            }
                            series.Overview = builder.ToString();
                            if (callback != null)
                                callback(series);
                        }

                    }

                }

            }

        }
        public override void UpdateViewCount(Func<object, bool> callback)
        {
            List<string> removeStrings = new List<string>(File.ReadAllLines("RemoveStrings.txt"));
            HtmlDocument doc = new HtmlDocument();
            foreach (var series in MangaSeries)
            {
                doc.LoadHtml(HttpUtility.GetResponseString(series.SiteUri));
                var query = doc.DocumentNode.SelectSingleNode("//div[@class='span5']//ul[@class='mangainfo']//span[@class='info_view'][1]");
                if (query != null)
                {
                    try
                    {
                        series.SourceViewCount = Int32.Parse(query.InnerText);
                    }
                    catch
                    { }
                }
                query = doc.DocumentNode.SelectSingleNode("//div[@class='span5']//ul[@class='mangainfo']//span[@class='info_ngay'][1]");
                //if (query != null)
                //{
                //    series.UpdateViewCount(callback);
                //    series.LastUpdatedSource = query.InnerText;
                //}
                if (callback != null)
                    callback(null);

            }
        }
        private static DateTime GetDate(string dateString)
        {
            DateTime date;
            DateTime.TryParseExact(dateString.Trim(), "yyyy-MM-dd", null, DateTimeStyles.None, out date);
            return date;
        }
        public  static bool CheckDate(string last, string current)
        {
            try
            {
                return DateTime.ParseExact(last.Trim(), "yyyy-MM-dd", null) < DateTime.ParseExact(current.Trim(), "yyyy-MM-dd", null);
            }
            catch
            {
                return false;
            }
        }

    }


}

