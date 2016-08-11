namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MatchGeobox : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Match", "GeoboxNWLatitude", c => c.Double());
            AddColumn("dbo.Match", "GeoboxNWLongitude", c => c.Double());
            AddColumn("dbo.Match", "GeoboxSELatitude", c => c.Double());
            AddColumn("dbo.Match", "GeoboxSELongitude", c => c.Double());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Match", "GeoboxSELongitude");
            DropColumn("dbo.Match", "GeoboxSELatitude");
            DropColumn("dbo.Match", "GeoboxNWLongitude");
            DropColumn("dbo.Match", "GeoboxNWLatitude");
        }
    }
}
