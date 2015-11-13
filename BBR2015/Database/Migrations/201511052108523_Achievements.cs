namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Achievements : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Achievement",
                c => new
                    {
                        LagId = c.String(nullable: false, maxLength: 128),
                        AchievementType = c.String(nullable: false, maxLength: 128),
                        Score = c.Int(nullable: false),
                        Tidspunkt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.LagId, t.AchievementType })
                .ForeignKey("dbo.Lag", t => t.LagId, cascadeDelete: true)
                .Index(t => t.LagId);
            
            AddColumn("dbo.PostIMatch", "RiggetVåpen", c => c.String());
            AddColumn("dbo.PostIMatch", "RiggetVåpenParam", c => c.String());
            DropColumn("dbo.PostIMatch", "VåpenImplClass");
            DropColumn("dbo.PostIMatch", "VåpenParamJson");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PostIMatch", "VåpenParamJson", c => c.String());
            AddColumn("dbo.PostIMatch", "VåpenImplClass", c => c.String());
            DropForeignKey("dbo.Achievement", "LagId", "dbo.Lag");
            DropIndex("dbo.Achievement", new[] { "LagId" });
            DropColumn("dbo.PostIMatch", "RiggetVåpenParam");
            DropColumn("dbo.PostIMatch", "RiggetVåpen");
            DropTable("dbo.Achievement");
        }
    }
}
