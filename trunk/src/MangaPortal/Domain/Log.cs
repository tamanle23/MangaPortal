using BlueWind.Crawler.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MangaPortal.Domain
{
    public class Log:IEntity
    {
        public string Message { get; set; }
        public string DateTime { get; set; }
        public string LogLevel { get; set; }
        public string Exception { get; set; }
        public int Id
        {
            get;
            set;
        }
    }
}