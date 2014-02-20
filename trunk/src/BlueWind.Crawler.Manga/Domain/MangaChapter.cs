using BlueWind.Common;
using BlueWind.Crawler.Core;
using BlueWind.Crawler.Core.Interfaces;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.IO;

namespace BlueWind.Crawler.Manga.Domain
{
    public abstract class MangaChapter : IEntity
    {
        public MangaChapter()
        { }

        internal virtual MangaSeries HomeSeries { get; set; }

        public int Id
        {
            get;
            set;
        }

        [NotMapped]
        public string ChapterPath { get; set; }

        public bool IsScanFinished { get; set; }

        public string Name { get; set; }

        public virtual IList<ImageCache> ImageCaches { get; set; }

        public string SiteUri { get; set; }

        public string UpdatedDate { get; set; }

        public string Discriminator { get; set; }

        public int SourceViewCount { get; set; }

        public int ViewCount { get; set; }

        public abstract IEnumerable<ImageCache> GetPages();

        public bool Scan()
        {
            var crawlerParameter = ServiceLocator.Current.GetInstance<MangaCrawlParameter>();

            if (!crawlerParameter.UseDb)
                if (!Directory.Exists(ChapterPath))
                    Directory.CreateDirectory(ChapterPath);
            if (!IsScanFinished)
            {
                if (crawlerParameter.UseDb)
                {
                    using (var context = ServiceLocator.Current.GetInstance<SitesContext>())
                    {
                        string commandText = "";
                        foreach (var page in GetPages())
                        {
                            commandText += String.Format("insert into imagecaches(Url,HomeChapter_Id) values('{0}',{1});", page.Url, Id);

                        }
                        if (context.Database.ExecuteSqlCommand(commandText) > 0)
                        {
                            context.Database.ExecuteSqlCommand("Update MangaChapters Set IsScanFinished=1 where Id={0}", this.Id);
                            this.IsScanFinished = true;
                        }
                    }
                }
                else
                {
                    ImageCaches = new List<ImageCache>(GetPages());
                    if (crawlerParameter.IsDeepScan)
                        if (WriteChapters())
                            IsScanFinished = true;
                }
            }
            return IsScanFinished;
        }

        private bool WriteChapters()
        {
            var logPath = ChapterPath + Name + ".log";
            string path;
            int count = 0;
            for (int i = 0; i < ImageCaches.Count; i++)
            {
                path = ChapterPath + i.ToString("D4") + ".png";
                if (!File.Exists(path))
                {
                    while (!HttpUtility.GetResponseAndWriteFile(ImageCaches[i].Url, path))
                    {
                        Logger.Write("Writing file failed!!!");
                        count++;
                        if (count > 5) return false;
                    };
                    Logger.Write(path + " Writed");
                }
            }
            File.WriteAllBytes(logPath, new byte[] { (byte)WriteFileCode.Success });
            return true;
        }
    }
}