-- =======================================================
-- MZD PORTAL - KULLANICI SÝLME SQL SCRIPT
-- Tüm iliþkili tablolarý temizleyen kapsamlý SQL sorgularý
-- =======================================================

-- KULLANIM TALÝMATI:
-- 1. Bu script'i SQL Server Management Studio'da çalýþtýrýn
-- 2. @UserId parametresini silinecek kullanýcýnýn ID'si ile deðiþtirin
-- 3. Alternatif olarak @Username parametresini kullanabilirsiniz
-- 4. Script tüm iliþkili tablolarý sýrayla temizleyecektir

-- =======================================================
-- PARAMETRELER - Burayý kullanýcýya göre güncelleyin
-- =======================================================
USE MZDPORTAL_DBV2

DECLARE @UserId INT = NULL;              -- Silinecek kullanýcýnýn ID'si (örnek: 123)
DECLARE @Username NVARCHAR(255) = NULL;  -- Alternatif: Kullanýcý adý (örnek: 'john.doe')

-- Username ile UserId'yi bulma (Username verilmiþse)
IF @Username IS NOT NULL AND @UserId IS NULL
BEGIN
    SELECT @UserId = Id FROM Users WHERE Username = @Username;
    IF @UserId IS NULL
    BEGIN
        PRINT 'HATA: Belirtilen kullanýcý adý bulunamadý: ' + @Username;
        RETURN;
    END
END

-- UserId kontrolü
IF @UserId IS NULL
BEGIN
    PRINT 'HATA: Lütfen @UserId veya @Username parametresini belirtin!';
    RETURN;
END

-- Kullanýcý bilgilerini göster
DECLARE @FoundUsername NVARCHAR(255), @FullName NVARCHAR(255);
SELECT @FoundUsername = Username, @FullName = Name + ' ' + Surname 
FROM Users WHERE Id = @UserId;

IF @FoundUsername IS NULL
BEGIN
    PRINT 'HATA: Belirtilen UserId bulunamadý: ' + CAST(@UserId AS NVARCHAR);
    RETURN;
END

PRINT '=======================================================';
PRINT 'SÝLÝNECEK KULLANICI BÝLGÝLERÝ:';
PRINT 'User ID: ' + CAST(@UserId AS NVARCHAR);
PRINT 'Username: ' + @FoundUsername;
PRINT 'Full Name: ' + @FullName;
PRINT '=======================================================';

-- Transaction baþlat
BEGIN TRANSACTION UserDeletion;

