using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
namespace BlueWind.Crawler.Manga.Domain
{
    public class SitesContext : DbContext
    {
        static SitesContext()
        {
            Database.SetInitializer(new SitesContextInitializer()
            {
                FirstInitializationCallback = (context) => { context.IsFirstInitialization = true; }
            });
        }
        public class SitesContextInitializer : IDatabaseInitializer<SitesContext>
        {
            public void InitializeDatabase(SitesContext context)
            {
                if (!context.Database.Exists())
                {
                    Create(context);
                    //context.Database.ExecuteSqlCommand("Alter Table MangaSites Add Unique (Name)");
                    //context.Database.ExecuteSqlCommand("Alter Table MangaSeries Add Unique (Name)");
                    //context.Database.ExecuteSqlCommand("Alter Table MangaChapters Add Unique (Name)");
                    //context.Database.ExecuteSqlCommand("Alter Table ImageCaches Add Unique (Url) With (IGNORE_DUP_KEY  = ON)");
                    if (FirstInitializationCallback != null) FirstInitializationCallback(context);
                }
                else
                {
                }
            }
            public Action<SitesContext> FirstInitializationCallback{get;set;}
            private static void Create(SitesContext context)
            {
                context.Database.Create();
            }
        }
        public static SitesContext MangaContext(string connectionString)
        {
            return new SitesContext(GetConnection());
        }
        public static DbConnection GetConnection()
        {
            var connectionString = (String)MangaSettings.Default["ConnectionString"];
            DbConnection connection = null;
            if (connectionString.Contains(".sdf"))
            {
                connection = new SqlCeConnection(connectionString);
            }
            else
            {
                connection = new SqlConnection(connectionString);
            }
            return connection;
        }
        public SitesContext():base((String)MangaSettings.Default["ConnectionString"])
        {

        }
        private SitesContext(DbConnection connection)
            : base(connection,false)
        {
        }
        public bool IsFirstInitialization { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MangaChapter>();
            modelBuilder.Entity<MangaSeries>().Property(p => p.ThumbnailBuffer).HasColumnType("image").IsMaxLength();
            modelBuilder.Entity<MangaSeries>().Property(p => p.Overview).HasColumnType("ntext").IsMaxLength();
            modelBuilder.Entity<MangaSeries>().Property(p => p.Thumbnail80).HasColumnType("ntext").IsMaxLength();
            modelBuilder.Entity<MangaSeries>().Property(p => p.Thumbnail150).HasColumnType("ntext").IsMaxLength();
            modelBuilder.Entity<MangaSeries>().Property(p => p.Thumbnail200).HasColumnType("ntext").IsMaxLength();
            modelBuilder.Entity<MangaSeries>().Property(p => p.FullText).HasColumnType("ntext").IsMaxLength();
            modelBuilder.Entity<MangaSite>();
            modelBuilder.Entity<ImageCache>().Property(p => p.Buffer).HasColumnType("image").IsMaxLength();
            modelBuilder.Entity<ImageCache>().Property(p => p.Url).HasColumnType("ntext").IsMaxLength();
        }
        public DbSet<MangaSite> Sites { get; set; }
        public DbSet<MangaSeries> Series { get; set; }
        public DbSet<MangaChapter> Chapters { get; set; }
        public DbSet<ImageCache> ImageCaches { get; set; }
        public DbSet<VersionInfo> VersionInfos { get; set; }

    }
}