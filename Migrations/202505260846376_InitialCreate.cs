namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Reservations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        UserName = c.String(),
                        Room = c.String(),
                        Date = c.DateTime(nullable: false),
                        Time = c.String(),
                        Title = c.String(),
                        Description = c.String(),
                        Attendees = c.String(),
                        Status = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ApprovedAt = c.DateTime(),
                        RejectedAt = c.DateTime(),
                        RejectionReason = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Reservations");
        }
    }
}
