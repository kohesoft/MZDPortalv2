namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Reservations", "StartTime", c => c.String());
            AddColumn("dbo.Reservations", "EndTime", c => c.String());
            DropColumn("dbo.Reservations", "Time");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Reservations", "Time", c => c.String());
            DropColumn("dbo.Reservations", "EndTime");
            DropColumn("dbo.Reservations", "StartTime");
        }
    }
}
