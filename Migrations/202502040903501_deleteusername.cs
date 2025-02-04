namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deleteusername : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Answers", "Username");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Answers", "Username", c => c.String());
        }
    }
}
