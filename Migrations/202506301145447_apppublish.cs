namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class apppublish : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Answers",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        QuestionID = c.Int(nullable: false),
                        UserID = c.Int(nullable: false),
                        AnswerText = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Questions", t => t.QuestionID, cascadeDelete: true)
                .Index(t => t.QuestionID);
            
            CreateTable(
                "dbo.AnswerOptions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        QuestionID = c.Int(nullable: false),
                        OptionText = c.String(),
                        AnswerId = c.Int(nullable: false),
                        Question_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Answers", t => t.AnswerId)
                .ForeignKey("dbo.Questions", t => t.Question_ID)
                .ForeignKey("dbo.Questions", t => t.QuestionID)
                .Index(t => t.QuestionID)
                .Index(t => t.AnswerId)
                .Index(t => t.Question_ID);
            
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        SurveyID = c.Int(nullable: false),
                        QuestionText = c.String(),
                        Type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Surveys", t => t.SurveyID, cascadeDelete: true)
                .Index(t => t.SurveyID);
            
            CreateTable(
                "dbo.Surveys",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        Duration = c.Int(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.BeyazTahtaEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedAt = c.DateTime(nullable: false),
                        OneriVeren = c.String(),
                        Problem = c.String(),
                        Oneri = c.String(),
                        UserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        Password = c.String(),
                        Name = c.String(),
                        Surname = c.String(),
                        Department = c.String(),
                        Position = c.String(),
                        Intercom = c.String(),
                        PhoneNumber = c.String(),
                        InternalEmail = c.String(),
                        ExternalEmail = c.String(),
                        Sicil = c.String(),
                        IsPasswordChanged = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Tasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Username = c.String(),
                        UserId = c.Int(nullable: false),
                        Progress = c.Int(nullable: false),
                        CreatedBy = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.TodoItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        AdditionalDescription = c.String(),
                        IsCompleted = c.Boolean(nullable: false),
                        TaskId = c.Int(nullable: false),
                        DueDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tasks", t => t.TaskId, cascadeDelete: true)
                .Index(t => t.TaskId);
            
            CreateTable(
                "dbo.UserInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        RealPhoneNumber = c.String(),
                        Adres = c.String(),
                        Adres2 = c.String(),
                        Sehir = c.String(),
                        Ulke = c.String(),
                        Postakodu = c.String(),
                        KanGrubu = c.String(),
                        DogumTarihi = c.String(),
                        Cinsiyet = c.String(),
                        MedeniDurum = c.String(),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        AssignedDate = c.DateTime(nullable: false),
                        AssignedBy = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Role_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Roles", t => t.Role_Id)
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId)
                .Index(t => t.Role_Id);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(maxLength: 200),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        CreatedBy = c.Int(nullable: false),
                        ModifiedDate = c.DateTime(),
                        ModifiedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ChatMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChatId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        Content = c.String(nullable: false, unicode: false, storeType: "text"),
                        MessageType = c.String(maxLength: 50),
                        FilePath = c.String(maxLength: 200),
                        FileName = c.String(maxLength: 100),
                        FileSize = c.Long(),
                        ReplyToMessageId = c.Int(),
                        IsEdited = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        DeletedAt = c.DateTime(),
                        DeletedBy = c.Int(),
                        SentAt = c.DateTime(nullable: false),
                        EditedAt = c.DateTime(),
                        IsRead = c.Boolean(nullable: false),
                        ReadAt = c.DateTime(),
                        IpAddress = c.String(maxLength: 45),
                        UserAgent = c.String(maxLength: 500),
                        Status = c.String(maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Chats", t => t.ChatId)
                .ForeignKey("dbo.Users", t => t.DeletedBy)
                .ForeignKey("dbo.ChatMessages", t => t.ReplyToMessageId)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.ChatId)
                .Index(t => t.UserId)
                .Index(t => t.ReplyToMessageId)
                .Index(t => t.DeletedBy);
            
            CreateTable(
                "dbo.Chats",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        Type = c.String(maxLength: 50),
                        CreatedBy = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        MaxMembers = c.Int(),
                        RequiresModeration = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        LastMessageAt = c.DateTime(),
                        LastMessage = c.String(maxLength: 200),
                        MessageCount = c.Int(nullable: false),
                        MemberCount = c.Int(nullable: false),
                        Status = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.CreatedBy, cascadeDelete: true)
                .Index(t => t.CreatedBy);
            
            CreateTable(
                "dbo.DailyMoods",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Mood = c.Int(nullable: false),
                        Comment = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.DilekOneris",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        Mesaj = c.String(),
                        Bilidirim = c.String(),
                        GonderimTarihi = c.DateTime(nullable: false),
                        IsAnonymous = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Dokumantasyons",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        TalepAdSoyad = c.String(),
                        TalepBirim = c.String(),
                        TalepUnvan = c.String(),
                        TalepTarihi = c.DateTime(nullable: false),
                        HamUreticiParcaNo = c.String(),
                        HamDatasheet = c.String(),
                        HamHammaddeTuru = c.String(),
                        HamStokBirimi = c.String(),
                        HamDokumantasyonTarafindanVerilenKod = c.String(),
                        MamulFirmaAdi = c.String(),
                        MamulMamulKodu = c.String(),
                        MamulMamulTuru = c.String(),
                        MamulStokTanimi = c.String(),
                        MamulDokumantasyonTarafindanVerilenKod = c.String(),
                        YariMamulFirmaAdi = c.String(),
                        YariMamulYarimamulTuru = c.String(),
                        YariMamulStokTanimi = c.String(),
                        YarimamulBagliMamulKodu = c.String(),
                        YarimamulDokumantasyonTarafindanVerilenKod = c.String(),
                        UretimDokFirmaAdi = c.String(),
                        UretimDokTur = c.String(),
                        UretimDokStokTanimi = c.String(),
                        UretimDokBagliMamulKodu = c.String(),
                        UretimDokumantasyonTarafindanVerilenKod = c.String(),
                        KablajTahtasiFirmaAdi = c.String(),
                        KablajTahtasiTuru = c.String(),
                        KablajTahtasiStokTanimi = c.String(),
                        KablajTahtasiBagliKod = c.String(),
                        KablajTahtasiDokumantasyonTarafindanVerilenKod = c.String(),
                        EpoksiFirmaAdi = c.String(),
                        EpoksiHammadeParcaNo = c.String(),
                        EpoksiYukseklik = c.String(),
                        EpoksiTur = c.String(),
                        EpoksiStokTanimi = c.String(),
                        EpoksiBagliKod = c.String(),
                        EpoksiDokumantasyonTarafindanVerilenKod = c.String(),
                        MakinaFirmaAdi = c.String(),
                        MakinaYatirimNo = c.String(),
                        MakinaHammadeParcaNo = c.String(),
                        MakinaTur = c.String(),
                        MakinaStokTanimi = c.String(),
                        MakinaBagliKod = c.String(),
                        MakinaDokumantasyonTarafindanVerilenKod = c.String(),
                        TestKonnektorTur = c.String(),
                        TestHammadeParcaNo = c.String(),
                        TestJigNoSiraNo = c.String(),
                        TestParcaAciklama = c.String(),
                        TestStokTanimi = c.String(),
                        TestDokumantasyonTarafindanVerilenKod = c.String(),
                        EpoksiHavFirmaAdi = c.String(),
                        EpoksiHavHammadeParcaNo = c.String(),
                        EpoksiHavEn = c.String(),
                        EpoksiHavBoy = c.String(),
                        EpoksiHavDerinlik = c.String(),
                        EpoksiHavTur = c.String(),
                        EpoksiHavStokTanimi = c.String(),
                        EpoksiHavBagliKod = c.String(),
                        EpoksiHavDokumantasyonTarafindanVerilenKod = c.String(),
                        DokumantasyonSorumlu = c.String(),
                        DokumantasyonTarih = c.String(),
                        KaliteYonetimTemsilcisi = c.String(),
                        KaliteYonetimTemsilcisiTarih = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Feedbacks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Liked = c.Boolean(nullable: false),
                        Suggestion = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        IpAddress = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Gonderis",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Content = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LeaveRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        LeaveType = c.Int(nullable: false),
                        Reason = c.String(nullable: false, maxLength: 500),
                        Description = c.String(maxLength: 1000),
                        Status = c.Int(nullable: false),
                        ApprovalReason = c.String(maxLength: 500),
                        ApprovedById = c.Int(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        ContactInfo = c.String(maxLength: 200),
                        Department = c.String(maxLength: 100),
                        SubstituteName = c.String(maxLength: 200),
                        Tasks = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.ApprovedById)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.ApprovedById);
            
            CreateTable(
                "dbo.MeetingRoomReservations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        RoomName = c.String(nullable: false, maxLength: 100),
                        Title = c.String(nullable: false, maxLength: 200),
                        Description = c.String(unicode: false, storeType: "text"),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        AttendeeCount = c.Int(nullable: false),
                        Attendees = c.String(maxLength: 500),
                        Status = c.String(maxLength: 50),
                        EquipmentNeeds = c.String(maxLength: 200),
                        IsRecurring = c.Boolean(nullable: false),
                        RecurrenceType = c.String(maxLength: 50),
                        RecurrenceEndDate = c.DateTime(),
                        IsApproved = c.Boolean(nullable: false),
                        ApprovedBy = c.Int(),
                        ApprovedAt = c.DateTime(),
                        ApprovalNote = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        Priority = c.String(maxLength: 50),
                        ReminderMinutes = c.Int(nullable: false),
                        ReminderSent = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.ApprovedBy)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.ApprovedBy);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(),
                        Message = c.String(),
                        IsRead = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PermissionCaches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        PermissionsJson = c.String(nullable: false, unicode: false, storeType: "text"),
                        CachedAt = c.DateTime(nullable: false),
                        LastAccessedAt = c.DateTime(nullable: false),
                        ExpiresAt = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Version = c.Int(nullable: false),
                        SizeInBytes = c.Int(nullable: false),
                        HitCount = c.Int(nullable: false),
                        CreatedBy = c.String(maxLength: 100),
                        CacheReason = c.String(maxLength: 200),
                        DataHash = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.PermissionNodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Path = c.String(nullable: false, maxLength: 200),
                        Description = c.String(maxLength: 500),
                        ParentId = c.Int(),
                        Type = c.String(nullable: false, maxLength: 50),
                        Icon = c.String(maxLength: 50),
                        SortOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        HasViewPermission = c.Boolean(nullable: false),
                        HasCreatePermission = c.Boolean(nullable: false),
                        HasEditPermission = c.Boolean(nullable: false),
                        HasDeletePermission = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PermissionNodes", t => t.ParentId)
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.RolePermissions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoleId = c.Int(nullable: false),
                        PermissionNodeId = c.Int(nullable: false),
                        CanView = c.Boolean(nullable: false),
                        CanCreate = c.Boolean(nullable: false),
                        CanEdit = c.Boolean(nullable: false),
                        CanDelete = c.Boolean(nullable: false),
                        CanManage = c.Boolean(nullable: false),
                        CanApprove = c.Boolean(nullable: false),
                        CanReject = c.Boolean(nullable: false),
                        CanExport = c.Boolean(nullable: false),
                        CanImport = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        CreatedBy = c.String(maxLength: 100),
                        UpdatedBy = c.String(maxLength: 100),
                        Notes = c.String(maxLength: 500),
                        PermissionNode_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PermissionNodes", t => t.PermissionNodeId, cascadeDelete: true)
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.PermissionNodes", t => t.PermissionNode_Id)
                .Index(t => t.RoleId)
                .Index(t => t.PermissionNodeId)
                .Index(t => t.PermissionNode_Id);
            
            CreateTable(
                "dbo.Reservations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        UserName = c.String(),
                        Room = c.String(),
                        Date = c.DateTime(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                        Title = c.String(),
                        Description = c.String(),
                        Attendees = c.String(),
                        Status = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ApprovedAt = c.DateTime(),
                        RejectedAt = c.DateTime(),
                        RejectionReason = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SuggestionComplaints",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 500),
                        Content = c.String(nullable: false, unicode: false, storeType: "text"),
                        Type = c.String(nullable: false, maxLength: 50),
                        Category = c.String(maxLength: 100),
                        IsAnonymous = c.Boolean(nullable: false),
                        Status = c.String(maxLength: 50),
                        Priority = c.String(maxLength: 100),
                        AdminResponse = c.String(unicode: false, storeType: "text"),
                        ReviewedBy = c.Int(),
                        ReviewedAt = c.DateTime(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        Tags = c.String(maxLength: 200),
                        VoteCount = c.Int(nullable: false),
                        ViewCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.ReviewedBy)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.ReviewedBy);
            
            CreateTable(
                "dbo.TvHeaders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.VisitorEntries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        FullName = c.String(nullable: false, maxLength: 150),
                        Organization = c.String(maxLength: 150),
                        Duty = c.String(maxLength: 100),
                        IdentityNo = c.String(maxLength: 20),
                        TCKimlik = c.String(maxLength: 11),
                        EntryTime = c.Time(nullable: false, precision: 7),
                        ExitTime = c.Time(precision: 7),
                        SignaturePath = c.String(),
                        Approved = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.VisitorEntryHeaders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentNo = c.String(nullable: false, maxLength: 100),
                        FirstPublishDate = c.DateTime(nullable: false),
                        RevisionDate = c.DateTime(nullable: false),
                        RevisionNo = c.String(nullable: false, maxLength: 20),
                        PrintableNote = c.String(maxLength: 300),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SuggestionComplaints", "UserId", "dbo.Users");
            DropForeignKey("dbo.SuggestionComplaints", "ReviewedBy", "dbo.Users");
            DropForeignKey("dbo.RolePermissions", "PermissionNode_Id", "dbo.PermissionNodes");
            DropForeignKey("dbo.RolePermissions", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.RolePermissions", "PermissionNodeId", "dbo.PermissionNodes");
            DropForeignKey("dbo.PermissionNodes", "ParentId", "dbo.PermissionNodes");
            DropForeignKey("dbo.PermissionCaches", "UserId", "dbo.Users");
            DropForeignKey("dbo.MeetingRoomReservations", "UserId", "dbo.Users");
            DropForeignKey("dbo.MeetingRoomReservations", "ApprovedBy", "dbo.Users");
            DropForeignKey("dbo.LeaveRequests", "UserId", "dbo.Users");
            DropForeignKey("dbo.LeaveRequests", "ApprovedById", "dbo.Users");
            DropForeignKey("dbo.DailyMoods", "UserId", "dbo.Users");
            DropForeignKey("dbo.ChatMessages", "UserId", "dbo.Users");
            DropForeignKey("dbo.ChatMessages", "ReplyToMessageId", "dbo.ChatMessages");
            DropForeignKey("dbo.ChatMessages", "DeletedBy", "dbo.Users");
            DropForeignKey("dbo.ChatMessages", "ChatId", "dbo.Chats");
            DropForeignKey("dbo.Chats", "CreatedBy", "dbo.Users");
            DropForeignKey("dbo.BeyazTahtaEntries", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserRoles", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.UserRoles", "Role_Id", "dbo.Roles");
            DropForeignKey("dbo.UserInfoes", "UserId", "dbo.Users");
            DropForeignKey("dbo.Tasks", "UserId", "dbo.Users");
            DropForeignKey("dbo.TodoItems", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.AnswerOptions", "QuestionID", "dbo.Questions");
            DropForeignKey("dbo.Questions", "SurveyID", "dbo.Surveys");
            DropForeignKey("dbo.Answers", "QuestionID", "dbo.Questions");
            DropForeignKey("dbo.AnswerOptions", "Question_ID", "dbo.Questions");
            DropForeignKey("dbo.AnswerOptions", "AnswerId", "dbo.Answers");
            DropIndex("dbo.SuggestionComplaints", new[] { "ReviewedBy" });
            DropIndex("dbo.SuggestionComplaints", new[] { "UserId" });
            DropIndex("dbo.RolePermissions", new[] { "PermissionNode_Id" });
            DropIndex("dbo.RolePermissions", new[] { "PermissionNodeId" });
            DropIndex("dbo.RolePermissions", new[] { "RoleId" });
            DropIndex("dbo.PermissionNodes", new[] { "ParentId" });
            DropIndex("dbo.PermissionCaches", new[] { "UserId" });
            DropIndex("dbo.MeetingRoomReservations", new[] { "ApprovedBy" });
            DropIndex("dbo.MeetingRoomReservations", new[] { "UserId" });
            DropIndex("dbo.LeaveRequests", new[] { "ApprovedById" });
            DropIndex("dbo.LeaveRequests", new[] { "UserId" });
            DropIndex("dbo.DailyMoods", new[] { "UserId" });
            DropIndex("dbo.Chats", new[] { "CreatedBy" });
            DropIndex("dbo.ChatMessages", new[] { "DeletedBy" });
            DropIndex("dbo.ChatMessages", new[] { "ReplyToMessageId" });
            DropIndex("dbo.ChatMessages", new[] { "UserId" });
            DropIndex("dbo.ChatMessages", new[] { "ChatId" });
            DropIndex("dbo.UserRoles", new[] { "Role_Id" });
            DropIndex("dbo.UserRoles", new[] { "RoleId" });
            DropIndex("dbo.UserRoles", new[] { "UserId" });
            DropIndex("dbo.UserInfoes", new[] { "UserId" });
            DropIndex("dbo.TodoItems", new[] { "TaskId" });
            DropIndex("dbo.Tasks", new[] { "UserId" });
            DropIndex("dbo.BeyazTahtaEntries", new[] { "UserId" });
            DropIndex("dbo.Questions", new[] { "SurveyID" });
            DropIndex("dbo.AnswerOptions", new[] { "Question_ID" });
            DropIndex("dbo.AnswerOptions", new[] { "AnswerId" });
            DropIndex("dbo.AnswerOptions", new[] { "QuestionID" });
            DropIndex("dbo.Answers", new[] { "QuestionID" });
            DropTable("dbo.VisitorEntryHeaders");
            DropTable("dbo.VisitorEntries");
            DropTable("dbo.TvHeaders");
            DropTable("dbo.SuggestionComplaints");
            DropTable("dbo.Reservations");
            DropTable("dbo.RolePermissions");
            DropTable("dbo.PermissionNodes");
            DropTable("dbo.PermissionCaches");
            DropTable("dbo.Notifications");
            DropTable("dbo.MeetingRoomReservations");
            DropTable("dbo.LeaveRequests");
            DropTable("dbo.Gonderis");
            DropTable("dbo.Feedbacks");
            DropTable("dbo.Dokumantasyons");
            DropTable("dbo.DilekOneris");
            DropTable("dbo.DailyMoods");
            DropTable("dbo.Chats");
            DropTable("dbo.ChatMessages");
            DropTable("dbo.Roles");
            DropTable("dbo.UserRoles");
            DropTable("dbo.UserInfoes");
            DropTable("dbo.TodoItems");
            DropTable("dbo.Tasks");
            DropTable("dbo.Users");
            DropTable("dbo.BeyazTahtaEntries");
            DropTable("dbo.Surveys");
            DropTable("dbo.Questions");
            DropTable("dbo.AnswerOptions");
            DropTable("dbo.Answers");
        }
    }
}
