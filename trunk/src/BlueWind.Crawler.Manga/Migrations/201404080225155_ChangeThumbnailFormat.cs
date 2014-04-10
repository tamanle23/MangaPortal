namespace BlueWind.Crawler.Manga.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeThumbnailFormat : DbMigration
    {
        public override void Up()
        {
            this.DropColumn("MangaSeries", "ThumbnailBuffer");
            this.DropColumn("MangaSeries", "Thumbnail80");
            this.DropColumn("MangaSeries", "Thumbnail150");
            this.DropColumn("MangaSeries", "Thumbnail200");
            this.AddColumn("MangaSeries", "ThumbnailUrl", (builder) => builder.String(false));
        }
        
        public override void Down()
        {
            this.DropColumn("MangaSeries", "ThumbnailUrl");
            this.AddColumn("MangaSeries", "ThumbnailBuffer",(builder)=>builder.Binary());
            this.AddColumn("MangaSeries", "Thumbnail80", (builder) => builder.Binary());
            this.AddColumn("MangaSeries", "Thumbnail150", (builder) => builder.Binary());
            this.AddColumn("MangaSeries", "Thumbnail200", (builder) => builder.Binary());
        }
    }
}
