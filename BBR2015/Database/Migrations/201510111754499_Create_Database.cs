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
                        Id = c.Int(nullable: false, identity: true),
                        PoengSum = c.Int(nullable: false),
                        Lag_LagId = c.String(nullable: false, maxLength: 128),
                        Match_MatchId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lag", t => t.Lag_LagId, cascadeDelete: true)
                .ForeignKey("dbo.Match", t => t.Match_MatchId, cascadeDelete: true)
                .Index(t => t.Lag_LagId)
                .Index(t => t.Match_MatchId);
            
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
                        Id = c.Int(nullable: false, identity: true),
                        CurrentPoengIndex = c.Int(nullable: false),
                        PoengArray = c.String(),
                        SynligFraUTC = c.DateTime(nullable: false),
                        SynligTilUTC = c.DateTime(nullable: false),
                        Match_MatchId = c.Guid(nullable: false),
                        Post_PostId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Match", t => t.Match_MatchId, cascadeDelete: true)
                .ForeignKey("dbo.Post", t => t.Post_PostId, cascadeDelete: true)
                .Index(t => t.Match_MatchId)
                .Index(t => t.Post_PostId);
            
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
                        Id = c.Int(nullable: false, identity: true),
                        PoengForRegistrering = c.Int(nullable: false),
                        BruktVaapenId = c.String(maxLength: 128),
                        RegistertForLag_Id = c.Int(),
                        RegistertPost_Id = c.Int(),
                        RegistrertAvDeltaker_DeltakerId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Vaapen", t => t.BruktVaapenId)
                .ForeignKey("dbo.LagIMatch", t => t.RegistertForLag_Id)
                .ForeignKey("dbo.PostIMatch", t => t.RegistertPost_Id)
                .ForeignKey("dbo.Deltaker", t => t.RegistrertAvDeltaker_DeltakerId)
                .Index(t => t.BruktVaapenId)
                .Index(t => t.RegistertForLag_Id)
                .Index(t => t.RegistertPost_Id)
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
                        Id = c.Int(nullable: false, identity: true),
                        LagIMatchId = c.Int(nullable: false),
                        VaapenId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LagIMatch", t => t.LagIMatchId, cascadeDelete: true)
                .ForeignKey("dbo.Vaapen", t => t.VaapenId, cascadeDelete: true)
                .Index(t => t.LagIMatchId)
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
            DropForeignKey("dbo.VaapenBeholdning", "LagIMatchId", "dbo.LagIMatch");
            DropForeignKey("dbo.PostRegistrering", "RegistrertAvDeltaker_DeltakerId", "dbo.Deltaker");
            DropForeignKey("dbo.PostRegistrering", "RegistertPost_Id", "dbo.PostIMatch");
            DropForeignKey("dbo.PostRegistrering", "RegistertForLag_Id", "dbo.LagIMatch");
            DropForeignKey("dbo.PostRegistrering", "BruktVaapenId", "dbo.Vaapen");
            DropForeignKey("dbo.LagIMatch", "Match_MatchId", "dbo.Match");
            DropForeignKey("dbo.PostIMatch", "Post_PostId", "dbo.Post");
            DropForeignKey("dbo.PostIMatch", "Match_MatchId", "dbo.Match");
            DropForeignKey("dbo.LagIMatch", "Lag_LagId", "dbo.Lag");
            DropForeignKey("dbo.Deltaker", "Lag_LagId", "dbo.Lag");
            DropIndex("dbo.VaapenBeholdning", new[] { "VaapenId" });
            DropIndex("dbo.VaapenBeholdning", new[] { "LagIMatchId" });
            DropIndex("dbo.PostRegistrering", new[] { "RegistrertAvDeltaker_DeltakerId" });
            DropIndex("dbo.PostRegistrering", new[] { "RegistertPost_Id" });
            DropIndex("dbo.PostRegistrering", new[] { "RegistertForLag_Id" });
            DropIndex("dbo.PostRegistrering", new[] { "BruktVaapenId" });
            DropIndex("dbo.PostIMatch", new[] { "Post_PostId" });
            DropIndex("dbo.PostIMatch", new[] { "Match_MatchId" });
            DropIndex("dbo.LagIMatch", new[] { "Match_MatchId" });
            DropIndex("dbo.LagIMatch", new[] { "Lag_LagId" });
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