BEGIN TRY
    -- =======================================================
    -- 1. PERMISSION & ROLE SÝSTEMÝ TEMÝZLÝÐÝ
    -- =======================================================
    PRINT '1. Permission Cache kayýtlarý siliniyor...';
    DELETE FROM PermissionCaches WHERE UserId = @UserId;
    PRINT '   - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' PermissionCache kaydý silindi.';

    PRINT '2. User Role iliþkileri siliniyor...';
    DELETE FROM UserRoles WHERE UserId = @UserId;
    PRINT '   - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' UserRole kaydý silindi.';

    -- =======================================================
    -- 2. NOTIFICATION SÝSTEMÝ TEMÝZLÝÐÝ
    -- =======================================================
    PRINT '3. Notification kayýtlarý siliniyor...';
    DELETE FROM Notifications WHERE UserId = CAST(@UserId AS NVARCHAR);
    PRINT '   - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' Notification kaydý silindi.';

    -- =======================================================
    -- 3. CHAT SÝSTEMÝ TEMÝZLÝÐÝ
    -- =======================================================
    PRINT '4. Chat Group üyelikleri siliniyor...';
    DELETE FROM ChatGroupMembers WHERE UserId = @UserId;
    PRINT '   - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' ChatGroupMember kaydý silindi.';

    PRINT '5. Chat mesajlarý soft delete yapýlýyor...';
    UPDATE ChatMessages 
    SET IsDeleted = 1, DeletedAt = GETDATE(), DeletedBy = @UserId
    WHERE UserId = @UserId AND IsDeleted = 0;
    PRINT '   - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' ChatMessage kaydý soft delete yapýldý.';

    PRINT '6. Oluþturulan Chat Groups siliniyor...';
    DELETE FROM ChatGroups WHERE CreatedBy = @UserId;
    PRINT '   - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' ChatGroup kaydý silindi.';

    PRINT '6. Oluþturulan Chat Groups siliniyor...';
    DELETE FROM ChatGroupUsers WHERE User_Id = @UserId;
    PRINT '   - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' ChatGroup kaydý silindi.';
    
    PRINT '6. Oluþturulan Chat Groups siliniyor...';
    DELETE FROM ChatGroupUser1 WHERE User_Id = @UserId;
    PRINT '   - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' ChatGroup kaydý silindi.';

    PRINT '7. Oluþturulan Chat kayýtlarý siliniyor...';
    DELETE FROM Chats WHERE CreatedBy = @UserId;
    PRINT '   - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' Chat kaydý silindi.';

    -- =======================================================
    -- 4. TASK YÖNETÝMÝ SÝSTEMÝ TEMÝZLÝÐÝ
    -- =======================================================
    PRINT '8. Todo Items siliniyor...';
    DELETE FROM TodoItems WHERE TaskId IN (SELECT Id FROM Tasks WHERE UserId = @UserId);
    PRINT '   - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' TodoItem kaydý silindi.';

    PRINT '9. Task kayýtlarý siliniyor...';
    DELETE FROM Tasks WHERE UserId = @UserId;
    PRINT '   - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' Task kaydý silindi.';

    -- =======================================================
    -- 5. SERVÝS YÖNETÝMÝ SÝSTEMÝ TEMÝZLÝÐÝ
    -- =======================================================
    PRINT '10. Service Personnel kayýtlarý siliniyor...';
    DELETE FROM ServicePersonnels WHERE UserId = @UserId;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' ServicePersonnel kaydý silindi.';

    PRINT '11. Overtime Service Personnel kayýtlarý siliniyor...';
    DELETE FROM OvertimeServicePersonnels WHERE UserId = @UserId;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' OvertimeServicePersonnel kaydý silindi.';

    -- =======================================================
    -- 6. DÝLEK & ÖNERÝ SÝSTEMÝ TEMÝZLÝÐÝ
    -- =======================================================
    PRINT '12. Dilek & Öneri kayýtlarý siliniyor...';
    DELETE FROM DilekOneris WHERE Username = @FoundUsername;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' DilekOneri kaydý silindi.';

    PRINT '13. Suggestion Complaint kayýtlarý siliniyor...';
    DELETE FROM SuggestionComplaints WHERE UserId = @UserId;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' SuggestionComplaint kaydý silindi.';

    -- =======================================================
    -- 7. ANKET SÝSTEMÝ TEMÝZLÝÐÝ
    -- =======================================================
    PRINT '14. Answer Options siliniyor...';
    DELETE FROM AnswerOptions WHERE AnswerId IN (SELECT ID FROM Answers WHERE UserID = @UserId);
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' AnswerOption kaydý silindi.';

    PRINT '15. Survey Answers siliniyor...';
    DELETE FROM Answers WHERE UserID = @UserId;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' Answer kaydý silindi.';

    -- =======================================================
    -- 8. REZERVASYON SÝSTEMÝ TEMÝZLÝÐÝ
    -- =======================================================
    PRINT '16. Rezervasyon kayýtlarý siliniyor...';
    DELETE FROM Reservations WHERE UserId = @UserId;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' Reservation kaydý silindi.';

    PRINT '17. Meeting Room Reservation kayýtlarý siliniyor...';
    DELETE FROM MeetingRoomReservations WHERE UserId = @UserId;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' MeetingRoomReservation kaydý silindi.';

    -- =======================================================
    -- 9. PERSONEL TAKÝP SÝSTEMÝ TEMÝZLÝÐÝ
    -- =======================================================
    PRINT '18. Daily Mood kayýtlarý siliniyor...';
    DELETE FROM DailyMoods WHERE UserId = @UserId;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' DailyMood kaydý silindi.';

    PRINT '19. Leave Request kayýtlarý siliniyor...';
    DELETE FROM LeaveRequests WHERE UserId = @UserId;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' LeaveRequest kaydý silindi.';

    PRINT '20. Beyaz Tahta Entry kayýtlarý siliniyor...';
    DELETE FROM BeyazTahtaEntries WHERE UserId = @UserId;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' BeyazTahtaEntry kaydý silindi.';

    -- =======================================================
    -- 10. ZÝYARETÇÝ TAKÝP SÝSTEMÝ TEMÝZLÝÐÝ
    -- =======================================================
    PRINT '21. Visitor Entry kayýtlarý siliniyor...';
    DELETE FROM VisitorEntries 
    WHERE FullName LIKE '%' + (SELECT Name FROM Users WHERE Id = @UserId) + '%' 
      AND FullName LIKE '%' + (SELECT Surname FROM Users WHERE Id = @UserId) + '%';
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' VisitorEntry kaydý silindi.';

    PRINT '22. Late Arrival Report kayýtlarý siliniyor...';
    DELETE FROM LateArrivalReports 
    WHERE FullName LIKE '%' + (SELECT Name FROM Users WHERE Id = @UserId) + '%' 
      AND FullName LIKE '%' + (SELECT Surname FROM Users WHERE Id = @UserId) + '%';
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' LateArrivalReport kaydý silindi.';

    -- =======================================================
    -- 11. ÞÝFRE SIFIRLAMA SÝSTEMÝ TEMÝZLÝÐÝ
    -- =======================================================
    PRINT '23. Password Reset Request kayýtlarý siliniyor...';
    DELETE FROM PasswordResetRequests WHERE UserId = @UserId;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' PasswordResetRequest kaydý silindi.';

    -- =======================================================
    -- 13. DÖKÜMAN YÖNETÝMÝ TEMÝZLÝÐÝ
    -- =======================================================
    PRINT '25. Dokümantasyon kayýtlarý siliniyor...';
    DELETE FROM Dokumantasyons WHERE Username = @FoundUsername;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' Dokumantasyon kaydý silindi.';

    -- =======================================================
    -- 15. KULLANICI BÝLGÝLERÝ TEMÝZLÝÐÝ (SON ADIMLAR)
    -- =======================================================
    PRINT '27. UserInfo kayýtlarý siliniyor...';
    DELETE FROM UserInfoes WHERE UserId = @UserId;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' UserInfo kaydý silindi.';

    PRINT '28. Ana User kaydý siliniyor...';
    DELETE FROM Users WHERE Id = @UserId;
    PRINT '    - ' + CAST(@@ROWCOUNT AS NVARCHAR) + ' User kaydý silindi.';

    -- =======================================================
    -- ÝÞLEM TAMAMLANDI
    -- =======================================================
    COMMIT TRANSACTION UserDeletion;
    
    PRINT '=======================================================';
    PRINT 'BAÞARILI! Kullanýcý ve tüm iliþkili veriler silindi.';
    PRINT 'Silinen Kullanýcý: ' + @FoundUsername + ' (ID: ' + CAST(@UserId AS NVARCHAR) + ')';
    PRINT '=======================================================';

