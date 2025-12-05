## Riskler & Öneriler

### 🔴 Yüksek Öncelikli Riskler

#### 1. Bildirim Sistemi Entegrasyonu Eksik
- **Risk:** Şu anda bildirim sistemi ile entegrasyon planlanmış ancak kod tabanında aktif bir SignalR/bildirim mekanizması görünmüyor
- **Öneri:** 
  - SignalR hub'ı oluşturulmalı (`NotificationHub.cs`)
  - Bildirim modeli ve servisi geliştirilmeli
  - Real-time bildirim altyapısı kurulmalı
  - E-posta servisi entegrasyonu yapılmalı (SMTP ayarları)

#### 2. Kullanıcı Seçimi Manuel Yapılıyor
- **Risk:** Katılımcılar string olarak virgülle ayrılmış şekilde girilmekte, bu hatalı veri girişine açık
- **Öneri:**
  - AspNetUsers tablosundan kullanıcı listesi çekilmeli
  - Multiselect/Tokenize input komponenti eklenm
  - Kullanıcı arama ve filtreleme özelliği geliştirilmeli
  - Select2 veya benzer bir kütüphane kullanılabilir

#### 3. Toplantı Notları ve Kararlar Sistemi Yok
- **Risk:** Planlanan toplantı notları ve kararlarının kaydedilmesi özelliği henüz implemente edilmemiş
- **Öneri:**
  - `MeetingNotes` ve `MeetingDecisions` tabloları oluşturulmalı
  - WYSIWYG editör (TinyMCE, CKEditor) entegre edilmeli
  - Karar maddeleri için checklist/checkbox sistemi eklenebilir
  - Toplantı sonrası rapor oluşturma özelliği geliştirilmeli

### 🟡 Orta Öncelikli Riskler

#### 4. Toplantı Hatırlatıcı Sistemi Eksik
- **Risk:** Kullanıcılara toplantı öncesi otomatik hatırlatma gönderilemiyor
- **Öneri:**
  - Background job servisi (Hangfire, Quartz.NET) kurulmalı
  - Toplantıdan 24 saat, 1 saat, 15 dakika önce hatırlatma gönderilmeli
  - E-posta ve/veya in-app notification ile bildirim yapılmalı

#### 5. Rezervasyon Düzenleme/Silme Eksik
- **Risk:** Kullanıcılar oluşturdukları rezervasyonları düzenleyemiyor veya iptal edemiyor
- **Öneri:**
  - `CancelReservation` metodu mevcut ancak UI'da eksik
  - Edit/Update endpoint'i eklenmeli
  - Sadece kendi rezervasyonlarını düzenleyebilme yetkisi kontrolü yapılmalı
  - Onaylanmış toplantılar için düzenleme kısıtlaması getirilmeli

#### 6. Tekrarlayan Toplantı Desteği Yok
- **Risk:** Haftalık/aylık düzenli toplantılar için her seferinde yeni rezervasyon oluşturulması gerekiyor
- **Öneri:**
  - Recurring meeting pattern'i (günlük, haftalık, aylık) eklenebilir
  - Seri rezervasyon oluşturma özelliği geliştirilebilir

### 🟢 Düşük Öncelikli İyileştirmeler

#### 7. Performans ve Ölçeklenebilirlik
- **Risk:** Tüm rezervasyonlar her seferinde çekilmekte (pagination yok)
- **Öneri:**
  - Tarih aralığı filtresi eklenebilir (son 3 ay, gelecek 1 ay vb.)
  - Sayfalama (pagination) implementasyonu yapılmalı
  - Caching mekanizması düşünülebilir

#### 8. Güvenlik İyileştirmeleri
- **Risk:** UserId int olarak alınıyor ancak ASP.NET Identity string UserId kullanıyor
- **Öneri:**
  - `CreateReservation` metodunda userId parametresi nullable int yerine Identity'den alınmalı
  - XSS koruması için input sanitization eklenmeli
  - Rate limiting düşünülmeli (spam rezervasyon önleme)

#### 9. UX İyileştirmeleri
- **Risk:** Sadece ay görünümü var, haftalık veya günlük görünüm yok
- **Öneri:**
  - Takvim görünümü çeşitlendirilebilir (FullCalendar.js kullanılabilir)
  - Drag & drop ile rezervasyon değiştirme özelliği eklenebilir
  - Toplantı salonu doluluk oranı görselleştirmesi yapılabilir

### 📋 Acil Aksiyonlar (Öncelik Sırası)

1. ✅ **Çakışma kontrolü var** - Zaten implement edilmiş
2. 🔴 **Kullanıcı seçim sistemi** - Hemen geliştirilmeli
3. 🔴 **Bildirim sistemi** - Kritik özellik, acil yapılmalı
4. 🟡 **Toplantı notları modülü** - Orta vadede eklenm
eli
5. 🟡 **Rezervasyon iptal/düzenleme UI** - Kullanıcı deneyimi için önemli
6. 🟢 **Tekrarlayan toplantılar** - Nice-to-have özellik

### 💡 Teknik Borç Notları
- `Reservation` modelinde `UserId` int olarak tanımlı, ASP.NET Identity ile tutarsızlık var
- Frontend'de katılımcılar string olarak tutulmakta, relational yapı kurulmalı (`ReservationAttendees` junction table)
- Email/notification altyapısı için `System.Net.Mail` veya SendGrid gibi servis entegrasyonu yapılmalı