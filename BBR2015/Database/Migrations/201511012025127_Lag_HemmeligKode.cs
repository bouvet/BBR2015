namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Lag_HemmeligKode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Lag", "HemmeligKode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Lag", "HemmeligKode");
        }
    }
}
