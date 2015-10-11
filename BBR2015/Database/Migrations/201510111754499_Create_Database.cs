namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Create_Database : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Deltaker",
                c => new
                    {
                        DeltakerId = c.String(nullable: false, maxLength: 128),
                        Navn = c.String(),
                        Lag_LagId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.DeltakerId)
                .ForeignKey("dbo.Lag", t => t.Lag_LagId)
                .Index(t => t.Lag_LagId);
            
            CreateTable(
                "dbo.Lag",
                c => new
                    {
                        LagId = c.String(nullable: false, maxLength: 128),
                        Navn = c.String(),
                        Farge = c.String(),
                        Ikon = c.String(),
                    })
                .PrimaryKey(t => t.LagId);
            
            CreateTable(
                "dbo.DeltakerPosisjon",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DeltakerId = c.String(nullable: false),
                        LagId = c.String(nullable: false),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                        TidspunktUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LagIMatch",
                c => new
                    {
                        LagId = c.String(nullable: false, maxLength: 128),
                        MatchId = c.Guid(nullable: false),
                        PoengSum = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.LagId, t.MatchId })
                .ForeignKey("dbo.Lag", t => t.LagId, cascadeDelete: true)
                .ForeignKey("dbo.Match", t => t.MatchId, cascadeDelete: true)
                .Index(t => t.LagId)
                .Index(t => t.MatchId);
            
            CreateTable(
                "dbo.Match",
                c => new
                    {
                        MatchId = c.Guid(nullable: false),
                        Navn = c.String(),
                        StartUTC = c.DateTime(nullable: false),
                        SluttUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.MatchId);
            
            CreateTable(
                "dbo.PostIMatch",
                c => new
                    {
                        PostId = c.Guid(nullable: false),
                        MatchId = c.Guid(nullable: false),
                        CurrentPoengIndex = c.Int(nullable: false),
                        PoengArray = c.String(),
                        SynligFraUTC = c.DateTime(nullable: false),
                        SynligTilUTC = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.PostId, t.MatchId })
                .ForeignKey("dbo.Match", t => t.MatchId, cascadeDelete: true)
                .ForeignKey("dbo.Post", t => t.PostId, cascadeDelete: true)
                .Index(t => t.PostId)
                .Index(t => t.MatchId);
            
            CreateTable(
                "dbo.Post",
                c => new
                    {
                        PostId = c.Guid(nullable: false),
                        HemmeligKode = c.String(),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                        Altitude = c.Double(nullable: false),
                        DefaultPoengArray = c.String(),
                        Beskrivelse = c.String(),
                        Image = c.String(),
                        Omraade = c.String(),
                    })
                .PrimaryKey(t => t.PostId);
            
            CreateTable(
                "dbo.PostRegistrering",
                c => new
                    {
                        PostId = c.Guid(nullable: false),
                        MatchId = c.Guid(nullable: false),
                        Id = c.Int(nullable: false, identity: true),
                        PoengForRegistrering = c.Int(nullable: false),
                        BruktVaapenId = c.String(maxLength: 128),
                        RegistertForLag_LagId = c.String(nullable: false, maxLength: 128),
                        RegistertForLag_MatchId = c.Guid(nullable: false),
                        RegistrertAvDeltaker_DeltakerId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.PostId, t.MatchId })
                .ForeignKey("dbo.Vaapen", t => t.BruktVaapenId)
                .ForeignKey("dbo.LagIMatch", t => new { t.RegistertForLag_LagId, t.RegistertForLag_MatchId }, cascadeDelete: true)
                .ForeignKey("dbo.PostIMatch", t => new { t.PostId, t.MatchId })
                .ForeignKey("dbo.Deltaker", t => t.RegistrertAvDeltaker_DeltakerId, cascadeDelete: true)
                .Index(t => new { t.PostId, t.MatchId })
                .Index(t => t.BruktVaapenId)
                .Index(t => new { t.RegistertForLag_LagId, t.RegistertForLag_MatchId })
                .Index(t => t.RegistrertAvDeltaker_DeltakerId);
            
            CreateTable(
                "dbo.Vaapen",
                c => new
                    {
                        VaapenId = c.String(nullable: false, maxLength: 128),
                        Beskrivelse = c.String(),
                    })
                .PrimaryKey(t => t.VaapenId);
            
            CreateTable(
                "dbo.VaapenBeholdning",
                c => new
                    {
                        LagId = c.String(nullable: false, maxLength: 128),
                        MatchId = c.Guid(nullable: false),
                        VaapenId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LagId, t.MatchId, t.VaapenId })
                .ForeignKey("dbo.LagIMatch", t => new { t.LagId, t.MatchId }, cascadeDelete: true)
                .ForeignKey("dbo.Vaapen", t => t.VaapenId, cascadeDelete: true)
                .Index(t => new { t.LagId, t.MatchId })
                .Index(t => t.VaapenId);
            
            CreateTable(
                "dbo.Melding",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DeltakerId = c.String(nullable: false),
                        LagId = c.String(nullable: false),
                        SekvensId = c.Long(nullable: false),
                        TidspunktUTC = c.DateTime(nullable: false),
                        Tekst = c.String(maxLength: 256),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.VaapenBeholdning", "VaapenId", "dbo.Vaapen");
            DropForeignKey("dbo.VaapenBeholdning", new[] { "LagId", "MatchId" }, "dbo.LagIMatch");
            DropForeignKey("dbo.PostRegistrering", "RegistrertAvDeltaker_DeltakerId", "dbo.Deltaker");
            DropForeignKey("dbo.PostRegistrering", new[] { "PostId", "MatchId" }, "dbo.PostIMatch");
            DropForeignKey("dbo.PostRegistrering", new[] { "RegistertForLag_LagId", "RegistertForLag_MatchId" }, "dbo.LagIMatch");
            DropForeignKey("dbo.PostRegistrering", "BruktVaapenId", "dbo.Vaapen");
            DropForeignKey("dbo.LagIMatch", "MatchId", "dbo.Match");
            DropForeignKey("dbo.PostIMatch", "PostId", "dbo.Post");
            DropForeignKey("dbo.PostIMatch", "MatchId", "dbo.Match");
            DropForeignKey("dbo.LagIMatch", "LagId", "dbo.Lag");
            DropForeignKey("dbo.Deltaker", "Lag_LagId", "dbo.Lag");
            DropIndex("dbo.VaapenBeholdning", new[] { "VaapenId" });
            DropIndex("dbo.VaapenBeholdning", new[] { "LagId", "MatchId" });
            DropIndex("dbo.PostRegistrering", new[] { "RegistrertAvDeltaker_DeltakerId" });
            DropIndex("dbo.PostRegistrering", new[] { "RegistertForLag_LagId", "RegistertForLag_MatchId" });
            DropIndex("dbo.PostRegistrering", new[] { "BruktVaapenId" });
            DropIndex("dbo.PostRegistrering", new[] { "PostId", "MatchId" });
            DropIndex("dbo.PostIMatch", new[] { "MatchId" });
            DropIndex("dbo.PostIMatch", new[] { "PostId" });
            DropIndex("dbo.LagIMatch", new[] { "MatchId" });
            DropIndex("dbo.LagIMatch", new[] { "LagId" });
            DropIndex("dbo.Deltaker", new[] { "Lag_LagId" });
            DropTable("dbo.Melding");
            DropTable("dbo.VaapenBeholdning");
            DropTable("dbo.Vaapen");
            DropTable("dbo.PostRegistrering");
            DropTable("dbo.Post");
            DropTable("dbo.PostIMatch");
            DropTable("dbo.Match");
            DropTable("dbo.LagIMatch");
            DropTable("dbo.DeltakerPosisjon");
            DropTable("dbo.Lag");
            DropTable("dbo.Deltaker");
        }
    }
}
