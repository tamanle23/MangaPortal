using BlueWind.Crawler.Core;
using BlueWind.Crawler.Manga.Domain;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BlueWind.Crawler.Manga.Site.BlogTruyen
{
    internal class BlogTruyenSeries : MangaSeries
    {
        private string seriesPath;

        public BlogTruyenSeries()
        { }

        public BlogTruyenSeries(string siteUri, string name, string tag, MangaSite site, ProgressStatus status)
        {
            this.SiteUri = siteUri;
            this.Tag = tag;
            this.Name = name.Trim().RemoveInvalidChars();
            this.FullText = Name.RemoveVietnameseSign() + " " + this.Tag;
            this.HomeSite = site;
            this.Status = (byte)status;
            seriesPath = HomeSite.Name + "\\" + Name + "\\";
        }
        public override void GetChapters()
        {
            if (MangaChapters == null || MangaChapters.Count < 1)
            {
                MangaChapters = new List<MangaChapter>();
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(HttpUtility.GetResponseString(SiteUri));
                var xpath = "//table[@class='tablesorter'][1]//tbody//tr";
                var query = doc.DocumentNode.SelectNodes(xpath);
                if (query != null)
                    foreach (var node in query)
                    {
                        try
                        {
                            var list = new List<HtmlNode>(node.ChildNodes.Where(n => n.Name == "td"));
                            MangaChapter chapter = new BlogTruyenChapter(HomeSite.SiteUri + "/" + list[0].ChildNodes[0].Attributes["href"].Value, list[0].ChildNodes[0].InnerText, this, list[2].InnerText);
                            MangaChapters.Add(chapter);
                        }
                        catch { }
                    }
            }
        }


        public override void UpdateChapters(bool isDeepScan = false, bool isUseDb = false)
        {

        }

        public override void UpdateViewCount(System.Func<object, bool> Callback)
        {
            throw new System.NotImplementedException();
        }

        public override System.DateTime GetDate(string dateString)
        {
            throw new System.NotImplementedException();
        }

        public override bool CheckDate(string last, string current)
        {
            throw new System.NotImplementedException();
        }
    }
}