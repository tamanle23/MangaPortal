using BlueWind.Crawler.Core.Interfaces;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;

namespace BlueWind.Crawler.Manga.Domain
{
    public abstract class MangaSite : IEntity
    {
        public MangaSite()
        {
        }

        public string ChapterListQueryPath { get; set; }

        public string Discriminator { get; set; }

        public int Id { get; set; }

        public bool IsScanFinished { get; set; }

        public string Language { get; set; }

        public long[] MangaSequenceIndentifier { get; set; }

        public virtual IList<MangaSeries> MangaSeries { get; set; }

        public string Name { get; set; }

        public string PaginationQueryPath { get; set; }

        public string SeriesListQueryPath { get; set; }

        public string SiteUri { get; set; }

        public abstract bool GetSeries();

        public void Scan()
        {
            var crawlerParameter = ServiceLocator.Current.GetInstance<MangaCrawlParameter>();
            if (!crawlerParameter.UseDb)
            {
                if (!Directory.Exists("Sites\\" + Name))
                    Directory.CreateDirectory("Sites\\" + Name);
            }
            if (GetSeries())
                if (ScanChildren())
                    this.IsScanFinished = true;
        }

        public abstract void UpdateInfo();

        public abstract void UpdateViewCount();

        public abstract void UpdateSeries();

        internal void UpdateChapters()
        {
            using (var context = ServiceLocator.Current.GetInstance<MangaDataContext>())
            {
                context.ObjContext().CommandTimeout = Int32.MaxValue;
                foreach (var series in context.Series.Where(series => series.HomeSite.Id == this.Id & series.Status == (byte)ProgressStatus.Ongoing))
                {
                    context.Entry(series).State = EntityState.Detached;
                    if (series.UpdateChapters())
                    {
                    }
                }
            }
        }

        private bool ScanChildren()
        {
            bool isSuccess = true;
            var crawlerParameter = ServiceLocator.Current.GetInstance<MangaCrawlParameter>();
            if (MangaSeries == null)
                if (crawlerParameter.UseDb)
                {
                    using (DbContext context = (DbContext)ServiceLocator.Current.GetInstance<MangaDataContext>())
                    {
                        context.Entry<MangaSite>(this).Collection<MangaSeries>(n => n.MangaSeries).Load();
                    }
                }
                else MangaSeries = new List<MangaSeries>();

            foreach (var series in MangaSeries)
            {
                if (!series.Scan())
                    isSuccess = false;
                series.MangaChapters.Clear();
            }
            return isSuccess;
        }
    }
}