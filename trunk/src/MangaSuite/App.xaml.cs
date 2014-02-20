using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;

namespace MangaDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
            : base()
        {
            AggregateCatalog = new AggregateCatalog();
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            this.AggregateCatalog.Catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*Site*"));
        }

        public AggregateCatalog AggregateCatalog { get; set; }
    }
}