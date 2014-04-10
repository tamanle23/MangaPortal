using BlueWind.Crawler.Core;
using BlueWind.Crawler.Manga.Domain;
using HtmlAgilityPack;
using Microsoft.Practices.ServiceLocation;
using ProjectX.Common.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading;
using ProjectX.Common.Extensions;

namespace BlueWind.Crawler.Manga.Site.Manga24h
{
    public class Manga24hSite : MangaSite
    {
        public string MangaListUri
        {
            get
            {
                return SiteUri + "/manga/status";
            }
        }

        public Manga24hSite()
            : base()
        {
            this.Name = "Manga24h";
            this.SiteUri = "http://manga24h.com";
            this.Language = "vi-VN";
            this.ChapterListQueryPath = "//div[contains(@class,'table_chapter')][1]//table[1]//tbody//tr";
            base.PaginationQueryPath = "//ul[@class='pagination'][1]//a";
            base.SeriesListQueryPath = "//div[contains(@class,'container')]/table/tbody//tr";
            base.Discriminator = "Manga24hSite";
        }
        public override bool GetSeries()
        {
            var crawlerParameter = ServiceLocator.Current.GetInstance<MangaCrawlParameter>();
            bool isSuccess = true;
            using (DbContext context = (DbContext)ServiceLocator.Current.GetInstance<SitesContext>())
            {
                if (!(!crawlerParameter.UseDb || context == null))
                    context.Set<MangaSite>().Attach(this);
                if (MangaSeries == null || MangaSeries.Count == 0)
                {
                    MangaSeries = new List<MangaSeries>();
                    HtmlDocument doc = new HtmlDocument();
                    int i = 0;
                    int max = 0;
                    if (crawlerParameter.Status == ProgressStatus.Unknown)
                    {
                        i = 1;
                        max = (int)ProgressStatus.Suspended + 1;
                    }
                    else
                    {
                        i = (int)crawlerParameter.Status;
                        max = (int)crawlerParameter.Status + 1;
                    }
                    for (; i < max; i++)
                    {
                        doc.LoadHtml(HttpUtility.GetResponseString(MangaListUri + "/" + i));
                        var query = doc.DocumentNode.SelectNodes(PaginationQueryPath);
                        if (query != null)
                        {
                            var node = query.LastOrDefault();
                            var hrefValue = node != null ? node.Attributes["href"].Value : "";
                            int totalPages = Int32.Parse(hrefValue.Substring(hrefValue.LastIndexOf('/') + 1));

                            for (int j = 1; j <= totalPages; j++)
                            {
                                MangaSeries series = null;
                                doc.LoadHtml(HttpUtility.GetResponseString(MangaListUri + "/" + i + "/page/" + j));
                                query = doc.DocumentNode.SelectNodes(SeriesListQueryPath);
                                foreach (var _node in query)
                                {
                                    node = _node.ChildNodes["a"];
                                    series = new Manga24hSeries(SiteUri + "/" + node.Attributes["href"].Value, node.InnerText, _node.ChildNodes[5].ChildNodes[1].InnerText, this, (ProgressStatus)i);
                                    series.HomeSite = this;
                                    if (context != null) context.Set<MangaSeries>().Add(series);
                                    MangaSeries.Add(series);
                                }
                            }
                            if (!(!crawlerParameter.UseDb || context == null))
                            {
                                if (context.SaveChanges()<1)
                                    isSuccess = false;
                            }
                        }
                    }
                }
            }
            return isSuccess;
        }

        public override void UpdateInfo()
        {
            foreach (var series in MangaSeries)
            {
                series.UpdateInfo();
            }
        }

        public override void UpdateSeries()
        {
            var crawlerParameter = ServiceLocator.Current.GetInstance<MangaCrawlParameter>();
            bool isSuccess = true;
            using (DbContext context = (DbContext)ServiceLocator.Current.GetInstance<SitesContext>())
            {
                if (!(!crawlerParameter.UseDb || context == null))
                    context.Set<MangaSite>().Attach(this);

                if (MangaSeries != null)
                {
                    HtmlDocument doc = new HtmlDocument();
                    int i = 0;
                    int max = 0;
                    if (crawlerParameter.Status == ProgressStatus.Unknown)
                    {
                        i = 1;
                        max = (int)ProgressStatus.Suspended + 1;
                    }
                    else
                    {
                        i = (int)crawlerParameter.Status;
                        max = (int)crawlerParameter.Status + 1;
                    }
                    for (; i < max; i++)
                    {
                        doc.LoadHtml(HttpUtility.GetResponseString(MangaListUri + "/" + i));
                        var query = doc.DocumentNode.SelectNodes(PaginationQueryPath);
                        if (query != null)
                        {
                            var node = query.LastOrDefault();
                            var hrefValue = node != null ? node.Attributes["href"].Value : "";
                            int totalPages = Int32.Parse(hrefValue.Substring(hrefValue.LastIndexOf('/') + 1));

                            for (int j = 1; j <= totalPages; j++)
                            {
                                MangaSeries series = null;
                                doc.LoadHtml(HttpUtility.GetResponseString(MangaListUri + "/" + i + "/page/" + j));
                                query = doc.DocumentNode.SelectNodes(SeriesListQueryPath);
                                foreach (var _node in query.Select(n => n.SelectNodes("td")))
                                {
                                    series = new Manga24hSeries(SiteUri + "/" + _node[0].ChildNodes["a"].Attributes["href"].Value,
                                        _node[0].ChildNodes["a"].FirstChild.InnerText.Replace("-", " ")
                                        , _node[2].InnerText,
                                        this,
                                        (ProgressStatus)i);
                                    series.HomeSite = this;
                                    if (!MangaSeries.Any(n => n.SiteUri == series.SiteUri))
                                        MangaSeries.Add(series);
                                }
                            }
                            if (!(!crawlerParameter.UseDb || context == null))
                            {
                                if (context.SaveChanges()<1)
                                    isSuccess = false;
                            }
                        }
                    }
                }
            }
        }

        public override void UpdateViewCount()
        {
            List<string> removeStrings = new List<string>(File.ReadAllLines("RemoveStrings.txt"));
            HtmlDocument doc = new HtmlDocument();
            var crawlerParameter = ServiceLocator.Current.GetInstance<MangaCrawlParameter>();
            using (DbContext context = (DbContext)ServiceLocator.Current.GetInstance<SitesContext>())
            {
                if (!(!crawlerParameter.UseDb || context == null))
                    context.Set<MangaSite>().Attach(this);
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
                    if (!(!crawlerParameter.UseDb || context == null))
                        context.SaveChanges();
                }
            }
        }
    }
}