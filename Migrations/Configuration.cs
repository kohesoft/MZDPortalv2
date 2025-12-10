namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<MZDNETWORK.Data.MZDNETWORKContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true; // MeetingReminderLog tablosu için gerekli
        }

        protected override void Seed(MZDNETWORK.Data.MZDNETWORKContext context)
        {
            //  This method will be called after migrating to the latest version.

            // Toplantı odaları seed data
            context.MeetingRooms.AddOrUpdate(
                r => r.Name,
                new Models.MeetingRoom
                {
                    Name = "Yerleşke Büyük Toplantı Salonu",
                    Location = "Yerleşke",
                    Capacity = 15,
                    Features = "Projeksiyon, Beyaz Tahta, Video Konferans",
                    ColorCode = "purple",
                    IsActive = true,
                    OrderIndex = 1,
                    CreatedAt = DateTime.Now
                },
                new Models.MeetingRoom
                {
                    Name = "Merkez Toplantı Salonu",
                    Location = "Merkez",
                    Capacity = 10,
                    Features = "Projeksiyon, Beyaz Tahta",
                    ColorCode = "green",
                    IsActive = true,
                    OrderIndex = 3,
                    CreatedAt = DateTime.Now
                }
            );
        }
    }
}
