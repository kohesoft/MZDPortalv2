-- Test script to check database connection and create GonderiEks table
-- Execute this script in SQL Server Management Studio or similar tool

-- Test connection to database
USE [MZDPORTAL_DBV2]
GO

-- Check if Gonderis table exists (parent table)
IF EXISTS (SELECT * FROM sysobjects WHERE name='Gonderis' AND xtype='U')
BEGIN
    PRINT 'Gonderis table exists - OK'
END
ELSE
BEGIN
    PRINT 'ERROR: Gonderis table does not exist! Please create it first.'
    RETURN
END

-- Create GonderiEks table if it doesn't exist
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
    
    PRINT 'GonderiEks table created successfully!';
END
ELSE
BEGIN
    PRINT 'GonderiEks table already exists.';
END

-- Verify the table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'GonderiEks'
ORDER BY ORDINAL_POSITION;

-- Test query to ensure everything works
SELECT COUNT(*) as GonderiCount FROM [Gonderis];
SELECT COUNT(*) as GonderiEksCount FROM [GonderiEks];

PRINT 'Database setup verification completed.'