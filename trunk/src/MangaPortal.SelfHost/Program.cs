using BlueWind.Crawler.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Web.Http.SelfHost;


namespace MangaPortal.SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Initialize(true);
            Console.WriteLine("Press Enter to quit.");
            Console.ReadLine();
        }

    }
}
