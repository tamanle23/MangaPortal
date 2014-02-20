using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlueWind.Crawler.Core;
using System;
using System.Globalization;
using BlueWind.Crawler.Manga.Domain;

namespace BlueWind.Crawler.Manga.Site.KissManga
{
    internal class KissMangaSeries : MangaSeries
    {
        public KissMangaSeries()
        { }
        public KissMangaSeries(string siteUri, string name, string tag, MangaSite site, ProgressStatus status)
        {
            this.SiteUri = siteUri;
            this.Tag = tag;
            this.Name = name.Trim().RemoveInvalidChars();
            this.FullText = this.Name + " " + this.Tag;
            this.HomeSite = site;
            this.Status = (byte)status;
            SeriesFolderPath ="Sites\\"+ HomeSite.Name + "\\" + Name + "\\";
        }
        public override void GetChapters()
        {
            if (MangaChapters == null || MangaChapters.Count < 1)
            {
                MangaChapters = new List<MangaChapter>();
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(HttpUtility.GetResponseString(SiteUri));
                var path = "//table[@class='tablesorter'][1]//tbody//tr";
                var query = doc.DocumentNode.SelectNodes(path);
                if (query != null)
                    foreach (var node in query)
                    {
                        try
                        {
                            var list = new List<HtmlNode>(node.ChildNodes.Where(n => n.Name == "td"));
                            MangaChapter chapter = new KissMangaChapter(HomeSite.SiteUri + "/" + list[0].ChildNodes[0].Attributes["href"].Value, list[0].ChildNodes[0].InnerText, this, list[2].InnerText, list[1].InnerText);
                            MangaChapters.Add(chapter);
                        }
                        catch (Exception ex)
                        {
                            Logger.Write(ex.Message);
                        }
                    }
            }
        }
        public override void UpdateChapters(bool isDeepScan = false, bool isUseDb = false)
        {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(HttpUtility.GetResponseString(SiteUri));
                this.LastUpdatedSource=MangaChapters.Select(n => n.UpdatedDate).Max();
                var path = "//table[@class='tablesorter'][1]//tbody//tr";
                var query = doc.DocumentNode.SelectNodes(path);
                if (query != null)
                {
                    foreach (var nodes in query.Select(n => n.ChildNodes))
                    {
                        try
                        {
                            var list = new List<HtmlNode>(nodes.Where(n => n.Name == "td"));
                            if (KissMangaSite.CheckDate(this.LastUpdatedSource, list[2].InnerText))
                            {
                                var chapter = new KissMangaChapter(HomeSite.SiteUri + "/" + list[0].ChildNodes[0].Attributes["href"].Value, list[0].ChildNodes[0].InnerText, this, list[2].InnerText, list[1].InnerText);
                                MangaChapters.Add(chapter);
                                if (callback != null)
                                    callback(null);
                                chapter.Scan(callback, isDeepScan, isUseDb);
                            }
                            else break;
                        }
                        catch (Exception ex)
                        {
                            Logger.Write(ex.Message);
                        }
                    }
                    this.LastUpdatedSource = MangaChapters.Select(n => n.UpdatedDate).Max();
                    if (callback != null)
                        callback(null);

                }
        }

        public override void UpdateViewCount(System.Func<object, bool> Callback)
        {

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HttpUtility.GetResponseString(SiteUri));
            var path = "//table[@class='tablesorter'][1]//tbody//tr";
            var query = doc.DocumentNode.SelectNodes(path);
            MangaChapter chapter = null;
            int count = 0;
            if (query != null)
                foreach (var node in query)
                {
                    try
                    {
                        var list = new List<HtmlNode>(node.ChildNodes.Where(n => n.Name == "td"));
                        chapter = MangaChapters.SingleOrDefault(n => n.Name == list[0].ChildNodes[0].InnerText.Trim().RemoveInvalidChars());
                        if (chapter != null)
                        {
                            if (Int32.TryParse(list[1].InnerText, out count))
                            {
                                chapter.SourceViewCount = count;
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Write(ex.Message); }
                }
            if (Callback != null)
                Callback(null);
        }

        public override DateTime GetDate(string dateString)
        {
            throw new NotImplementedException();
        }

        public override bool CheckDate(string last, string current)
        {
            throw new NotImplementedException();
        }
    }
}