# pop-up yapalım bildirimler bilgisayarında gözüksün

# Yetki & rol & kullanıcı düzenleme talebi
- kullanıcı yetki, rol, kullanıcı bilgilerini düzenleme talebi ticket oluşturacağı yer.
- admin paneli gelen ticketlara göre aksiyon alacak.
- kullanıcı yetki değişikliği talebi seçerse mevcut yetkisi ve istenilen yetki seçecek. bunlar controllerdeki yetki isimleri olacak. türkçe ve anlamlı şekile yazmamız lazım dinamik olucak rol yetki yeri gibi.
- kullanıcı rol değişikliği seçerse mevcut rolu ve istenilen rolu seçecek. bunlar controllerdeki rol isimleri olacak. türkçe ve anlamlı şekile yazmamız lazım dinamik olucak rol yetki yeri gibi.
- kullanıcı bilgileri düzenleme talebi mevcut bilgileri yüklenecek yeni bilgileri girecek ve ticket atıcak admin tarafından onaylanırsa değişiklik hem uygulanacak hemde kullanıcıya bilgi gidicek.
- admin panelinde bu ticketlar listelenecek onayla reddet butonları olacak. onaylarsa gerekli değişiklikler yapılacak ve kullanıcıya bilgi bildirimi gidecek. reddederse sadece kullanıcıya bilgi bildirimi gidecek.
- sistemde seninde parmağın olsun yukarıda belirtilen maddelere uyarak tasarım ve implementasyon yap.

## yukarıdaki maddeler için soru sorman gerekiyorsa bu başlığın altında sor.
- Roller/Permissions: Role listesi ve PermissionNode.Path zaten DB’de; dropdown’da bunları kullanmam doğru mu? (Varsayılan: Role tablosu + PermissionNodes)

Kullanabilirsin

- Kullanıcı bilgisi alanları: Hangi alanlar değiştirilebilir? (Örn. Name, Surname, Username, InternalEmail, ExternalEmail, Department, Title, Phone… hangi sütunlar zorunlu/serbest?)

Zorunlu alan yok hepsi serbest

- Admin yetki kontrolü: Bu modül için hangi permission kullanılmalı? (Örn. RoleManagement.RoleManagement.Manage veya yeni bir SystemManagement.AccessChange.Manage tanımlayalım mı?)

- Yeni bir permission tanımlayalım: UserManagement.AccessChange.Manage

- Desktop bildirim: Tarayıcı Notification API ile (kullanıcı izin verirse) OS toast gösterelim, izin yoksa mevcut toast kalsın — uygun mu?

uygun

- Onay uygulama davranışı: Onayla dediğimiz anda değişiklikler otomatik uygulansın mı, yoksa “Onaylandı” + ayrı “Uygula” butonu ister misiniz?

otomatik uygulansın