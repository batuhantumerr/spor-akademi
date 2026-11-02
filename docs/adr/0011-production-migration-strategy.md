# ADR-0011: Migration'ları uygulama başlarken değil, deploy'da tek seferlik bir bundle ile uygula

> Repodaki yeri: `docs/adr/0011-production-migration-strategy.md`

| Alan | Değer |
| --- | --- |
| Durum | `Accepted` |
| Tarih | 2026-11-02 |
| Karar veren | Batuhan Tümer |
| İlgili kararlar | DEC-078, DEC-038, ADR-0001 |
| İlgili gereksinimler | NFR-SEC-10, NFR-REL-02, NFR-REL-06, NFR-REL-07 |

## Bağlam

P0-25'te veritabanında iki kullanıcı oluşturduk (NFR-SEC-10):

- `spor_akademi_migrator`: veritabanının sahibi; şema ve tablo oluşturur, migration'ları çalıştırır.
- `spor_akademi_app`: API'nin kullandığı kullanıcı; veri okur ve yazar, ama şemayı değiştiremez.

Bu ayrımın amacı, API'de bir açık bulunsa bile saldırganın tabloları silememesi veya değiştirememesidir. Ancak ayrımın bir sonucu var: EF Core'un en bilinen kullanımı olan "uygulama başlarken `Database.Migrate()` çağır" yöntemi artık mümkün değil, çünkü uygulama kullanıcısının buna yetkisi yok. Production'da migration'ların nasıl uygulanacağına P0-31'deki CD pipeline'ı yazılmadan önce karar vermemiz gerekiyor.

Kararı etkileyen gerçekler:

- **Tek sunucu, tek veritabanı** (ADR-0001, P0-30). Aynı anda tek bir API sürümü çalışır.
- **Deploy kuralları:** production deploy'ları elle onaylanır, hafta içi 09:00–14:00 arasında yapılır ve kesinti 1 dakikayı geçmez (NFR-REL-02). Her migration'dan önce yedek alınır (NFR-REL-06). Deploy'dan sonra sağlık kontrolü yapılır (NFR-REL-07).
- **İnceleme kuralı:** üretilen SQL her PR'da incelenir (CONTRIBUTING §7).
- **Microsoft'un önerisi** ("Applying Migrations"):
  - Otomatik deploy için migration bundle, SQL'in incelenmesi gerekiyorsa SQL script önerilir.
  - Uygulama başlarken migration yapmak, basit deploy'u tercih eden uygulamalar için kabul edilebilir bulunuyor. EF Core 9'dan beri `Migrate()` veritabanı genelinde bir kilit aldığından, birden fazla örneğin aynı anda migration çalıştırması artık sorun değil. Ancak inceleme, en az yetkili kimlik bilgisi veya koordineli geçiş önemliyse ayrı bir migration adımı tercih ediliyor.
  - Deploy için şemayı değiştirme yetkisi olan ayrı bir kimlik kullanılması önerilir. Uygulamanın kimliği sadece veri okuyup yazabilmelidir.
  - Bundle tek seferlik bir deploy işi olarak çalıştırılmalıdır. Uygulama imajının başlangıç komutundan çalıştırılmamalıdır.
  - Production bağlantı bilgisi bundle'ın içine gömülmez; deploy sisteminin gizli bilgi deposundan verilir.
- **İki kavram:**
  - **Migration bundle:** `dotnet ef migrations bundle` ile üretilen tek bir çalıştırılabilir dosya (`efbundle`). İçinde migration'lar vardır. Çalıştığı makinede .NET SDK'sı veya kaynak kod gerekmez. Bağlantı bilgisini parametre ya da yapılandırmayla alır.
  - **Idempotent SQL script:** `dotnet ef migrations script --idempotent` ile üretilir. Her migration'ı "geçmiş tablosunda yoksa uygula" koşuluyla sarar, bu yüzden veritabanı hangi migration'da olursa olsun güvenle çalıştırılabilir.

## Karar kriterleri

1. **En az yetki:** API süreci şemayı değiştiren bir kimlik bilgisine hiçbir zaman sahip olmamalı.
2. **Güvenli uygulama:** migration'lar tutarlı bir şekilde, aynı anda tek bir kez ve hatada öngörülebilir davranışla uygulanmalı.
3. **İncelenebilirlik:** veritabanında çalışacak SQL, merge'den önce bir insan tarafından okunabilmeli.
4. **Geri dönülebilirlik:** başarısız bir deploy'dan sonra veri kaybetmeden geri dönülebilmeli.
5. **Basitlik:** tek geliştiricinin yönettiği tek bir sunucuda anlaşılır ve bakımı kolay olmalı.

## Değerlendirilen seçenekler

### Seçenek 1: Uygulama başlarken `Migrate()`, uygulama kullanıcısıyla

- **Artıları:** En basit yöntem; ayrı bir deploy adımı gerekmez.
- **Eksileri:** **Uygulanamaz.** Uygulama kullanıcısı şemayı değiştiremez (NFR-SEC-10).

