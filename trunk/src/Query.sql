/* Reset IsScanFinished flag  for incorrect ImageCache Url*/
Update MangaChapters
Set IsScanFinished=0
where Id in (
SELECT    distinct    HomeChapter_Id
FROM            ImageCaches
WHERE        (Url LIKE '%BlueWind.Crawler.Manga.ImageCache%'))

DELETE FROM ImageCaches
WHERE        (Url LIKE '%BlueWind.Crawler.Manga.ImageCache%')

Update MangaSeries 
Set IsScanFinished=0
where Id in (SELECT    distinct    HomeSeries_Id
FROM            MangaChapters
WHERE        (IsScanFinished=0))

/* Create Full Text Catalog, Index */
CREATE FULLTEXT CATALOG MangaFulltextCatalog;
GO

CREATE FULLTEXT INDEX 
  ON MangaSeries(FullText) 
  KEY INDEX [PK_dbo.MangaSeries] ON MangaFulltextCatalog; 
GO

SELECT * FROM MangaSeries WHERE fulltext(FullText,'hiep khach')

ALTER FULLTEXT INDEX ON [MangaSeries] START UPDATE POPULATION;

Select top 20 * from MangaSeries where FreeText(FullText,'hiep khach');

Select top 20 * from MangaSeries where CONTAINS(FullText,'hiep AND khach AND GiaNg AND hO');

Alter Table MangaSeries Add FullText ntext;

Update MangaSeries
Set Tag=Lower(Replace(Tag,'Tags :',''));

Update MangaSeries
Set FullText=Lower(dbo.RemoveVietnameseSign(Name)+' '+Tag);

Select Name,tag,fulltext
from mangaseries

DBCC CHECKIDENT ('ImageCaches', RESEED, 2980000);
DBCC CHECKIDENT ('MangaChapters', RESEED, 65895);


select max(Id) from ImageCaches
select max(Id) from MangaChapters