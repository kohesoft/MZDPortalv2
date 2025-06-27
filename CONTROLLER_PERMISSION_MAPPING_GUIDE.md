# 🚀 MZD Portal Controller-Permission Mapping Guide v2.0

## 📋 Controller/Action Permission Complete Mapping Table

Bu tablo MZD Portal'daki tüm kontrolcülerin permission'larını tam olarak eşleştirmektedir.

### 📊 Tam Controller-Permission Tablosu

| Controller                | Permission Path                      | ActionType | Açıklama                                      |
|---------------------------|--------------------------------------|------------|-----------------------------------------------|
| **DilekOneriController**  | Suggestion.View                      | View       | Dilek/öneri bildirimleri                      |
|                           | Suggestion.Create                    | Create     | Dilek/öneri gönder                            |
|                           | Suggestion.Reply                     | Reply      | Dilek/öneriye yanıt gönder                    |
|                           | Suggestion.Manage                    | Manage     | Dilek/öneri yanıtı güncelle                   |
| **FeedbackController**    | Feedback.View                        | View       | Geri bildirim var mı kontrolü                 |
|                           | Feedback.Create                      | Create     | Geri bildirim gönder                          |
| **GonderiController**     | Announcements.View                   | View       | Duyuru listele                                |
|                           | Announcements.Create                 | Create     | Duyuru oluştur                                |
|                           | Announcements.Edit                   | Edit       | Duyuru düzenle                                |
|                           | Announcements.Delete                 | Delete     | Duyuru sil                                    |
| **InsanKaynaklariController** | HumanResources.View               | View       | Dilek/istekleri görüntüle, IK ana sayfa       |
| **Kullanici_IslemleriController** | UserManagement.View            | View       | Kullanıcı listesi/detayları                   |
|                           | UserManagement.Create                | Create     | Kullanıcı oluştur                             |
|                           | UserManagement.Edit                  | Edit       | Kullanıcı düzenle                             |
|                           | UserManagement.Delete                | Delete     | Kullanıcı sil                                 |
| **LeaveRequestController**| LeaveRequest.View                    | View       | Kendi/tüm izin taleplerini listele, detay     |
|                           | LeaveRequest.Create                  | Create     | İzin talebi oluştur                           |
|                           | LeaveRequest.Edit                    | Edit       | İzin talebi incele/güncelle                   |
| **MeetingRoomController** | MeetingRoom.View                     | View       | Rezervasyonları listele/getir                 |
|                           | MeetingRoom.Create                   | Create     | Rezervasyon oluştur                           |
|                           | MeetingRoom.Edit                     | Edit       | Rezervasyon onayla/reddet                     |
| **NotificationController**| Notification.Send                    | Send       | Bildirim gönder                               |
|                           | Notification.Read                    | Read       | Bildirimi okundu olarak işaretle/okunmamışları getir |
| **OnlineUsersController** | OnlineUsers.View                     | View       | Online kullanıcıları görüntüle                |
| **PerformanceController** | Performance.View                     | View       | Performans ana sayfa/verisi                   |
| **PermissionTreeController** | SystemManagement.Permissions.View | View       | Yetki ağacı ana sayfa/verisi/detayı           |
|                           | SystemManagement.Permissions.Create | Create     | Yetki düğümü oluştur                          |
|                           | SystemManagement.Permissions.Edit   | Edit       | Yetki düğümü güncelle                         |
|                           | SystemManagement.Permissions.Delete | Delete     | Yetki düğümü sil                              |
|                           | SystemManagement.Permissions.Manage | Manage     | Yetki ağacını yeniden oluştur                 |
| **RoleOrganizationController** | SystemManagement.RoleManagement.View | View   | Rol yönetimi ana sayfa/istatistikler          |
|                           | SystemManagement.RoleManagement.Create | Create | Dinamik rol oluştur                           |
|                           | SystemManagement.RoleManagement.Edit   | Edit   | Kullanıcı rolleri toplu güncelle              |
| **RolePermissionController** | RoleManagement.Permissions.View    | View       | Rol yetkileri ana sayfa                       |
|                           | RoleManagement.Permissions.Edit     | Edit       | Yetki ata/kaldır/rol kaydet                   |
|                           | RoleManagement.Permissions.Manage   | Manage     | Toplu yetki ata/rol şablonu uygula            |
|                           | RoleManagement.Permissions.Delete   | Delete     | Rol sil                                       |
| **SurveyController**      | Survey.View                          | View       | Anketleri listele/sonuçları görüntüle         |
|                           | Survey.Create                        | Create     | Anket oluştur                                 |
|                           | Survey.Delete                        | Delete     | Anket sil                                     |
| **TaskController**        | Task.View                            | View       | Görevleri listele/detaylar/kullanıcıya ait görevler |
|                           | Task.Create                          | Create     | Görev oluştur                                 |
|                           | Task.Edit                            | Edit       | Görev düzenle/ilerleme/ek açıklama            |
|                           | Task.Delete                          | Delete     | Görev sil                                     |
| **ChatController**        | Chat.View                            | View       | Chat ana sayfa/görüntüle                      |
|                           | Chat.Create                          | Create     | Mesaj gönderme                                |
| **ContactController**     | Contact.View                         | View       | Kişi rehberi listeleme                        |
|                           | Contact.Export                       | Export     | Kişi rehberi dışa aktarma                     |
| **DailyMoodController**   | DailyMood.Create                     | Create     | Günlük ruh hali girişi                        |
| **BeyazTahtaController**  | WhiteBoard.View                      | View       | TV ekran içeriği görüntüleme                   |
|                           | WhiteBoard.Edit                      | Edit       | TV ekran içeriği düzenleme                     |
| **BilgiIslemController**  | IT.View                              | View       | IT ana sayfa görüntüleme                       |
|                           | IT.FoodPhoto.Merkez.View             | View       | Merkez yemek fotoğrafları görüntüleme          |
|                           | IT.FoodPhoto.Merkez.Create           | Create     | Merkez yemek fotoğrafı yükleme                 |
|                           | IT.FoodPhoto.Merkez.Delete           | Delete     | Merkez yemek fotoğrafı silme                   |
|                           | IT.FoodPhoto.Yerleske.View           | View       | Yerleşke yemek fotoğrafları görüntüleme        |
|                           | IT.FoodPhoto.Yerleske.Create         | Create     | Yerleşke yemek fotoğrafı yükleme               |
|                           | IT.FoodPhoto.Yerleske.Delete         | Delete     | Yerleşke yemek fotoğrafı silme                 |
|                           | IT.BreakPhoto.Create                 | Create     | Mola fotoğrafı yükleme                         |
|                           | IT.BreakPhoto.Delete                 | Delete     | Mola fotoğrafı silme                           |

