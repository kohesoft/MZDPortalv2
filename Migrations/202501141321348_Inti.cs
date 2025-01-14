namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Inti : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Tasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                        Progress = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.TodoItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        IsCompleted = c.Boolean(nullable: false),
                        TaskId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tasks", t => t.TaskId, cascadeDelete: true)
                .Index(t => t.TaskId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tasks", "UserId", "dbo.Users");
            DropForeignKey("dbo.TodoItems", "TaskId", "dbo.Tasks");
            DropIndex("dbo.TodoItems", new[] { "TaskId" });
            DropIndex("dbo.Tasks", new[] { "UserId" });
            DropTable("dbo.TodoItems");
            DropTable("dbo.Tasks");
        }
    }
}
