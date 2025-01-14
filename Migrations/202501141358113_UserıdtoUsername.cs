namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserıdtoUsername : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Tasks", "UserId", "dbo.Users");
            DropIndex("dbo.Tasks", new[] { "UserId" });
            RenameColumn(table: "dbo.Tasks", name: "UserId", newName: "User_Id");
            AddColumn("dbo.Tasks", "Username", c => c.String());
            AlterColumn("dbo.Tasks", "User_Id", c => c.Int());
            CreateIndex("dbo.Tasks", "User_Id");
            AddForeignKey("dbo.Tasks", "User_Id", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tasks", "User_Id", "dbo.Users");
            DropIndex("dbo.Tasks", new[] { "User_Id" });
            AlterColumn("dbo.Tasks", "User_Id", c => c.Int(nullable: false));
            DropColumn("dbo.Tasks", "Username");
            RenameColumn(table: "dbo.Tasks", name: "User_Id", newName: "UserId");
            CreateIndex("dbo.Tasks", "UserId");
            AddForeignKey("dbo.Tasks", "UserId", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