END TRY
BEGIN CATCH
    -- Hata durumunda transaction'ý geri al
    ROLLBACK TRANSACTION UserDeletion;
    
    PRINT '=======================================================';
    PRINT 'HATA! Ýþlem geri alýndý.';
    PRINT 'Hata Mesajý: ' + ERROR_MESSAGE();
    PRINT 'Hata Satýrý: ' + CAST(ERROR_LINE() AS NVARCHAR);
    PRINT '=======================================================';
    
    -- Hata detaylarýný göster
    SELECT 
        ERROR_NUMBER() AS ErrorNumber,
        ERROR_SEVERITY() AS ErrorSeverity,
        ERROR_STATE() AS ErrorState,
        ERROR_PROCEDURE() AS ErrorProcedure,
        ERROR_LINE() AS ErrorLine,
        ERROR_MESSAGE() AS ErrorMessage;
END CATCH;

-- =======================================================
-- EK KONTROL SORGUSU (ÝSTEÐE BAÐLI)
-- Silme iþleminden sonra geriye kalan kayýtlarý kontrol etmek için
-- =======================================================
/*
-- Bu kýsmý aktif etmek isterseniz comment'larý kaldýrýn:

PRINT 'KONTROL: Kullanýcý ile iliþkili kalan kayýtlar kontrol ediliyor...';

SELECT 'Users' AS TableName, COUNT(*) AS RemainingRecords FROM Users WHERE Id = @UserId
UNION ALL
SELECT 'UserInfos', COUNT(*) FROM UserInfos WHERE UserId = @UserId
UNION ALL
SELECT 'UserRoles', COUNT(*) FROM UserRoles WHERE UserId = @UserId
UNION ALL
SELECT 'PermissionCaches', COUNT(*) FROM PermissionCaches WHERE UserId = @UserId
UNION ALL
SELECT 'ChatMessages', COUNT(*) FROM ChatMessages WHERE UserId = @UserId AND IsDeleted = 0
UNION ALL
SELECT 'ChatGroupMembers', COUNT(*) FROM ChatGroupMembers WHERE UserId = @UserId
UNION ALL
SELECT 'Tasks', COUNT(*) FROM Tasks WHERE UserId = @UserId
UNION ALL
SELECT 'ServicePersonnels', COUNT(*) FROM ServicePersonnels WHERE UserId = @UserId
UNION ALL
SELECT 'Notifications', COUNT(*) FROM Notifications WHERE UserId = CAST(@UserId AS NVARCHAR);
*/