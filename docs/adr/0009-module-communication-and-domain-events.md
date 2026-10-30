# ADR-0009: Modüller arası iletişimde public sözleşmeler kullan; domain event'lerini aynı transaction içinde işle

> Repodaki yeri: `docs/adr/0009-module-communication-and-domain-events.md`

| Alan | Değer |
| --- | --- |
| Durum | `Accepted` |
| Tarih | 2026-10-27 |
| Karar veren | Batuhan Tümer |
| İlgili kararlar | DEC-039 (genişletir), DEC-046 |
| İlgili gereksinimler | US-01, US-08, US-11, US-15 |

## Bağlam

Mimariye genel bakış dokümanında (P0-16) istek akışlarını uçtan uca çizerken, daha önce aldığımız iki kararın birlikte çalışmadığı iki durum ortaya çıktı.

**Problem 1: Domain event'leri commit'ten sonra işlenirse tutarlılık bozulabilir.**

Domain modelinde (§6) event'lerin "transaction commit edildikten sonra" yayınlanacağını yazmıştık. Bir öğrenci ayrıldı olarak işaretlendiğinde (US-15) şu olur:

1. `Student` güncellenir ve `StudentLeft` event'i üretilir.
2. Transaction commit edilir: öğrenci artık `Left`.
3. Event işlenir: `Enrollments` modülü öğrencinin aktif kayıtlarını sonlandırmaya çalışır.

Üçüncü adım başarısız olursa (ör. bir eşzamanlılık çakışması veya bir bug), öğrenci `Left` olur ama kayıtları aktif kalır. Gruptaki yeri boşalmaz ve antrenörün yoklama listesinde görünmeye devam eder. Bu tutarsızlığı düzeltecek otomatik bir mekanizma yoktur.

**Problem 2: Atomik olması gereken bir işlem iki modülü birden değiştiriyor.**

US-01'e göre okul ve ilk admini **atomik** olarak oluşturulmalı: e-posta zaten kullanılıyorsa okul da oluşturulmamalı. Ama okul `Tenancy` modülüne, kullanıcı `Identity` modülüne ait ve DEC-039 bir modülün başka bir modülün aggregate'ını değiştirmesini yasaklıyor. Event ile yapılırsa (okul oluşturulur, commit edilir, sonra kullanıcı oluşturulur) işlem atomik olmaz; e-posta çakışırsa geride adminsiz bir okul kalır.

## Karar kriterleri

