namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Intialcreate232 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BeyazTahtaEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedAt = c.DateTime(nullable: false),
                        OneriVeren = c.String(),
                        Problem = c.String(),
                        Oneri = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropTable("dbo.BeyazTahtaGonderis");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.BeyazTahtaGonderis",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Content = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropTable("dbo.BeyazTahtaEntries");
        }
    }
}
