namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Inti : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Users", "AdditionalRole");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "AdditionalRole", c => c.String());
        }
    }
}