1. **Tutarlılık:** bir iş işlemi ya tamamen gerçekleşmeli ya da hiç gerçekleşmemeli.
2. **Modül sınırları:** bir modül, başka bir modülün iç yapısını (aggregate'larını, tablolarını) bilmemeli.
3. **Basitlik:** MVP'de tek veritabanımız var; dağıtık sistemlerin karmaşıklığını şimdi getirmemeliyiz.
4. **Gelişebilirlik:** faz 2'de dış sistemlere (e-posta) giden yan etkiler ve faz 5'te ayrı bir servis için yol açık kalmalı.

## Değerlendirilen seçenekler

### Domain event'lerinin ne zaman işleneceği

#### Seçenek A1: Commit'ten sonra, süreç içinde (mevcut plan)

- **Artıları:** Event işleyicisindeki bir hata, asıl işlemi geri almaz.
- **Eksileri:** İşleyici başarısız olursa veri tutarsız kalır (Problem 1). Bunu düzeltmek için tekrar deneme ve kalıcı bir kuyruk gerekir.

#### Seçenek A2: Aynı transaction içinde, commit'ten önce

Handler aggregate'ları değiştirir; `TransactionDecorator`, commit etmeden önce üretilen event'leri toplar ve işleyicilerini aynı transaction içinde çalıştırır. İşleyiciler yeni event'ler üretirse onlar da işlenir. Herhangi bir adım başarısız olursa her şey geri alınır.

- **Artıları:** İşlem ya tamamen gerçekleşir ya da hiç gerçekleşmez. Tek veritabanımız olduğu için bu ücretsiz bir garantidir.
- **Eksileri:** Transaction biraz uzar. Event işleyicisinin içinde dış bir sisteme (e-posta) çağrı yapılırsa ve transaction sonradan geri alınırsa, e-posta gitmiş olur. Bu yüzden bu yöntem sadece veritabanı içi yan etkiler için uygundur.

#### Seçenek A3: Transactional outbox

Event'ler aynı transaction içinde bir "outbox" tablosuna yazılır; ayrı bir arka plan işi onları okuyup işler ve başarısız olanları tekrar dener.

- **Artıları:** Hem tutarlı hem güvenilir; dış sistemlere giden yan etkiler için doğru çözüm. Faz 5'teki ayrı servislere geçiş için de temel oluşturur.
- **Eksileri:** Arka plan işleri ve tekrar deneme altyapısı gerektirir; bunlar faz 2'nin konusu (P2-F2, P2-F3). Veritabanı içi yan etkiler için bile eventual consistency (gecikmeli tutarlılık) getirir.

### Atomik çok modüllü işlemler

#### Seçenek B1: Bir modülün başka bir modülün aggregate'ını doğrudan kullanması

- **Eksileri:** DEC-039'u ihlal eder; modüller birbirinin iç yapısına bağımlı hale gelir.

#### Seçenek B2: Her modülün bir public sözleşme (`Contracts`) yayınlaması

Her modül, başka modüllerin kullanabileceği küçük bir arayüz yayınlar: okuma arayüzleri (ör. `IStudentReader`) ve gerektiğinde senkron komutlar (ör. `IIdentityModule.CreateSchoolAdminAsync`). Arayüzü çağıran modül, hedef modülün aggregate'larını veya tablolarını bilmez; işi hedef modülün kendi kodu yapar. Çağrı, çağıranın transaction'ı içinde çalışır.

- **Artıları:** İşlem atomik kalır. Modül sınırları korunur: her modül kendi aggregate'larını sadece kendisi değiştirir. Faz 5'te bir modül servise ayrıldığında, ayrılacak yüzey (sözleşme) zaten bellidir.
- **Eksileri:** Senkron çağrılar modüller arasında bir bağımlılık yaratır; bağımlılık yönü takip edilmelidir (döngüsel bağımlılık olmamalı).

## Karar

**Domain event'leri için Seçenek A2'yi (aynı transaction içinde, commit'ten önce), atomik çok modüllü işlemler için Seçenek B2'yi (public sözleşmeler) seçiyoruz.** Seçenek A3 (outbox), dış sistemlere giden yan etkiler için faz 2'de eklenecek.

**Domain event'leri:**

- `TransactionDecorator` şu sırayla çalışır: handler'ı çalıştır → değişen aggregate'lardan event'leri topla → her event'in işleyicilerini aynı transaction içinde çalıştır (yeni event'ler üretildikçe tekrarla) → `SaveChanges` → commit.
- Herhangi bir adım başarısız olursa transaction geri alınır ve istemciye hata döner.
- Domain event işleyicileri **sadece veritabanı içi değişiklik** yapar. E-posta, ödeme sağlayıcı gibi dış sistem çağrıları domain event işleyicisinde yapılmaz; faz 2'de outbox üzerinden, commit'ten sonra yapılır.
- Sonsuz döngüye karşı, tek bir işlem içinde işlenecek event turu sayısı sınırlıdır (ör. 10); aşılırsa beklenmeyen hata olarak ele alınır.

**Modüller arası sözleşmeler:**

- Her modül, `Application` katmanında bir `Contracts` klasörü yayınlar. Başka bir modül, o modüle sadece bu klasördeki arayüzler üzerinden erişebilir.
- Sözleşmeler iki türdür:
  - **Okuma arayüzleri:** başka modülün ihtiyaç duyduğu veriyi DTO olarak döner (ör. `IStudentReader.GetEnrollmentInfoAsync` → doğum tarihi, durum). Aggregate dönmez.
  - **Senkron komutlar:** sadece atomik bir iş işleminin gerektirdiği durumlarda (ör. `IIdentityModule.CreateSchoolAdminAsync`, US-01). Varsayılan yol event'lerdir; senkron komut istisnadır ve her biri bu ADR'ın listesine eklenir.
- Modüller arası bağımlılıklar döngüsel olamaz.
- Bu, DEC-039'u genişletir: "Bir modül başka bir modülün aggregate'ını değiştiremez; değişiklikler domain event'leriyle veya hedef modülün yayınladığı sözleşmeyle, okumalar okuma arayüzleriyle yapılır."

**MVP'deki senkron komut listesi:**

| Çağıran | Sözleşme | Neden senkron |
| --- | --- | --- |
| `Tenancy` | `IIdentityModule.CreateSchoolAdminAsync` | US-01: okul ve ilk admin atomik olarak oluşturulmalı |

## Sonuçlar

- **Olumlu:**
  - Öğrencinin ayrılması, grubun arşivlenmesi, antrenörün pasif hale getirilmesi gibi işlemler ve yan etkileri her zaman birlikte gerçekleşir; tutarsız veri kalmaz.
  - Okul ve ilk admini atomik olarak oluşturulur.
  - Modüller birbirinin iç yapısını bilmez; her modülün dışa açık yüzeyi `Contracts` klasöründe tek bir yerde görülür.
  - Faz 5'te bir modül ayrıldığında, sözleşmeleri HTTP veya mesajlaşma ile değiştirilecek yüzey bellidir.
- **Olumsuz:**
  - Transaction'lar, event işleyicilerinin süresi kadar uzar. ADR-0005'teki kilitler de bu süre boyunca tutulur; event işleyicileri hızlı olmalıdır.
  - Bir event işleyicisindeki hata, asıl işlemi de geri alır. Bu, tutarlılık için bilinçli olarak tercih edilir.
  - Senkron sözleşmeler modüller arasında derleme zamanı bağımlılığı yaratır.
- **Riskler ve önlemler:**
  - *Risk:* Bir geliştirici veya agent, domain event işleyicisine e-posta gönderimi ekler. *Önlem:* `CLAUDE.md` kuralı ve mimari testi (domain event işleyicileri dış servis arayüzlerine bağımlı olamaz).
  - *Risk:* Modüller başka modüllerin `Contracts` dışındaki tiplerini kullanır. *Önlem:* mimari testi.

## Uygulama ve doğrulama

- P1-03: `TransactionDecorator`'da event toplama ve işleme döngüsü.
- P1-04: event'lerin aggregate'lardan toplanması.
- P1-07: mimari testleri (modüller arası erişim sadece `Contracts` üzerinden; event işleyicilerinde dış servis bağımlılığı yok; döngüsel modül bağımlılığı yok).
- Entegrasyon testi: event işleyicisi başarısız olduğunda asıl değişikliğin de geri alındığı (ör. US-15'te kayıt sonlandırma başarısız olursa öğrenci `Active` kalır).
- Domain modeli §6 bu karara göre güncellenir.

## Yeniden değerlendirme

- Faz 2'de outbox eklendiğinde, hangi event'lerin transaction içinde, hangilerinin outbox üzerinden işleneceği netleştirilir.
- Faz 5'te bir modül servise ayrıldığında, o modüle ait sözleşmeler ağ üzerinden çağrılara dönüşür ve atomiklik yerine saga gibi telafi edici yöntemler gerekir.

## Kaynaklar

- ADR-0001, ADR-0004, `docs/domain/domain-model.md` §6, `docs/architecture/overview.md`
