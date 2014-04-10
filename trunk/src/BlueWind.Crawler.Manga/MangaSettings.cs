using ProjectX.Common.Utility;

namespace BlueWind.Crawler.Manga
{
    public class MangaSettings : Settings
    {
        private static MangaSettings defaultInstance = new MangaSettings();

        public static MangaSettings Default
        {
            get
            {
                return defaultInstance;
            }
        }
        
        public string ConnectionString
        {
            get
            {
                return (string)this["ConnectionString"];
            }
            set {
                this["ConnectionString"] = value;            
            }
        }
    }
}