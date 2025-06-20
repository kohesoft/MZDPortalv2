namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Leaverequest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LeaveRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        LeaveType = c.Int(nullable: false),
                        Reason = c.String(nullable: false, maxLength: 500),
                        Description = c.String(maxLength: 1000),
                        Status = c.Int(nullable: false),
                        ApprovalReason = c.String(maxLength: 500),
                        ApprovedById = c.Int(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.ApprovedById)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ApprovedById);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LeaveRequests", "UserId", "dbo.Users");
            DropForeignKey("dbo.LeaveRequests", "ApprovedById", "dbo.Users");
            DropIndex("dbo.LeaveRequests", new[] { "ApprovedById" });
            DropIndex("dbo.LeaveRequests", new[] { "UserId" });
            DropTable("dbo.LeaveRequests");
        }
    }
}
