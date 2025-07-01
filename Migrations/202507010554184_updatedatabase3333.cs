namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedatabase3333 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VisitorEntries", "ArrivalReason", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.VisitorEntries", "ArrivalReason");
        }
    }
}
