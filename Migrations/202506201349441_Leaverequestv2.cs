namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Leaverequestv2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LeaveRequests", "ContactInfo", c => c.String(maxLength: 200));
            AddColumn("dbo.LeaveRequests", "Department", c => c.String(maxLength: 100));
            AddColumn("dbo.LeaveRequests", "SubstituteName", c => c.String(maxLength: 200));
            AddColumn("dbo.LeaveRequests", "Tasks", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LeaveRequests", "Tasks");
            DropColumn("dbo.LeaveRequests", "SubstituteName");
            DropColumn("dbo.LeaveRequests", "Department");
            DropColumn("dbo.LeaveRequests", "ContactInfo");
        }
    }
}
