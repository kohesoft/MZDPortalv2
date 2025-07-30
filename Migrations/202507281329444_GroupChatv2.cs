namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GroupChatv2 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ChatMessages", new[] { "ChatId" });
            DropIndex("dbo.ChatMessages", new[] { "ChatGroup_Id" });
            RenameColumn(table: "dbo.ChatMessages", name: "ChatGroup_Id", newName: "ChatGroupId");
            RenameColumn(table: "dbo.ChatMessages", name: "ChatId", newName: "Chat_Id");
            AlterColumn("dbo.ChatMessages", "Chat_Id", c => c.Int());
            AlterColumn("dbo.ChatMessages", "ChatGroupId", c => c.Int(nullable: false));
            CreateIndex("dbo.ChatMessages", "ChatGroupId");
            CreateIndex("dbo.ChatMessages", "Chat_Id");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ChatMessages", new[] { "Chat_Id" });
            DropIndex("dbo.ChatMessages", new[] { "ChatGroupId" });
            AlterColumn("dbo.ChatMessages", "ChatGroupId", c => c.Int());
            AlterColumn("dbo.ChatMessages", "Chat_Id", c => c.Int(nullable: false));
            RenameColumn(table: "dbo.ChatMessages", name: "Chat_Id", newName: "ChatId");
            RenameColumn(table: "dbo.ChatMessages", name: "ChatGroupId", newName: "ChatGroup_Id");
            CreateIndex("dbo.ChatMessages", "ChatGroup_Id");
            CreateIndex("dbo.ChatMessages", "ChatId");
        }
    }
}
