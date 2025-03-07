namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addendate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Surveys", "EndDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Surveys", "EndDate");
        }
    }
}
