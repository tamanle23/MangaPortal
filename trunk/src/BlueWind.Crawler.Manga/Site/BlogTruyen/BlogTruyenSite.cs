using BlueWind.Crawler.Core;
using BlueWind.Crawler.Manga.Domain;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueWind.Crawler.Manga.Site.BlogTruyen
{
    internal class BlogTruyenSite : MangaSite
    {
        public BlogTruyenSite()
            : base()
        {
            this.Name = "BlogTruyen";
            this.SiteUri = "http://BlogTruyen.com";
            this.Language = "vi-VN";
        }
        public string MangaListUri
        {
            get
            {
                return @"POST http://BlogTruyen.com/partialDanhSach/listtruyen/ HTTP/1.1
Host: BlogTruyen.com
Connection: keep-alive
Origin: http://BlogTruyen.com
X-Requested-With: XMLHttpRequest
Content-Type: application/x-www-form-urlencoded; charset=UTF-8
Referer: http://BlogTruyen.com/danhsach/tatca
Accept-Encoding: gzip,deflate,sdch
Accept-Language: vi,en-US;q=0.8,en;q=0.6

listOrCate=list&key=tatca&page={0}&orderBy=title";
            }
        }
        public override void GetSeries(ProgressStatus progressStatus = ProgressStatus.Unknown)
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
                            series = new BlogTruyenSeries(SiteUri + "/" + node.Attributes["href"].Value, node.InnerText, _node.ChildNodes[5].ChildNodes[1].InnerText, this, (ProgressStatus)i);
                            series.HomeSite = this;
                            MangaSeries.Add(series);
                        }
                    }
                }
            }
        }
        public override void UpdateInfo(Func<object, bool> callback)
        {
        }

        public override void UpdateViewCount(Func<object, bool> Callback)
        {

        }


    }
}