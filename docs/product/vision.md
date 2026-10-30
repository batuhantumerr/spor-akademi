# Ürün Vizyonu — Spor Akademisi Yönetim Platformu

> Durum: Onaylandı v1.1 · Repodaki yeri: `docs/product/vision.md`

## 1. Bu proje neden var

Bu projenin iki amacı var, önem sırasına göre:

1. **Öğrenme.** İyi yetişmiş bir full stack geliştirici olmak için bilinçli olarak tasarlanmış bir pratik projesi: mimari, temiz kod, test, güvenlik, deployment ve yapay zeka destekli geliştirme; uçtan uca uygulanmış ve mülakatlarda anlatılabilir şekilde.
2. **Ürün.** Yine de gerçek bir ürün gibi davranmalı: gerçek gereksinimler, gerçek kısıtlar ve gerçek kullanıcılar düşünülerek. Kararlar, sanki production'a çıkacakmış gibi alınır.

İkisi çatıştığında, aktarılabilir bir kavramı öğreten seçeneği tercih ederiz; ama hiçbir teknolojiyi sadece popüler olduğu için seçmeyiz.

## 2. Problem tanımı

Küçük ve orta ölçekli spor okulları (futbol, yüzme, basketbol vb.) öğrenci kaydını, grup programlarını, yoklamayı ve aylık aidatları Excel, kağıt ve mesajlaşma grupları ile yönetiyor. Bu durum şunlara yol açıyor:

- tek bir doğru bilgi kaynağı olmadığı için grupların kontenjanı aşılıyor,
- ödenmemiş aidatlar aylarca fark edilmiyor,
- antrenörler ile okul ofisi arasında bilgi kayboluyor,
- geçmiş tutulmuyor: kimse "bu öğrenci geçen dönem ne sıklıkla antrenmana geldi?" sorusuna cevap veremiyor.

## 3. Vizyon

Her spor okulunun branşlarını, tesislerini, gruplarını, öğrencilerini ve yoklamalarını tek bir yerden yönettiği; her okulun verisinin diğer tüm okullardan tamamen izole olduğu, çok kiracılı (multi-tenant) bir web platformu.

## 4. Roller

| Rol | Kapsam | Ana sorumluluklar (MVP) |
| --- | --- | --- |
| Platform Admin | Tüm platform; hiçbir okula bağlı değil | Okulları (tenant) ve ilk Okul Adminlerini oluşturur |
| Okul Admini | Tek bir okul | Branşları, tesisleri, grupları, antrenörleri, öğrencileri ve velileri yönetir; aidat ödemelerini kaydeder |
| Antrenör | Tek bir okul, sadece kendi grupları | Atandığı grupları ve programlarını görür; yoklama alır |
| Veli | Tek bir okul | MVP'de giriş yapmaz. Reşit olmayan öğrencilere bağlı bir iletişim kişisi olarak saklanır. Faz 3'te giriş yetkisi kazanır |

**Öğrenciler kullanıcı değil, profildir.** Öğrenci sisteme giriş yapmaz. Öğrenciler çocuk veya yetişkin olabilir: reşit olmayan bir öğrenci en az bir veliye bağlıdır; yetişkin bir öğrencinin velisi olması gerekmez, kendi iletişim bilgileri tutulur. Her öğrencinin, veli olması gerekmeyen bir acil durum kişisi vardır. Bu yaklaşım, çocukların kişisel verilerini kimlik doğrulama sisteminin dışında tutar ve modeli sadeleştirir. (Bkz. ADR-0002.)

## 5. MVP kapsamı (v0.1.0)

### Must (olmazsa olmaz)

- Platform Admin bir okul (tenant) ve o okulun ilk Okul Adminini oluşturabilir.
- JWT access ve refresh token ile e-posta + şifre girişi.
- Rol bazlı yetkilendirme (Platform Admin, Okul Admini, Antrenör).
- Sıkı tenant izolasyonu: kullanıcılar her zaman sadece kendi okullarının verisini görür.
- Branş yönetimi (ör. Futbol, Yüzme).
- Adres ve kapasite bilgili tesis yönetimi (ör. A Sahası, 1 Numaralı Havuz).
- Antrenör yönetimi (Okul Admini antrenör hesaplarını oluşturur).
- Grup yönetimi: branş, doğum yılı aralığı, kontenjan, atanan antrenör, tesis, haftalık program.
- Öğrenci ve veli kaydı; çocuk ve yetişkin öğrenciler, her öğrenci için bir acil durum kişisi.
- Bir öğrencinin bir veya daha fazla gruba kontenjan, doğum yılı aralığı ve program çakışması kontrolleriyle kaydedilmesi.
- Ayrılma tarihli öğrenci yaşam döngüsü durumu (`Active`, `Left`).
- Antrenörün kendi grubunun bir antrenmanı için yoklama alması.
- Öğrenci ve grup bazında yoklama geçmişi.

### Should (olsa iyi olur)

- Grup başına aylık aidat tanımı.
- Elle ödeme kaydı (nakit / havale).
- Ödenmemiş aidatı olan öğrencilerin listesi.

### Could (vakit kalırsa)

