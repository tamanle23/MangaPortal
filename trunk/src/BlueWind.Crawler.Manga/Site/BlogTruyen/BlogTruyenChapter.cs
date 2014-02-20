using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using BlueWind.Crawler.Manga;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using BlueWind.Crawler.Core;
using BlueWind.Crawler.Manga.Domain;

namespace BlueWind.Crawler.Manga.Site.BlogTruyen
{
    internal class BlogTruyenChapter : MangaChapter
    {
        private  string chapterPath;
        public BlogTruyenChapter()
        {

        }
        public BlogTruyenChapter(string siteUri, string name, MangaSeries series,string uploadedDate="")
        {
            this.SiteUri = siteUri;
            this.Name = name.Trim().RemoveInvalidChars();
            this.HomeSeries = series;
            this.UpdatedDate = uploadedDate;
            chapterPath = HomeSeries.HomeSite.Name + "\\" + HomeSeries.Name + "\\" + Name + "\\";

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