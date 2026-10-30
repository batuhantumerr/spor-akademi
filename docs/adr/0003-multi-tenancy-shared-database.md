# ADR-0003: Multi-tenancy için paylaşımlı veritabanı ve TenantId kullan

> Repodaki yeri: `docs/adr/0003-multi-tenancy-shared-database.md`

| Alan | Değer |
| --- | --- |
| Durum | `Accepted` |
| Tarih | 2026-10-27 |
| Karar veren | Batuhan Tümer |
| İlgili kararlar | DEC-001, DEC-004, DEC-026, DEC-034, DEC-038 |
| İlgili gereksinimler | US-02, NFR-SEC-08, NFR-SCL-02, NFR-CST-01 |

## Bağlam

Platform ilk günden birden fazla okula hizmet verecek (DEC-001). Her okulun verisi, diğer okullardan tamamen izole olmalı; içinde çocuklara ait kişisel veriler var ve bir okulun başka bir okulun öğrencilerini görmesi kabul edilemez bir güvenlik ihlalidir (NFR-SEC-08).

Bu ADR iki soruyu cevaplar:

1. **Veri nasıl ayrılır?** (izolasyon modeli)
2. **Bir istek hangi okula ait olduğunu nasıl bilir?** (tenant çözümleme)

Kararı etkileyen gerçekler:

- Hedef yük: 50 okul, 5.000 öğrenci (NFR §1). Okullar küçük ve benzer büyüklükte.
- Bütçe aylık 20 USD ve tek sunucu (DEC-034).
- Yeni bir okul eklemek deploy veya yapılandırma gerektirmemeli (NFR-SCL-02).
- Tek bir geliştirici var; migration'lar ve operasyon basit olmalı.

## Karar kriterleri

1. **İzolasyonun güvenilirliği:** bir hata, veri sızıntısına dönüşmemeli; dönüşürse yakalanmalı.
2. **Operasyonel basitlik:** migration, yedekleme ve izleme tek bir yerden yapılabilmeli.
3. **Maliyet:** tek sunucu ve tek veritabanı.
4. **Yeni okul ekleme kolaylığı:** okul eklemek bir veri işlemi olmalı, altyapı işlemi değil.

## Değerlendirilen seçenekler

### İzolasyon modeli

#### Seçenek A1: Paylaşımlı veritabanı, `TenantId` sütunu

Tüm okullar aynı tabloları kullanır; tenant'a bağlı her satırda bir `TenantId` vardır ve her sorgu bu sütuna göre filtrelenir.

- **Artıları:** En basit ve en ucuz model. Tek migration, tek yedek. Yeni okul eklemek bir `INSERT` işlemidir. Okullar arası raporlama (platform düzeyinde) kolaydır.
- **Eksileri:** İzolasyon tamamen uygulamaya bağlıdır; unutulan tek bir filtre veri sızıntısı demektir. Bir okulun verisini ayrıca yedeklemek veya taşımak zordur. "Gürültülü komşu" problemi: bir okulun ağır sorguları diğerlerini etkileyebilir.

#### Seçenek A2: Tenant başına şema

Her okulun PostgreSQL'de kendi şeması vardır; tablolar her şemada tekrarlanır.

- **Artıları:** Daha güçlü izolasyon; bir okulun verisi fiziksel olarak ayrı tablolardadır. Bir okulun verisini yedeklemek veya silmek kolaydır.
- **Eksileri:** Her migration her şemada ayrı ayrı çalışmalıdır; 50 okul için 50 kez. Yeni okul eklemek bir şema oluşturma işlemidir. EF Core'da şemayı istek bazında değiştirmek karmaşıktır. Ayrıca DEC-038'deki "modül başına şema" yapısıyla çakışır.

#### Seçenek A3: Tenant başına veritabanı

Her okulun ayrı bir veritabanı vardır.

- **Artıları:** En güçlü izolasyon. Okul bazında yedekleme, taşıma ve ölçekleme.
- **Eksileri:** En pahalı ve en karmaşık seçenek. Bağlantı yönetimi, migration ve izleme okul sayısıyla çarpılır. Tek sunucu ve 20 USD bütçeyle uyumsuz. Büyük kurumsal müşterilerin bulunduğu sistemler için uygundur; küçük spor okulları için değil.

