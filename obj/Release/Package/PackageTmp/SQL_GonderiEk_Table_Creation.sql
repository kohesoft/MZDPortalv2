-- SQL Script to create GonderiEk table for announcement attachments
-- Run this script in your database to add the new table

-- Create GonderiEks table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='GonderiEks' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[GonderiEks] (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [GonderiId] [int] NOT NULL,
        [FileName] [nvarchar](255) NOT NULL,
        [OriginalFileName] [nvarchar](255) NOT NULL,
        [FilePath] [nvarchar](500) NOT NULL,
        [FileType] [nvarchar](100) NOT NULL,
        [FileSize] [bigint] NOT NULL,
        [UploadedAt] [datetime2](7) NOT NULL,
        [DownloadCount] [int] NOT NULL DEFAULT 0,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [Description] [nvarchar](500) NULL,
        CONSTRAINT [PK_GonderiEks] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_GonderiEks_Gonderis] FOREIGN KEY([GonderiId]) REFERENCES [dbo].[Gonderis] ([Id]) ON DELETE CASCADE
    );
    
    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_GonderiEks_GonderiId] ON [dbo].[GonderiEks]([GonderiId] ASC);
    CREATE NONCLUSTERED INDEX [IX_GonderiEks_IsActive] ON [dbo].[GonderiEks]([IsActive] ASC);
    
    PRINT 'GonderiEks table created successfully.';
END
ELSE
BEGIN
    PRINT 'GonderiEks table already exists.';
END

-- Create Uploads directory structure (this needs to be done manually on the server)
-- Create directory: ~/Uploads/Announcements/

GO