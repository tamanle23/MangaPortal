using BlueWind.Crawler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace BlueWind.Crawler.Manga.Domain
{
    public class ImageCache
    {
        [Key]
        public long Id
        {
            get;
            set;
        }
        public string Url { get; set; }
        public byte[] Buffer { get; set; }
        public ImageCache()
        {
        }
        public virtual MangaChapter HomeChapter { get; set; }
    }
    //public class ImageCacheComparer : IEqualityComparer<ImageCache>
    //{
    //    private static ImageCacheComparer comparer;

    //    public static ImageCacheComparer Comparer
    //    {
    //        get
    //        {
    //            if (comparer == null) comparer = new ImageCacheComparer();
    //            return ImageCacheComparer.comparer;
    //        }
    //        set { ImageCacheComparer.comparer = value; }
    //    }
    //    public bool Equals(ImageCache x, ImageCache y)
    //    {
    //        return x.Url == y.Url;
    //    }

    //    public int GetHashCode(ImageCache obj)
    //    {
    //        return obj.Url.GetHashCode();
    //    }
    //}
}
