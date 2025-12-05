namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMeetingRoomSystem : DbMigration
    {
        public override void Up()
        {
            // Tablolar zaten varsa oluşturmayı atla
            Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MeetingDecisions')
                BEGIN
                    CREATE TABLE [dbo].[MeetingDecisions] (
                        [Id] INT NOT NULL IDENTITY,
                        [ReservationId] INT NOT NULL,
                        [Title] NVARCHAR(200) NOT NULL,
                        [Description] NVARCHAR(MAX) NOT NULL,
                        [ResponsiblePerson] NVARCHAR(500),
                        [DueDate] DATETIME,
                        [Status] INT NOT NULL,
                        [OrderIndex] INT NOT NULL,
                        [CreatedByUserId] INT NOT NULL,
                        [CreatedByUserName] NVARCHAR(200),
                        [CreatedAt] DATETIME NOT NULL,
                        [UpdatedAt] DATETIME,
                        [CompletedAt] DATETIME,
                        [Notes] NVARCHAR(MAX),
                        CONSTRAINT [PK_dbo.MeetingDecisions] PRIMARY KEY ([Id])
                    );
                    CREATE INDEX [IX_ReservationId] ON [dbo].[MeetingDecisions]([ReservationId]);
                    ALTER TABLE [dbo].[MeetingDecisions] ADD CONSTRAINT [FK_dbo.MeetingDecisions_dbo.Reservations_ReservationId] 
                        FOREIGN KEY ([ReservationId]) REFERENCES [dbo].[Reservations] ([Id]) ON DELETE CASCADE;
                END
            ");

            Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ReservationAttendees')
                BEGIN
                    CREATE TABLE [dbo].[ReservationAttendees] (
                        [Id] INT NOT NULL IDENTITY,
                        [ReservationId] INT NOT NULL,
                        [UserId] INT NOT NULL,
                        [IsOptional] BIT NOT NULL,
                        [HasAccepted] BIT NOT NULL,
                        [HasDeclined] BIT NOT NULL,
                        [ResponseDate] DATETIME,
                        [CreatedAt] DATETIME NOT NULL,
                        CONSTRAINT [PK_dbo.ReservationAttendees] PRIMARY KEY ([Id])
                    );
                    CREATE INDEX [IX_ReservationId] ON [dbo].[ReservationAttendees]([ReservationId]);
                    CREATE INDEX [IX_UserId] ON [dbo].[ReservationAttendees]([UserId]);
                    ALTER TABLE [dbo].[ReservationAttendees] ADD CONSTRAINT [FK_dbo.ReservationAttendees_dbo.Reservations_ReservationId] 
                        FOREIGN KEY ([ReservationId]) REFERENCES [dbo].[Reservations] ([Id]) ON DELETE CASCADE;
                    ALTER TABLE [dbo].[ReservationAttendees] ADD CONSTRAINT [FK_dbo.ReservationAttendees_dbo.Users_UserId] 
                        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]);
                END
            ");
        }        public override void Down()
        {
            DropForeignKey("dbo.MeetingDecisions", "ReservationId", "dbo.Reservations");
            DropForeignKey("dbo.ReservationAttendees", "UserId", "dbo.Users");
            DropForeignKey("dbo.ReservationAttendees", "ReservationId", "dbo.Reservations");
            DropIndex("dbo.ReservationAttendees", new[] { "UserId" });
            DropIndex("dbo.ReservationAttendees", new[] { "ReservationId" });
            DropIndex("dbo.MeetingDecisions", new[] { "ReservationId" });
            DropTable("dbo.ReservationAttendees");
            DropTable("dbo.MeetingDecisions");
        }
    }
}