### Tenant çözümleme

#### Seçenek B1: JWT içindeki `tenant_id` claim'i

Kullanıcı giriş yaptığında, okulunun ID'si token'a yazılır. Her istekte tenant token'dan okunur.

- **Artıları:** Token imzalı olduğu için istemci tarafından değiştirilemez. Ek bir veritabanı sorgusu gerekmez. DEC-004 (her kullanıcı tek bir okula bağlı) ile doğal olarak uyumlu.
- **Eksileri:** Bir kullanıcının birden fazla okula erişmesi gerekirse yetersiz kalır (şu an bir ihtiyaç değil).

#### Seçenek B2: Alt alan adı (`ornekokul.sporakademi.com`)

- **Artıları:** Kullanıcı hangi okulda olduğunu adres çubuğunda görür; giriş öncesi okula özel sayfalar mümkün olur.
- **Eksileri:** DNS ve HTTPS sertifikası yapılandırması gerektirir. Tek başına güvenli değildir; kullanıcının o okula ait olduğu yine token'dan doğrulanmalıdır. Faz 3'te değerlendirilecek (P3-F7).

#### Seçenek B3: HTTP başlığı (`X-Tenant-Id`)

- **Artıları:** Basit.
- **Eksileri:** İstemci başlığı değiştirebilir; token ile ayrıca karşılaştırılmazsa güvenlik açığıdır. Token zaten bu bilgiyi taşıdığı için gereksizdir.

### Karşılaştırma (izolasyon)

| Kriter | Paylaşımlı DB + TenantId | Tenant başına şema | Tenant başına DB |
| --- | --- | --- | --- |
| İzolasyonun güvenilirliği | Orta (önlemlerle iyi) | İyi | Çok iyi |
| Operasyonel basitlik | Çok iyi | Kötü | Çok kötü |
| Maliyet | Çok iyi | İyi | Kötü |
| Yeni okul ekleme | Çok iyi | Orta | Kötü |

## Karar

**İzolasyon için Seçenek A1 (paylaşımlı veritabanı + `TenantId`), çözümleme için Seçenek B1'i (JWT `tenant_id` claim'i) seçiyoruz.** A1'in en büyük zayıflığı olan "unutulan filtre" riskini, birbirini tamamlayan beş savunma katmanıyla kapatıyoruz:

**1. Okuma: otomatik filtre.**
Tenant'a bağlı her entity `ITenantScoped` arayüzünü uygular. `DbContext`, bu arayüzü uygulayan her entity'ye otomatik olarak `TenantId == mevcut tenant` global query filter'ını ekler. Geliştirici veya agent filtreyi elle yazmaz, bu yüzden unutamaz.

**2. Yazma: otomatik atama ve koruma.**
Yeni bir entity kaydedilirken `TenantId`, bir EF Core interceptor'ı tarafından mevcut tenant'tan otomatik olarak atanır. Bir entity farklı bir tenant'a ait olarak kaydedilmeye veya değiştirilmeye çalışılırsa işlem bir hata ile durdurulur.

**3. Veritabanı: okullar arası referansın imkansız olması.**
Tenant'a bağlı tablolar arasındaki foreign key'ler `TenantId`'yi de içerir. Örneğin `enrollments` tablosu, öğrenciye `(tenant_id, student_id)` → `students(tenant_id, id)` şeklinde bağlanır. Böylece A okuluna ait bir kayıt, B okulunun öğrencisini gösteremez; kodda bir hata olsa bile veritabanı bunu reddeder. (US-02'deki "başka okula ait varlığa referans" kriteri.)

**4. Benzersizlik okul bazında.**
Okul içinde benzersiz olması gereken alanlar `TenantId` ile birlikte index'lenir: `(tenant_id, name)` branş adı için, `(tenant_id, phone)` veli telefonu için. Tek istisna, platform genelinde benzersiz olan kullanıcı e-postasıdır (DEC-004).

**5. Testler.**
Tenant izolasyon test paketi (P1-35) her endpoint'i başka bir okulun kullanıcısıyla çağırır. Mimari testi, `ITenantScoped` uygulamayan ama tenant'a bağlı olması gereken entity'leri yakalar (US-02).

**Ek kurallar:**

