namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Deltaker_Kode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Deltaker", "Kode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Deltaker", "Kode");
        }
    }
}
