using BlueWind.Crawler.Core.Interfaces;
using System;

namespace BlueWind.Crawler.Manga.Domain
{
    public class VersionInfo : IEntity
    {
        public int Id
        {
            get;
            set;
        }
        public int Version { get; set; }
        public DateTime Date { get; set; }
    }
}