- Tenant bağlamı (`ITenantContext`) istek başına oluşturulur ve sadece doğrulanmış JWT'den okunur; istemcinin gönderdiği hiçbir başlık veya parametre dikkate alınmaz.
- Başka bir okula ait bir kayıt istendiğinde yanıt `403` değil `404`'tür; kaydın varlığı bile açığa çıkmaz.
- **Platform Admin:** `tenant_id` claim'i yoktur. Filtreleri devre dışı bırakan `IgnoreQueryFilters()` çağrısı sadece `Tenancy` modülünde kullanılabilir; bu kural bir mimari testiyle korunur.
- **Ham SQL:** global query filter'lar ham SQL sorgularına uygulanmaz. Ham SQL kullanımı istisnadır; kullanılırsa tenant koşulunu açıkça içermeli ve kod incelemesinde ayrıca kontrol edilmelidir.
- **Arka plan işleri (faz 2):** HTTP isteği olmadığı için tenant bağlamını kendileri, her okul için ayrı ayrı açıkça kurmalıdır.
- **Cache (faz 2):** her cache anahtarı tenant ID'sini içermelidir.
- **Loglar:** her log kaydı tenant ID'sini içerir (NFR-OBS-01).

## Sonuçlar

- **Olumlu:**
  - Tek veritabanı, tek migration, tek yedek: tek geliştirici ve tek sunucu için en basit model.
  - Yeni bir okul eklemek bir veri işlemidir (NFR-SCL-02).
  - Filtreler otomatik olduğu için, günlük geliştirmede tenant'ı düşünmek gerekmez; risk, bilinçli istisnalarda (Platform Admin, ham SQL, arka plan işleri) toplanır.
  - Veritabanı düzeyindeki bileşik foreign key'ler, uygulamadaki bir hatanın bile okullar arası veri karışmasına yol açmasını engeller.
- **Olumsuz:**
  - Bileşik foreign key'ler EF Core yapılandırmasını karmaşıklaştırır: her ana tabloda `(tenant_id, id)` üzerinde ek bir benzersiz anahtar (alternate key) gerekir.
  - Okuma tarafındaki izolasyon hâlâ uygulama koduna bağlıdır; veritabanı düzeyinde ikinci bir okuma koruması (Row Level Security) faz 4'e kadar yoktur.
  - Tek bir okulun verisini ayrı yedeklemek veya başka bir sisteme taşımak, tüm tablolardan filtrelenerek yapılmalıdır.
  - Bir okulun yoğun kullanımı diğer okulları etkileyebilir; hedef yükte bu beklenen bir sorun değildir.
- **Riskler ve önlemler:**
  - *Risk:* Yeni bir entity `ITenantScoped` olmadan eklenir. *Önlem:* mimari testi ve `CLAUDE.md` kuralı.
  - *Risk:* Bir handler gereksiz yere filtreleri devre dışı bırakır. *Önlem:* `IgnoreQueryFilters()` kullanımını `Tenancy` modülüyle sınırlayan mimari testi.
  - *Risk:* Ham SQL filtreyi atlar. *Önlem:* ham SQL kullanımı inceleme kontrol listesinde ayrı bir madde; faz 4'te Row Level Security.

## Uygulama ve doğrulama

- P1-05: `ITenantScoped`, `ITenantContext`, global query filter'lar, yazma interceptor'ı.
- P1-07: mimari testleri (tenant'a bağlı entity'ler, `IgnoreQueryFilters()` kullanım kısıtı).
- P1-08: iki tenant'lı test fixture'ı.
- P1-35: her endpoint için tenant izolasyon testi.
- P4-F2: PostgreSQL Row Level Security ile ikinci katman.

## Yeniden değerlendirme

- Bir kullanıcının birden fazla okula erişmesi gerektiğinde (DEC-004 değişirse), tenant çözümleme yöntemi yeniden ele alınmalı.
- Büyük bir okul, verisinin fiziksel olarak ayrı tutulmasını sözleşmeyle talep ederse.
- Bir okulun yükü diğerlerini ölçülebilir biçimde etkilemeye başlarsa.

## Kaynaklar

- `docs/product/user-stories.md` (US-02), `docs/product/nfr.md` (NFR-SEC-08), `docs/domain/domain-model.md` §4
- Microsoft Learn, EF Core: *Global Query Filters* ve *Multi-tenancy*
