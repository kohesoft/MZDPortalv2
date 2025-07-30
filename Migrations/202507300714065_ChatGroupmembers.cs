namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChatGroupmembers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChatGroupMembers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChatGroupId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        CanWrite = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ChatGroups", t => t.ChatGroupId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.ChatGroupId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatGroupMembers", "UserId", "dbo.Users");
            DropForeignKey("dbo.ChatGroupMembers", "ChatGroupId", "dbo.ChatGroups");
            DropIndex("dbo.ChatGroupMembers", new[] { "UserId" });
            DropIndex("dbo.ChatGroupMembers", new[] { "ChatGroupId" });
            DropTable("dbo.ChatGroupMembers");
        }
    }
}