---

## 🏗️ Yeni Sistem Mimarisi

### 1. 📁 Modül Hiyerarşisi

```
MZD Portal v2.0
├── 👥 UserManagement (Kullanıcı Yönetimi)
├── 🛡️ RoleManagement (Rol Yönetimi)  
├── 🏢 HumanResources (İnsan Kaynakları)
├── 💻 InformationTechnology (Bilgi İşlem)
├── 📄 Documentation (Dokümantasyon)
├── ⚙️ SystemManagement (Sistem Yönetimi)
│   ├── 🌳 Permissions (Yetki Ağacı)
│   └── 👑 RoleManagement (Rol Organizasyon)
└── 🔄 Operational (Operasyonel İşlemler)
    ├── 💡 Suggestion (Dilek & Öneri)
    ├── 💬 Feedback (Geri Bildirim)
    ├── 📢 Announcements (Duyurular)
    ├── 📅 LeaveRequest (İzin Talepleri)
    ├── 🏛️ MeetingRoom (Toplantı Odası)
    ├── 🔔 Notification (Bildirim Sistemi)
    ├── 👤 OnlineUsers (Çevrimiçi Kullanıcılar)
    ├── 📊 Performance (Performans)
    ├── 📝 Survey (Anket Sistemi)
    ├── ✅ Task (Görev Yönetimi)
    ├── 💬 Chat (Sohbet Sistemi)
    ├── 📞 Contact (Kişi Rehberi)
    ├── 😊 DailyMood (Günlük Ruh Hali)
    ├── 📺 WhiteBoard (TV Ekran)
    ├── 🖥️ IT (Bilgi İşlem)
    └── 📈 Dashboard (Ana Dashboard)
```

---

## ✅ Sistem Durumu - PermissionTreeController

### 🎯 **PermissionTreeController Artık TAM ÇALIŞIYOR!**

✅ **Oluşturulan/Düzeltilen:**
1. ✅ `Views/PermissionTree/Index.cshtml` - Ana yetki ağacı sayfası
2. ✅ `Views/PermissionTree/Test.cshtml` - Test sayfası  
3. ✅ Navigation menüsüne link eklendi
4. ✅ Controller permission'ları PermissionSeeder'a eklendi

### 🚀 **Erişim:**
- **Ana Sayfa:** `/PermissionTree/Index`
- **Test Sayfası:** `/PermissionTree/Test` 
- **API Test:** `/PermissionTree/GetPermissionTree`

