namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FraUtcTilLokaltid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DeltakerPosisjon", "Tidspunkt", c => c.DateTime(nullable: false));
            AddColumn("dbo.Match", "StartTid", c => c.DateTime(nullable: false));
            AddColumn("dbo.Match", "SluttTid", c => c.DateTime(nullable: false));
            AddColumn("dbo.PostIMatch", "SynligFraTid", c => c.DateTime(nullable: false));
            AddColumn("dbo.PostIMatch", "SynligTilTid", c => c.DateTime(nullable: false));
            AddColumn("dbo.PostIMatch", "VåpenImplClass", c => c.String());
            AddColumn("dbo.PostIMatch", "VåpenParamJson", c => c.String());
            AddColumn("dbo.Melding", "Tidspunkt", c => c.DateTime(nullable: false));
            DropColumn("dbo.DeltakerPosisjon", "TidspunktUTC");
            DropColumn("dbo.Match", "StartUTC");
            DropColumn("dbo.Match", "SluttUTC");
            DropColumn("dbo.PostIMatch", "SynligFraUTC");
            DropColumn("dbo.PostIMatch", "SynligTilUTC");
            DropColumn("dbo.Melding", "TidspunktUTC");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Melding", "TidspunktUTC", c => c.DateTime(nullable: false));
            AddColumn("dbo.PostIMatch", "SynligTilUTC", c => c.DateTime(nullable: false));
            AddColumn("dbo.PostIMatch", "SynligFraUTC", c => c.DateTime(nullable: false));
            AddColumn("dbo.Match", "SluttUTC", c => c.DateTime(nullable: false));
            AddColumn("dbo.Match", "StartUTC", c => c.DateTime(nullable: false));
            AddColumn("dbo.DeltakerPosisjon", "TidspunktUTC", c => c.DateTime(nullable: false));
            DropColumn("dbo.Melding", "Tidspunkt");
            DropColumn("dbo.PostIMatch", "VåpenParamJson");
            DropColumn("dbo.PostIMatch", "VåpenImplClass");
            DropColumn("dbo.PostIMatch", "SynligTilTid");
            DropColumn("dbo.PostIMatch", "SynligFraTid");
            DropColumn("dbo.Match", "SluttTid");
            DropColumn("dbo.Match", "StartTid");
            DropColumn("dbo.DeltakerPosisjon", "Tidspunkt");
        }
    }
}