### Seçenek 2: Uygulama başlarken `Migrate()`, uygulamaya migrator şifresi de verilerek

- **Artıları:** Basit; ayrı bir deploy adımı gerekmez.
- **Eksileri:**
  - API süreci şemayı değiştirebilen bir kimlik bilgisi taşır; iki kullanıcıya ayırmanın anlamı kalmaz.
  - Migration hata verirse uygulama ayağa kalkmaz.
  - SQL, çalıştırılmadan önce incelenemez.
  - Microsoft, en az yetki önemliyse ayrı bir migration adımını tercih ediyor.

### Seçenek 3: Idempotent SQL script, `psql` ile uygulanır

Script CI'da üretilir; CD'de `psql` ile migrator kullanıcısıyla çalıştırılır.

- **Artıları:**
  - İncelenen SQL ile çalışan SQL birebir aynı.
  - Sunucuda .NET'e gerek yok.
- **Eksileri:**
  - EF Core'un migration kilidi devreye girmez; aynı anda tek bir kez çalışmayı biz garanti etmeliyiz.
  - Hata ve transaction davranışı `psql`'in ayarlarına bağlı. Microsoft bu yöntemde transaction davranışının "tutarsız ve bazen beklenmedik" olabileceğini belirtiyor.

### Seçenek 4: Sadece migration bundle

Bundle CI'da üretilir; CD'de tek seferlik bir iş (job) olarak migrator kullanıcısıyla çalışır.

- **Artıları:**
  - Microsoft'un otomatik deploy için önerisi.
  - EF Core'un migration kilidi ve tutarlı transaction davranışı.
  - Sunucuda SDK gerekmez.
- **Eksileri:**
  - Bundle bir çalıştırılabilir dosya. Çalıştıracağı SQL'i doğrudan okumak mümkün değil.

### Seçenek 5: PR'da SQL incelemesi ve CD'de migration bundle (karma)

- PR'da `migrations script` ile üretilen SQL incelenir; bu, CONTRIBUTING §7'de zaten var.
- CD'de seçenek 4'teki bundle tek seferlik bir iş olarak çalışır.

- **Artıları:**
  - Seçenek 4'ün güvenli uygulaması ile seçenek 3'ün incelenebilirliği bir arada.
  - Yeni bir süreç gerekmiyor; inceleme kuralı zaten var.
- **Eksileri:**
  - İki çıktı var: incelenen SQL ve çalışan bundle. İkisi aynı migration sınıflarından üretildiği için farklı olmaları beklenmez, ama bu bir varsayım; sağlayıcı (Npgsql) ve EF Core sürümü aynı kaldığı sürece geçerli.

### Karşılaştırma

| Kriter | 1 | 2 | 3 | 4 | 5 |
| --- | --- | --- | --- | --- | --- |
| En az yetki | — (uygulanamaz) | Hayır | Evet | Evet | Evet |
| Güvenli uygulama | — | Kısmen (EF kilidi var; hata uygulamanın başlamasını engeller) | Kısmen (kilit yok) | Evet | Evet |
| İncelenebilirlik | — | Hayır | Evet | Hayır | Evet |
| Geri dönülebilirlik | — | Zor (hata uygulamayı durdurur) | Evet (öncesinde yedek) | Evet (öncesinde yedek) | Evet (öncesinde yedek) |
| Basitlik | — | Yüksek | Orta | Orta | Orta |

## Karar

**Seçenek 5'i seçiyoruz.**

1. **Uygulama migration çalıştırmaz.** API'de `Database.Migrate()` veya `Database.EnsureCreated()` çağrılmaz. API, production'da her zaman `spor_akademi_app` ile bağlanır.
2. **Migration'lar her zaman `spor_akademi_migrator` ile çalıştırılır.** Bu kural yerelde (P0-27) ve tüm ortamlarda geçerli. `postgres` gibi bir süper kullanıcıyla çalıştırılırsa oluşan tablolar uygulamaya erişilemez olur.
3. **PR'da inceleme:** migration içeren her PR'da üretilen SQL incelenir (CONTRIBUTING §7). CI, model ile migration'lar arasında fark olup olmadığını `dotnet ef migrations has-pending-model-changes` ile kontrol eder (P0-28).
4. **Bundle bir kez üretilir:** CI, her commit için `linux-x64` hedefli, self-contained bir migration bundle üretir. Staging ve production'da **aynı bundle** kullanılır; production'a, staging'de denenmemiş bir bundle gitmez.
5. **Deploy sırası** (P0-31):
   1. Veritabanı yedeği (NFR-REL-06). Yedek başarısızsa deploy durur.
   2. Bundle, tek seferlik bir iş olarak migrator bağlantı bilgisiyle çalışır. API konteynerinin başlangıç komutu bundle'ı çalıştırmaz.
   3. Bundle başarılıysa yeni API sürümü başlatılır; başarısızsa eski API sürümü çalışmaya devam eder ve deploy durur.
   4. Sağlık kontrolü (NFR-REL-07).
