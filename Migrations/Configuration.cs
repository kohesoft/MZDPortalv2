namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using MZDNETWORK.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<MZDNETWORKContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "ApplicationDbContext";
        }

        protected override void Seed(MZDNETWORKContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
        }
    }
}
