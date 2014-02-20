<Query Kind="Program">
  <Reference>D:\bin\MangaSuite\build\BlueWind.CloudApi.dll</Reference>
  <Reference>D:\bin\MangaSuite\build\BlueWind.Crawler.Core.dll</Reference>
  <Reference>D:\bin\MangaSuite\build\BlueWind.Crawler.Manga.dll</Reference>
  <Reference>D:\bin\MangaSuite\build\EntityFramework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.DataSetExtensions.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.Entity.Design.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.Entity.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Data.Linq.dll</Reference>
  <Namespace>BlueWind.Crawler.Manga</Namespace>
  <Namespace>BlueWind.Crawler.Manga.Domain</Namespace>
  <Namespace>BlueWind.Crawler.Manga.Site.Manga24h</Namespace>
  <Namespace>System.Data.Entity</Namespace>
</Query>

void Main()
{
	var connectionString="data source=bluewind.cuvuuohvwkjt.us-west-2.rds.amazonaws.com;database=Manga;multipleactiveresultsets=True;user id=koneta_rw;password=23659841756632145";
	var context= SitesContext.MangaContext(connectionString);
	/*var keywords="echi";
	 keywords = keywords.Replace("'"," ").Replace("_", " AND ");
     var results = context.ObjContext().ExecuteStoreQuery<int>(String.Format("Select top 20 Id from MangaSeries where CONTAINS(FullText,'{0}')", keywords));
                if (results != null)
                {
                 var returnValue= from series in  context.Series
								 where results.Contains(series.Id)
								 select new {
								 Id=series.Id,
								 Name=series.Name,
								 Overview=series.Overview,
								 Thumbnail=series.Thumbnail80,
								 NumberOfChapters = series.MangaChapters.Count()
								 };
				  foreach(var item in returnValue)
				  Console.WriteLine(item.Id+" "+item.Name+" "+item.NumberOfChapters);
                }
				*/
				var returnValue = context.Series.OrderByDescending(n => n.ViewCount + n.SourceViewCount).Skip(80).Take(10).Select(
                    series =>
                    new
                            {
                                Id = series.Id,
                                Name = series.Name,
                                Thumbnail = series.Thumbnail80,
                                NumberOfChapters = series.MangaChapters.Count(),
                            });
     
	  foreach(var item in returnValue)
				  Console.WriteLine(item.Id+" "+item.Name+" "+item.NumberOfChapters);
}

// Define other methods and classes here