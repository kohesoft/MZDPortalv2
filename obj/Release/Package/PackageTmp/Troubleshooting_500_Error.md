# 500 Internal Server Error Troubleshooting Guide

## Step 1: Check Database Connection and Tables

1. **Execute the database setup script:**
   - Open SQL Server Management Studio
   - Connect to your database server: `192.168.1.102,1433`
   - Run the `Test_Database_Setup.sql` script
   - Verify that both `Gonderis` and `GonderiEks` tables exist

## Step 2: Test Database Connection from Application

1. **Visit the test endpoint:**
   - Navigate to: `http://yoursite/Gonderi/TestDatabase`
   - This will return JSON showing if the database connection works
   - Check if the GonderiEks table is accessible

## Step 3: Check Application Logs

1. **Enable detailed errors:**
   - In Web.config, temporarily set: `<customErrors mode="Off" />`
   - This will show the actual error message instead of generic 500 error

2. **Check Event Viewer:**
   - Open Windows Event Viewer
   - Look in Application logs for ASP.NET errors
   - Check for any database connection or Entity Framework errors

## Step 4: Common Issues and Solutions

### Issue 1: Missing GonderiEks Table
**Solution:** Run the database creation script
```sql
-- Execute Test_Database_Setup.sql
```

### Issue 2: Entity Framework Model Issues
**Solution:** The controllers have been updated to handle EF issues gracefully

### Issue 3: File Upload Configuration
**Solution:** Web.config has been updated with proper file upload settings:
- `maxRequestLength="10240"` (10MB)
- `maxAllowedContentLength="10485760"` (10MB)

### Issue 4: Permission Issues
**Solution:** Check if user has proper permissions for "HumanResources.Announcements"

## Step 5: Verify File Structure

Ensure these directories exist:
```
~/Uploads/
~/Uploads/Announcements/
```

## Step 6: Gradual Testing

1. **Test Home Page:** `/Home/Index`
2. **Test Announcement List:** `/Gonderi/GonderiListele`
3. **Test Announcement Creation:** `/Gonderi/GonderiOlustur`

## Step 7: If Issues Persist

1. **Check the exact error:**
   - Set `<customErrors mode="Off" />` in Web.config
   - Visit the problematic page
   - Note the exact error message

2. **Common Error Types:**
   - **Database Connection:** Check connection string and SQL Server
   - **Missing Table:** Run database setup scripts
   - **Entity Framework:** Check model relationships
   - **Permissions:** Verify user access rights

## Step 8: Rollback Plan

If the new features are causing issues, you can temporarily:
1. Comment out the `GonderiEkler` DbSet in ApplicationDbContext.cs
2. Remove the relationship configuration in OnModelCreating
3. This will disable file uploads but restore basic functionality

## Updated Files Summary

The following files have been modified with error handling:
- `Controllers/GonderiController.cs` - Added database test method and error handling
- `Controllers/HomeController.cs` - Added error handling for announcement loading
- `Web.config` - Added proper file upload configuration

## Next Steps

1. Run the database script first
2. Test the `/Gonderi/TestDatabase` endpoint
3. If successful, test the main pages
4. If errors persist, enable detailed error messages and check the specific error