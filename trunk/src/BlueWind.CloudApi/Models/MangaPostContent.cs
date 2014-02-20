using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlueWind.CloudApi.Models
{
    public class MangaPostContent
    {
        public int Site{get;set;}
        public int Series { get; set; }
        public int Chapter { get; set; }
        public string SearchValue { get; set; }
    }
}
