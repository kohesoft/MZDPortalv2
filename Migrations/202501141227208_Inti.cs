namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Inti : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Tasks", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Tasks", "AssignedById", "dbo.Users");
            DropForeignKey("dbo.Tasks", "AssignedToId", "dbo.Users");
            DropIndex("dbo.Tasks", new[] { "AssignedById" });
            DropIndex("dbo.Tasks", new[] { "AssignedToId" });
            DropIndex("dbo.Tasks", new[] { "User_Id" });
            DropTable("dbo.Tasks");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Tasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        DueDate = c.DateTime(nullable: false),
                        Priority = c.Int(nullable: false),
                        Status = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedDate = c.DateTime(nullable: false),
                        AssignedById = c.Int(nullable: false),
                        AssignedToId = c.Int(nullable: false),
                        User_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.Tasks", "User_Id");
            CreateIndex("dbo.Tasks", "AssignedToId");
            CreateIndex("dbo.Tasks", "AssignedById");
            AddForeignKey("dbo.Tasks", "AssignedToId", "dbo.Users", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Tasks", "AssignedById", "dbo.Users", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Tasks", "User_Id", "dbo.Users", "Id");
        }
    }
}
