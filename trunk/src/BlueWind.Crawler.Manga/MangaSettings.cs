using System.Collections.Generic;

namespace BlueWind.Crawler.Manga
{
    public class MangaSettings : Dictionary<string,object>
    {
        private static MangaSettings defaultInstance = new MangaSettings();

        public static MangaSettings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        public object this[string key]
        {
            get
            {
                return base.ContainsKey(key)?base[key]:null;
            }

            set
            {
                if (base.ContainsKey(key))
                    base[key] = value;
                else base.Add(key, value);
            }
        }
    }
}