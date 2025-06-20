namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ajhbhfjhb : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.DynamicPermissions");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.DynamicPermissions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoleName = c.String(maxLength: 128),
                        ControllerName = c.String(nullable: false, maxLength: 128),
                        ActionName = c.String(maxLength: 128),
                        ViewPath = c.String(maxLength: 256),
                        AllowAnonymous = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
    }
}
