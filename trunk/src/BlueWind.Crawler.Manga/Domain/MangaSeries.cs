using BlueWind.Crawler.Core;
using BlueWind.Crawler.Core.Interfaces;
using Microsoft.Practices.ServiceLocation;
using ProjectX.Common.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace BlueWind.Crawler.Manga.Domain
{
    public abstract class MangaSeries : IEntity
    {
        public string Author { get; set; }

        public string Discriminator { get; set; }

        public string FullText { get; set; }

        public int Id
        {
            get;
            set;
        }

        public bool IsScanFinished { get; set; }

        public string LastUpdatedSource { get; set; }

        [NotMapped]
        public string LogFilePath { get { return SeriesFolderPath + Name + ".log"; } }

        public virtual IList<MangaChapter> MangaChapters { get; set; }

        public string Name { get; set; }

        public string Overview { get; set; }

        [NotMapped]
        public string SeriesFolderPath { get; set; }

        public string SiteUri { get; set; }

        public string ThumbnailUrl { get; set; }

        public int SourceViewCount { get; set; }

        public byte Status
        {
            get;
            set;
        }

        public string Tag { get; set; }

        public int ViewCount { get; set; }

        public virtual MangaSite HomeSite { get; set; }

        public MangaSeries()
        {
        }

        public abstract bool CheckDate(string last, string current);

        public abstract bool GetChapters();

        public abstract DateTime GetDate(string dateString);

        public bool Scan()
        {
            var crawlerParameter = ServiceLocator.Current.GetInstance<MangaCrawlParameter>();
            if (!crawlerParameter.UseDb)
                if (!Directory.Exists(SeriesFolderPath))
                    Directory.CreateDirectory(SeriesFolderPath);
            if (crawlerParameter.UseDb)
            {
                if (!IsScanFinished)
                {
                    if (crawlerParameter.IsDeepScan)
                    {
                        if (GetChapters())
                            if (ScanChildren())
                                if ((Status == (byte)ProgressStatus.Completed || Status == (byte)ProgressStatus.Suspended))
                                    using (DbContext context = (DbContext)ServiceLocator.Current.GetInstance<MangaDataContext>())
                                    {
                                        IsScanFinished = true;
                                        context.Database.ExecuteSqlCommand("Update MangaSeries Set IsScanFinished=1 where id={0}", this.Id);
                                    }
                    }
                }
            }
            else
            {
                if (File.Exists(LogFilePath))
                {
                    var buffer = File.ReadAllBytes(LogFilePath);
                    if (buffer.Length > 0)
                        if (buffer[0] == (byte)WriteFileCode.Success)
                            crawlerParameter.IsDeepScan = false;
                }
                if (crawlerParameter.IsDeepScan)
                {
                    if (GetChapters())
                    {
                        ScanChildren();
                        if (Status == (byte)ProgressStatus.Completed || Status == (byte)ProgressStatus.Suspended)
                        {
                            File.WriteAllBytes(LogFilePath, new byte[] { (byte)WriteFileCode.Success, Status });
                        }
                        else
                        {
                            File.WriteAllBytes(LogFilePath, new byte[] { (byte)WriteFileCode.Fail, Status });
                        }
                    }
                }
                this.IsScanFinished = true;
            }
            if (this.IsScanFinished) Logger.Write(string.Format("{0}: {1} finished.", Id, Name));
            return IsScanFinished;
        }

        public abstract bool UpdateChapters();

        public abstract void UpdateInfo();

        public abstract void UpdateViewCount(Func<object, bool> Callback);

        private bool ScanChildren()
        {
            bool isSuccess = true;
            var crawlerParameter = ServiceLocator.Current.GetInstance<MangaCrawlParameter>();
            if (MangaChapters == null)
                if (crawlerParameter.UseDb)
                {
                    using (DbContext context = (DbContext)ServiceLocator.Current.GetInstance<MangaDataContext>())
                    {
                        context.Entry<MangaSeries>(this).Collection<MangaChapter>(n => n.MangaChapters).Load();
                    }
                }
                else MangaChapters = new List<MangaChapter>();

            foreach (var chapter in MangaChapters)
            {
                if (!chapter.Scan())
                    isSuccess = false;
                chapter.ImageCaches.Clear();
            }
            return isSuccess;
        }
    }
}