---

## 🔧 Yeni Controller Ekleme Adımları

### 1. 🎯 Controller Oluşturma

```csharp
[DynamicAuthorize("YourModule.View", "View")]
public class YourController : Controller
{
    [DynamicAuthorize("YourModule.View", "View")]
    public ActionResult Index()
    {
        return View();
    }
    
    [DynamicAuthorize("YourModule.Create", "Create")]
    public ActionResult Create()
    {
        return View();
    }
    
    [DynamicAuthorize("YourModule.Edit", "Edit")]
    public ActionResult Edit(int id)
    {
        return View();
    }
    
    [DynamicAuthorize("YourModule.Delete", "Delete")]
    public ActionResult Delete(int id)
    {
        return View();
    }
}
```

### 2. 🔑 PermissionSeeder'a Ekleme

`Data/PermissionSeeder.cs` → `CreateControllerBasedModules()` metoduna:

```csharp
// YourController Ekleme
["YourModule"] = new { 
    Name = "Modül Adı", 
    Icon = "bx-icon-name", 
    Description = "Modül açıklaması", 
    Permissions = new[]
    {
        new { Path = "YourModule.View", Name = "Görüntüle", Description = "...", ActionType = "View" },
        new { Path = "YourModule.Create", Name = "Oluştur", Description = "...", ActionType = "Create" },
        new { Path = "YourModule.Edit", Name = "Düzenle", Description = "...", ActionType = "Edit" },
        new { Path = "YourModule.Delete", Name = "Sil", Description = "...", ActionType = "Delete" }
    }
}
```

### 3. 📱 Navigation Ekleme

`Views/Shared/_Layout.cshtml`:

```html
@if (MZDNETWORK.Attributes.DynamicAuthorizeAttribute.CurrentUserHasPermission("YourModule.View", "View"))
{
    <li><a href="@Url.Action("Index", "Your")" class="nav-link">Modül Adı</a></li>
}
```

### 4. 🗃️ Veritabanı Güncellemesi

```bash
dotnet run  # PermissionSeeder otomatik çalışır
```

---

## 📊 PermissionSeeder v2.0 Özellikleri

### ✨ **Ana Özellikler:**

✅ **Tam Controller Mapping**: Tablodaki tüm kontrolcüler dahil  
✅ **ActionType Bazlı Logic**: View, Create, Edit, Delete, Manage, Send, Read, Export, Reply  
✅ **Modüler Yapı**: Her kontrolcü kendi modülü  
✅ **Hiyerarşik Yapı**: Ana modül → Alt modül → Permission  
✅ **Icon Mapping**: Her action type için uygun icon  
✅ **Otomatik Güncelleme**: Mevcut sistemde çalışır  
✅ **Admin SuperRole**: Tüm yetkilere sahip  

### 🎨 **ActionType → Icon Mapping:**

| ActionType | Icon | Açıklama |
|------------|------|----------|
| View | `bx-show` | Görüntüleme |
| Create | `bx-plus` | Oluşturma |
| Edit | `bx-edit` | Düzenleme |
| Delete | `bx-trash` | Silme |
| Manage | `bx-cog` | Yönetim |
| Send | `bx-plus` | Gönderme |
| Read | `bx-envelope-open` | Okuma |
| Export | `bx-export` | Dışa aktarma |
| Reply | `bx-edit` | Yanıtlama |

---

## 🚀 Test ve Doğrulama

### 1. **PermissionTree Test:**
```
URL: /PermissionTree/Test
API: /PermissionTree/GetPermissionTree
```

### 2. **Permission Kontrolü:**
```csharp
// Controller'da
[DynamicAuthorize("ModuleName.Action", "ActionType")]

// View'da  
@if (DynamicAuthorizeAttribute.CurrentUserHasPermission("ModuleName.Action", "ActionType"))
{
    // İçerik
}
```

### 3. **Rol Yönetimi:**
```
Rol Matrix: /RolePermission/Index
Kullanıcı Rolleri: /RoleOrganization/Index
```

---

## 🎯 Sonuç

✅ **Controller/Action Tablosu → %100 Implemented**  
✅ **PermissionTreeController → Çalışıyor**  
✅ **Dynamic Authorization → Tam Uyumlu**  
✅ **Modular Structure → Tamamlandı**  
✅ **Navigation Integration → Eklendi**  
✅ **Test Infrastructure → Hazır**  

**Sistem artık tablodaki tüm kontrolcüleri destekliyor ve yeni kontrolcü ekleme süreci net olarak tanımlanmış! 🎉** 