- Bir gruba duyuru (MVP'de antrenörler görür).

### Won't (MVP'de yok)

| Özellik | Planlanan faz |
| --- | --- |
| Online ödeme (ödeme sağlayıcı entegrasyonu) | Faz 2 |
| E-posta / SMS bildirimleri ve hatırlatmalar | Faz 2 |
| Belge yükleme (sağlık raporu, fotoğraf) | Faz 2 |
| Veli girişi ve veli uygulaması (React PWA) | Faz 3 |
| Yetişkin öğrencilerin girişi | Faz 3 (değerlendirilecek) |
| Gerçek zamanlı özellikler | Faz 3 |
| Okul içinde şube kavramı | Henüz planlanmadı |
| Bir kullanıcının birden fazla okula bağlı olması | Henüz planlanmadı |
| Okul başına alt alan adı | Faz 3 (değerlendirilecek) |
| Native mobil uygulama | Planlanmadı |

## 6. Erken alınan temel kararlar

Burada özetlenir, ayrıntıları ADR olarak belgelenir:

- **ADR-0001** — Clean Architecture katmanlarıyla modüler monolit.
- **ADR-0002** — Öğrenciler velilere bağlı profillerdir, kullanıcı değildir.
- **ADR-0003** — Paylaşımlı veritabanı ve `TenantId` sütunuyla multi-tenancy; EF Core global query filter'ları ile uygulanır ve tenant izolasyon testleriyle güvence altına alınır. Tenant, JWT'deki `tenant_id` claim'inden çözülür.
- **ADR-0004** — MediatR / AutoMapper yok: doğrudan enjekte edilen command/query handler'ları, decorator'lar ve elle mapping.

## 7. MVP başarı kriterleri

v0.1.0 şu koşullar sağlandığında tamamlanmış sayılır:

- İki farklı okul oluşturulabilir ve aynı anda kullanılabilir; otomatik testler hiçbirinin diğerinin verisine erişemediğini kanıtlar.
- Bir Okul Admini eksiksiz bir grup kurabilir (branş, tesis, antrenör, program) ve kontenjanı aşmadan öğrenci kaydedebilir.
- Bir Antrenör giriş yapabilir, sadece kendi gruplarını görür ve yoklama alabilir.
- Uygulama production'da HTTPS arkasında çalışır ve CI/CD pipeline'ı ile otomatik deploy edilir.
- Her Must kullanıcı hikayesinin geçen kabul testleri vardır.

## 8. Yol haritası

| Faz | Odak |
| --- | --- |
| Faz 0 | Planlama, dokümantasyon, repo, production'da çalışan walking skeleton |
| Faz 1 | MVP (bu doküman) |
| Faz 2 | Derinlemesine aidat yönetimi, online ödeme, arka plan işleri, bildirimler, dosya depolama, cache |
| Faz 3 | Veli uygulaması (React PWA), gerçek zamanlı özellikler, dashboard'lar, çoklu dil |
| Faz 4 | Production sağlamlaştırma: güvenlik, audit log, rate limiting, gözlemlenebilirlik, yük testi |
| Faz 5 | Ölçek ve mimari: event-driven mesajlaşma, bir modülün servise ayrılması, bulut ve IaC |

## 9. Netleşen kararlar

### 9.1 Veli verisine erişim (veri minimizasyonu)

- Veli bilgilerini (ad, e-posta, telefon, adres) sadece Okul Admini görebilir.
- Antrenör, sadece kendi gruplarındaki öğrenciler için acil durum kişisinin telefon numarasını görebilir. Velilere ve öğrencinin kendi iletişim bilgilerine ait başka hiçbir bilgi antrenörlere görünmez. Bu, sadece rol kontrolüyle değil, alan bazlı (field-level) yetkilendirme ile uygulanır.

### 9.2 Veri saklama (KVKK)

Kişisel veriler süresiz saklanmaz. KVKK'ya göre işleme amacı ortadan kalktığında ve saklamayı gerektiren yasal bir yükümlülük yoksa, veri silinmeli, yok edilmeli veya anonim hale getirilmelidir.

| Veri | Kural |
| --- | --- |
| Öğrenci ve veli kişisel verileri | Öğrencinin durumu `Left` olduktan sonra, ayarlanabilir bir süre geçince anonimleştirilir |
| Yoklama kayıtları | Saklanır, ancak anonimleştirme ile kişiyle bağlantısı koparılır; böylece istatistikler korunur |
| Ödeme kayıtları | Yasal olarak gereken süre boyunca saklanır, sonra anonimleştirilir |

- Saklama süreleri koda gömülmez, okul bazında ayarlanabilir. Gerçek kullanımdan önce kesin yasal süreler bir hukukçu ile netleştirilmelidir.
- **MVP:** sadece model (`Status`, `LeftAt`) uygulanır.
- **Faz 2:** zamanlanmış bir arka plan işi anonimleştirmeyi yapar ve her çalışmayı loglar.

### 9.3 Birden fazla gruba kayıt

Bir öğrenci aynı anda birden fazla gruba kayıtlı olabilir; bu, bir `Enrollment` varlığı ile modellenir. Kurallar:

- Bir öğrencinin aynı grupta iki aktif kaydı olamaz.
- Bir öğrenci, haftalık antrenman saatleri çakışan iki gruba kaydedilemez.
- Kontenjan ve doğum yılı aralığı kontrolleri her kayıt için ayrı ayrı uygulanır.
