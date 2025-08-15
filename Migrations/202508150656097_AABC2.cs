namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AABC2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OvertimeServicePersonnels", "DeletedDate", c => c.DateTime());
            AddColumn("dbo.OvertimeServicePersonnels", "DeletedBy", c => c.String());
            AddColumn("dbo.OvertimeServicePersonnels", "DeletedReason", c => c.String());
            AddColumn("dbo.ServiceConfigurations", "DeletedDate", c => c.DateTime());
            AddColumn("dbo.ServiceConfigurations", "DeletedBy", c => c.String());
            AddColumn("dbo.ServiceConfigurations", "DeletedReason", c => c.String());
            AddColumn("dbo.ServicePersonnels", "DeletedDate", c => c.DateTime());
            AddColumn("dbo.ServicePersonnels", "DeletedBy", c => c.String());
            AddColumn("dbo.ServicePersonnels", "DeletedReason", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ServicePersonnels", "DeletedReason");
            DropColumn("dbo.ServicePersonnels", "DeletedBy");
            DropColumn("dbo.ServicePersonnels", "DeletedDate");
            DropColumn("dbo.ServiceConfigurations", "DeletedReason");
            DropColumn("dbo.ServiceConfigurations", "DeletedBy");
            DropColumn("dbo.ServiceConfigurations", "DeletedDate");
            DropColumn("dbo.OvertimeServicePersonnels", "DeletedReason");
            DropColumn("dbo.OvertimeServicePersonnels", "DeletedBy");
            DropColumn("dbo.OvertimeServicePersonnels", "DeletedDate");
        }
    }
}
