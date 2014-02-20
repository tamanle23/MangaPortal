using BlueWind.Crawler.Manga;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlueWind.Crawler.Manga
{
    internal class MangaCrawlParameter
    {
        public BuiltInSites Site { get; set; }
        public ProgressStatus Status { get; set; }
        public bool IsScan { get; set; }
        public bool IsUpdateInfo { get; set; }
        public bool IsDeepScan { get; set; }
        public bool UseDb { get; set; }
        public bool IsNeedClean { get; set; }
        public bool IsUpdateViewCount { get; set; }
        public bool IsUpdateChapters { get; set;}

        public bool IsValid { get; set; }

        public bool IsUpdateSeries { get; set; }
    }
}
