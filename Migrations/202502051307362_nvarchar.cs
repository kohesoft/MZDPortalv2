namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nvarchar : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DilekOneris", "Mesaj", c => c.String());
            AlterColumn("dbo.DilekOneris", "Bilidirim", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DilekOneris", "Bilidirim", c => c.String(maxLength: 500));
            AlterColumn("dbo.DilekOneris", "Mesaj", c => c.String(maxLength: 500));
        }
    }
}
