using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using BlueWind.Crawler.Manga;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using BlueWind.Crawler.Core;
using System;
using BlueWind.Crawler.Manga.Domain;

namespace BlueWind.Crawler.Manga.Site.Manga24h
{
    public class Manga24hChapter : MangaChapter
    {

        public Manga24hChapter()
        {

        }
        public Manga24hChapter(string siteUri, string name, MangaSeries series,string uploadedDate="", string viewCountSource="0")
        {
            this.SiteUri = siteUri;
            this.Name = name.Trim().RemoveInvalidChars();
            this.HomeSeries = series;
            this.UpdatedDate = uploadedDate;
            int count = 0;
            if (Int32.TryParse(viewCountSource, out count))
                this.SourceViewCount = count;
            ChapterPath = "Sites\\" + HomeSeries.HomeSite.Name + "\\" + HomeSeries.Name + "\\" + Name + "\\";
            this.Discriminator = "Manga24hChapter";

        }
        public override IEnumerable<ImageCache> GetPages()
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(HttpUtility.GetResponseString(SiteUri));
            var query = doc.DocumentNode.SelectNodes("//div[@class='row']/div[@class='view2']/img");
            if (query != null)
            {
                foreach (var node in query)
                {
                    yield return new ImageCache() { Url = node.Attributes["src"].Value, HomeChapter = this };
                }
            }
        }
    }
}