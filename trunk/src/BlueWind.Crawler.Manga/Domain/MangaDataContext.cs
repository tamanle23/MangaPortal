using Microsoft.Practices.ServiceLocation;
using ProjectX.Common.Utility;
using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Data.SqlServerCe;

namespace BlueWind.Crawler.Manga.Domain
{
    public class MangaDataContext : DbContext
    {
        public DbSet<MangaChapter> Chapters { get; set; }

        public DbSet<ImageCache> ImageCaches { get; set; }

        public bool IsFirstInitialization { get; set; }

        public DbSet<MangaSeries> Series { get; set; }

        public DbSet<MangaSite> Sites { get; set; }

        public DbSet<VersionInfo> VersionInfos { get; set; }

        public class MangaDataContextInitializer : IDatabaseInitializer<MangaDataContext>
        {
            public Action<MangaDataContext> FirstInitializationCallback { get; set; }

            public void InitializeDatabase(MangaDataContext context)
            {
                if (!context.Database.Exists())
                {
                    Create(context);

                    //context.Database.ExecuteSqlCommand("Alter Table MangaSites Add Unique (Name)");
                    //context.Database.ExecuteSqlCommand("Alter Table MangaSeries Add Unique (Name)");
                    //context.Database.ExecuteSqlCommand("Alter Table MangaChapters Add Unique (Name)");
                    //context.Database.ExecuteSqlCommand("Alter Table ImageCaches Add Unique (Url) With (IGNORE_DUP_KEY  = ON)");
                    if (FirstInitializationCallback != null)
                        FirstInitializationCallback(context);
                }
                else
                {
                }
            }

            private static void Create(MangaDataContext context)
            {
                context.Database.Create();
            }
        }

        static MangaDataContext()
        {
            Database.SetInitializer(new MangaDataContextInitializer()
            {
                FirstInitializationCallback = (context) => { context.IsFirstInitialization = true; }
            });
        }

        public MangaDataContext()
            : base(GetConnection(),false)
        {
            //this.ObjContext().CommandTimeout = 10000;
        }

        private MangaDataContext(DbConnection connection)
            : base(connection, false)
        {
        }

        public static DbConnection GetConnection()
        {
            var connectionString = GetConnectionString();
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

        public static MangaDataContext MangaContext(string connectionString)
        {
            return new MangaDataContext(GetConnection());
        }

        public ObjectContext ObjContext()
        {
            return ((IObjectContextAdapter)this).ObjectContext;
        }

        public override int SaveChanges()
        {
            int result = 0;
            try
            {
                result = base.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.Write(ex);
            }
            finally
            {
                this.ObjContext().AcceptAllChanges();
            }
            return result;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MangaChapter>();
            
            modelBuilder.Entity<MangaSeries>().Property(p => p.Overview).HasColumnType("ntext").IsMaxLength();
            
            modelBuilder.Entity<MangaSeries>().Property(p => p.FullText).HasColumnType("ntext").IsMaxLength();
            modelBuilder.Entity<MangaSite>();
            modelBuilder.Entity<ImageCache>().Property(p => p.Buffer).HasColumnType("image").IsMaxLength();
            modelBuilder.Entity<ImageCache>().Property(p => p.Url).HasColumnType("ntext").IsMaxLength();
        }

        private static string GetConnectionString()
        {
            MangaCrawlParameter crawlerParameter;
            //try
            //{
                crawlerParameter = ServiceLocator.Current.GetInstance<MangaCrawlParameter>();
            //}
            //catch
            //{
            //    return "data source=.\\SQLEXPRESS;database=Manga;multipleactiveresultsets=True;integrated security=true";
            //}
            return crawlerParameter.ConnectionString;
        }
    }
}