6. **Gizli bilgiler:**
   - Migrator bağlantı bilgisi sadece CD'nin gizli bilgi deposunda (P0-31'de belirlenecek) bulunur ve sadece bundle adımına verilir. API'nin ortamına ve imajına asla girmez.
   - Bağlantı bilgisi loglara yazılmaz.
   - Bundle çalıştırılırken ortam (`ASPNETCORE_ENVIRONMENT`) açıkça belirtilir.
7. **Geriye uyumluluk:**
   - Bundle, yeni API sürümünden önce çalışır. Bu yüzden her migration, **bir önceki API sürümüyle de çalışabilmelidir**. Aksi halde bundle ile yeni sürüm arasındaki sürede, ya da yeni sürüm başarısız olursa, eski sürüm bozulur.
   - Sütun silme, yeniden adlandırma ve tip değiştirme gibi değişiklikler expand–contract ile iki deploy'a bölünür (CONTRIBUTING §7).
8. **Geri alma:**
   - Production'da `Down` migration'ları çalıştırılmaz.
   - Bir migration'ın sorununa çözüm, ileri yönlü yeni bir migration'dır.
   - Veri bozulduysa 5.1'de alınan yedekten geri dönülür.
   - Bu yolun provası P0-32'de yapılır.

Bundle'ın sunucuda tam olarak nasıl çalıştırılacağına (ör. `docker run` ile tek seferlik bir konteyner ya da compose'da ayrı bir servis) P0-31'de, sunucu yapısına (P0-30) göre karar verilir. Bu ADR'ın kuralları her iki durumda da geçerlidir.

## Sonuçlar

- **Olumlu:**
  - API, şemayı değiştirebilen bir kimlik bilgisini hiçbir zaman taşımaz.
  - Migration hatası, çalışan uygulamayı durdurmaz.
  - İncelenen SQL ile çalışan migration'lar aynı kaynaktan gelir.
  - Staging, production deploy'unun provası olur.
- **Olumsuz:**
  - CD pipeline'ında ayrı bir adım ve ayrı bir çıktı (bundle) var.
  - Her migration'ın bir önceki sürümle uyumlu olması gerekiyor. Bu, bazı değişiklikleri iki PR'a bölmeyi gerektirir.
  - `Down` migration'ları production'da kullanılmadığı için geri dönüş yedekten yapılır. Yedeğin alındığı an ile geri dönüş arasında yazılan veri kaybolur (NFR-REL-04).
- **Riskler ve önlemler:**
  - *İncelenen SQL ile bundle'ın farklı olması:* ikisi aynı migration'lardan ve aynı paket sürümlerinden üretilir. CI, bundle'ı ürettiği commit'te SQL script'i de çıktı olarak saklayabilir (P0-31'de değerlendirilecek).
  - *Migration'ın deploy kesinti süresini aşması (NFR-REL-02):* uzun sürecek veri dönüşümleri ayrı bir migration'a veya arka plan işine bölünür; PR'da süre tahmini belirtilir.
  - *Migration'ın yanlışlıkla `postgres` ile çalıştırılması:* yerelde `scripts/ef.sh` (P0-27), sunucularda CD sadece migrator bilgisini kullanır. Uygulamanın tablolara erişememesi, sağlık kontrolünde hemen fark edilir.

## Uygulama ve doğrulama

- **P0-27:** migration'lar yerelde sadece migrator kullanıcısıyla çalıştırılır; kural `CLAUDE.md`'ye eklenir.
- **P0-28:** CI'da `dotnet ef migrations has-pending-model-changes` adımı.
- **P0-31:** CD'de deploy sırası (yedek → bundle → API → sağlık kontrolü); migrator bilgisi sadece bundle adımında.
- **P0-32:** yedekten geri dönüşün provası ve runbook.
- **Kod incelemesi:** API'ye `Migrate()` veya `EnsureCreated()` eklenmesi, PR kontrol listesindeki "Migration varsa üretilen SQL incelendi" maddesiyle birlikte kontrol edilir.

## Yeniden değerlendirme

- Birden fazla API sunucusuna veya kesintisiz deploy'a geçildiğinde (faz 4–5).
- Migration'lar deploy penceresine sığmayacak kadar uzun sürmeye başladığında.
- Yönetilen bir veritabanı hizmetine geçildiğinde ve bu hizmet kendi migration aracını sunduğunda.
- Bundle ile incelenen SQL arasında bir fark bulunursa.

## Kaynaklar

- Microsoft Learn, [Applying Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying) (2026-08-05 tarihli sürüm): strateji tablosu, SQL script'ler, idempotent script'ler, bundle'lar ve tek seferlik deploy işleri, ortam seçimi, uygulama başlarken migration'ın ödünleşimleri, migration kilidi.
- `docs/product/nfr.md`: NFR-SEC-10, NFR-REL-02, NFR-REL-04, NFR-REL-06, NFR-REL-07.
- `CONTRIBUTING.md` §7: migration'ların incelenmesi ve expand–contract.
- `docker/postgres/initdb/10-create-roles.sh`: yerel ortamdaki iki kullanıcı (P0-25).
