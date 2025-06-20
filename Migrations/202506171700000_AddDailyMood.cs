namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddDailyMood : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DailyMoods",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Mood = c.Int(nullable: false),
                        Comment = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: false)
                .Index(t => new { t.UserId, t.Date }, unique: true, name: "IX_User_Date");
        }

        public override void Down()
        {
            DropForeignKey("dbo.DailyMoods", "UserId", "dbo.Users");
            DropIndex("dbo.DailyMoods", "IX_User_Date");
            DropTable("dbo.DailyMoods");
        }
    }
} 