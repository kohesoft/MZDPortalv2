namespace MZDNETWORK.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Inti : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DilekOneris",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        Mesaj = c.String(maxLength: 500),
                        Bilidirim = c.String(maxLength: 500),
                        GonderimTarihi = c.DateTime(nullable: false),
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
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        Password = c.String(),
                        Role = c.String(),
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserInfoes", "UserId", "dbo.Users");
            DropForeignKey("dbo.Tasks", "UserId", "dbo.Users");
            DropForeignKey("dbo.TodoItems", "TaskId", "dbo.Tasks");
            DropIndex("dbo.UserInfoes", new[] { "UserId" });
            DropIndex("dbo.TodoItems", new[] { "TaskId" });
            DropIndex("dbo.Tasks", new[] { "UserId" });
            DropTable("dbo.UserInfoes");
            DropTable("dbo.Users");
            DropTable("dbo.TodoItems");
            DropTable("dbo.Tasks");
            DropTable("dbo.Gonderis");
            DropTable("dbo.Feedbacks");
            DropTable("dbo.Dokumantasyons");
            DropTable("dbo.DilekOneris");
        }
    }
}
