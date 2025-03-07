namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSurveyDuration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Surveys", "Duration", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Surveys", "Duration");
        }
    }
}
