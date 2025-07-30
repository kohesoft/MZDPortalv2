namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GroupChat : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChatGroups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        CreatedBy = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.CreatedBy, cascadeDelete: false)
                .Index(t => t.CreatedBy);
            
            CreateTable(
                "dbo.ChatGroupRoles",
                c => new
                    {
                        ChatGroup_Id = c.Int(nullable: false),
                        Role_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ChatGroup_Id, t.Role_Id })
                .ForeignKey("dbo.ChatGroups", t => t.ChatGroup_Id, cascadeDelete: true)
                .ForeignKey("dbo.Roles", t => t.Role_Id, cascadeDelete: false)
                .Index(t => t.ChatGroup_Id)
                .Index(t => t.Role_Id);
            
            CreateTable(
                "dbo.ChatGroupUsers",
                c => new
                    {
                        ChatGroup_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ChatGroup_Id, t.User_Id })
                .ForeignKey("dbo.ChatGroups", t => t.ChatGroup_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: false)
                .Index(t => t.ChatGroup_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.ChatGroupUser1",
                c => new
                    {
                        ChatGroup_Id = c.Int(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ChatGroup_Id, t.User_Id })
                .ForeignKey("dbo.ChatGroups", t => t.ChatGroup_Id, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: false)
                .Index(t => t.ChatGroup_Id)
                .Index(t => t.User_Id);
            
            AddColumn("dbo.ChatMessages", "ChatGroup_Id", c => c.Int());
            CreateIndex("dbo.ChatMessages", "ChatGroup_Id");
            AddForeignKey("dbo.ChatMessages", "ChatGroup_Id", "dbo.ChatGroups", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatMessages", "ChatGroup_Id", "dbo.ChatGroups");
            DropForeignKey("dbo.ChatGroupUser1", "User_Id", "dbo.Users");
            DropForeignKey("dbo.ChatGroupUser1", "ChatGroup_Id", "dbo.ChatGroups");
            DropForeignKey("dbo.ChatGroupUsers", "User_Id", "dbo.Users");
            DropForeignKey("dbo.ChatGroupUsers", "ChatGroup_Id", "dbo.ChatGroups");
            DropForeignKey("dbo.ChatGroups", "CreatedBy", "dbo.Users");
            DropForeignKey("dbo.ChatGroupRoles", "Role_Id", "dbo.Roles");
            DropForeignKey("dbo.ChatGroupRoles", "ChatGroup_Id", "dbo.ChatGroups");
            DropIndex("dbo.ChatGroupUser1", new[] { "User_Id" });
            DropIndex("dbo.ChatGroupUser1", new[] { "ChatGroup_Id" });
            DropIndex("dbo.ChatGroupUsers", new[] { "User_Id" });
            DropIndex("dbo.ChatGroupUsers", new[] { "ChatGroup_Id" });
            DropIndex("dbo.ChatGroupRoles", new[] { "Role_Id" });
            DropIndex("dbo.ChatGroupRoles", new[] { "ChatGroup_Id" });
            DropIndex("dbo.ChatMessages", new[] { "ChatGroup_Id" });
            DropIndex("dbo.ChatGroups", new[] { "CreatedBy" });
            DropColumn("dbo.ChatMessages", "ChatGroup_Id");
            DropTable("dbo.ChatGroupUser1");
            DropTable("dbo.ChatGroupUsers");
            DropTable("dbo.ChatGroupRoles");
            DropTable("dbo.ChatGroups");
        }
    }
}
