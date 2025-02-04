namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addedusername : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Answers", "Username", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Answers", "Username");
        }
    }
}
