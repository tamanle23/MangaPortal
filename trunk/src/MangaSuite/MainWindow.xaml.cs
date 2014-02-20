using MangaDownloader.Converter;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MangaDownloader
{
    public static class Converters
    {
        private static UriToBitmapImageConverter uriToBitmapImageConverter;
        private static BooleanToVisibilityConverter visibilityConverter;
        public static UriToBitmapImageConverter UriToBitmapImageConverter
        {
            get
            {
                if (uriToBitmapImageConverter == null) uriToBitmapImageConverter = new UriToBitmapImageConverter();
                return uriToBitmapImageConverter;
            }
        }
        public static BooleanToVisibilityConverter VisibilityConverter
        {
            get
            {
                if (visibilityConverter == null) visibilityConverter = new BooleanToVisibilityConverter();
                return visibilityConverter;
            }
        }
        public static string RemoveInvalidChars(this string str)
        {
            string invalidChars = "^/:*?<>\"|]*";
            var builder = new StringBuilder(str);
            foreach (char ch in invalidChars.ToArray())
            {
                builder.Replace(ch, ' ');
            }
            return builder.ToString();
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<string> loadingPages;
        // private ObservableCollection<MangaSeries> series;
        private ObservableCollection<string> seriesFolders;
        public ObservableCollection<string> SeriesFolders
        {
            get { return seriesFolders; }
            set { seriesFolders = value; RaisePropertyChanged("SeriesFolders"); }
        }
        private ObservableCollection<string> chapterPaths;
        public ObservableCollection<string> ChapterPaths
        {
            get { return chapterPaths; }

            set
            {
                chapterPaths = value;
                SelectedChapter = chapterPaths.FirstOrDefault(); RaisePropertyChanged("ChapterPaths");
            }
        }
        private string selectedFolder;
        public string SelectedFolder
        {
            get { return selectedFolder; }

            set
            {
                if (selectedFolder != value)
                {
                    selectedFolder = value;
                    RaisePropertyChanged("SelectedFolder");
                    if (SelectedFolder != "")
                        ThreadPool.QueueUserWorkItem(
                            (obj) =>
                            {
                                ChapterPaths = new ObservableCollection<string>(Directory.EnumerateDirectories(selectedFolder, "*", SearchOption.TopDirectoryOnly).OrderBy(SortChapter));
                            });
                }
            }
        }
        private string SortChapter(string arg)
        {
            var matches = new Regex(@"(([0-9])*[0-9])").Matches(arg);
            if (matches.Count < 1) return "";
            Match match = matches[0];
            return match.Value.PadLeft(3, '0');
        }
        private string selectedChapter;
        public string SelectedChapter
        {
            get { return selectedChapter; }

            set
            {
                if (selectedChapter != value)
                {
                    if (isRunning)
                        isRunning = false;

                    selectedChapter = value;
                    LoadingPages = new ObservableCollection<string>();
                    RaisePropertyChanged("SelectedChapter");
                    ContentViewer.ScrollToTop();
                    ThreadPool.QueueUserWorkItem((state) =>
                    {
                        isRunning = true;
                        if (selectedChapter != null)
                        {
                            if (SelectedChapter != "")
                                foreach (var item in Directory.EnumerateFiles(selectedChapter, "*.png", SearchOption.TopDirectoryOnly).Concat(Directory.EnumerateFiles(selectedChapter, "*.jpg", SearchOption.TopDirectoryOnly)))
                                {
                                    if (!isRunning)
                                    {
                                        break;
                                    }
                                    this.Dispatcher.Invoke(new Action(
                                        () =>
                                        {
                                            LoadingPages.Add(item);
                                        }),
                                        System.Windows.Threading.DispatcherPriority.Input);
                                }
                        }
                        isRunning = false;
                    });
                }
            }
        }
        private StateCommand startGetMangaCommand;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            //  SitesContext context = new SitesContext();
            // var tmp = context.Set<MangaSite>().Select(n => n.MangaSeries);
            SeriesFolders = new ObservableCollection<string>();
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var item in Directory.EnumerateDirectories(".\\", "*", SearchOption.TopDirectoryOnly))
                {
                    SeriesFolders.Add(item);
                }
                SelectedFolder = LastView.Default.Series;
                SelectedChapter = LastView.Default.Chapter;
                ContentViewer.ScrollToVerticalOffset(LastView.Default.Position);
            }));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private bool isRunning = true;
        public ObservableCollection<string> LoadingPages
        {
            get { return loadingPages; }
            set { loadingPages = value; RaisePropertyChanged("LoadingPages"); }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            LastView.Default.Series = SelectedFolder;
            LastView.Default.Chapter = SelectedChapter;
            LastView.Default.Position = ContentViewer.VerticalOffset;
            LastView.Default.Save();
            base.OnClosing(e);
        }
        // public ObservableCollection<MangaSite> Sites { get; set; }

        //public StateCommand StartGetMangaCommand { get { return new StateCommand(DeepScan); } }

        //private void DeepScan(StateCommand command)
        //{
        //    foreach (var site in Sites)
        //    {
        //        ThreadPool.QueueUserWorkItem(
        //            (obj) =>
        //            {
        //                command.State = CommandState.Running;
        //                site.Scan(true);
        //                command.State = CommandState.Normal;
        //            }, null);
        //    }
        //}
        protected void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int nextChapter = ChapterPaths.IndexOf(selectedChapter) + 1;
            if (nextChapter < ChapterPaths.Count)
                SelectedChapter = ChapterPaths[nextChapter];
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int previousChapter = ChapterPaths.IndexOf(selectedChapter) - 1;
            if (previousChapter > -1)
                SelectedChapter = ChapterPaths[previousChapter];
        }

        private void loa_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var uri=new Uri("www/SGP.html", UriKind.Relative);
                Stream source = Application.GetResourceStream(uri).Stream;
                Browser.NavigateToStream(source);
            }
            catch(Exception ex)
            {

            }
        }
    }
}