# ADR-0005: Kayıt sırasında grup ve öğrenci satırlarını kilitle (pessimistic locking)

> Repodaki yeri: `docs/adr/0005-enrollment-concurrency.md`

| Alan | Değer |
| --- | --- |
| Durum | `Accepted` |
| Tarih | 2026-10-27 |
| Karar veren | Batuhan Tümer |
| İlgili kararlar | DEC-009, DEC-018, DEC-039, DEC-041 |
| İlgili gereksinimler | US-11, US-17, NFR-REL-08 |

## Bağlam

DEC-018 ile `Enrollment`'ı `Group`'tan ayrı bir aggregate yaptık. Bunun bedeli, kayıt kurallarının bir kısmının artık tek bir aggregate'ın içinde kontrol edilemeyecek olması. Bu kurallar, iki istek aynı anda geldiğinde bozulabilir.

**Problem senaryosu (race condition):** 20 kişilik bir grupta 19 aktif kayıt var. İki admin aynı anda iki farklı öğrenciyi kaydediyor.

| Zaman | İstek A | İstek B |
| --- | --- | --- |
| t1 | Aktif kayıtları sayar: 19 | |
| t2 | | Aktif kayıtları sayar: 19 |
| t3 | 19 < 20, kaydı ekler | |
| t4 | | 19 < 20, kaydı ekler |
| Sonuç | Grupta **21** öğrenci | |

Her iki istek de kuralı doğru kontrol etti; ama kontrol ile yazma arasında diğerinin yazdığını göremedi. Bu, "check-then-act" türünden klasik bir yarış durumudur.

Aynı problem üç kuralda var:

1. **Kontenjan** (US-17): iki farklı öğrencinin son boş yere aynı anda kaydedilmesi.
2. **Program çakışması** (US-17): aynı öğrencinin, saatleri çakışan iki farklı gruba aynı anda kaydedilmesi.
3. **Kontenjan düşürme** (US-11): admin grubun kontenjanını düşürürken aynı anda yeni bir kayıt yapılması.

Bir de daha basit bir kural var: **aynı gruba mükerrer kayıt**. Bu, tek bir satıra ilişkin olduğu için bir veritabanı kısıtıyla doğrudan çözülebilir.

Kararı etkileyen gerçekler:

- Kayıt işlemi seyrek yapılır: bir grubun haftada birkaç kaydı olur. Aynı grup için çakışan istekler nadirdir ama mümkündür (ör. sezon başında kayıt yoğunluğu).
- DEC-039'a göre `Enrollments` modülü, `Group` aggregate'ını değiştiremez.
- Veritabanı PostgreSQL ve tek bir veritabanı var (ADR-0003).

## Karar kriterleri

1. **Doğruluk:** kontenjan hiçbir koşulda aşılmamalı.
2. **Basitlik:** anlaşılması, test edilmesi ve hata ayıklaması kolay olmalı.
3. **Modül sınırlarına uyum:** DEC-039'u ihlal etmemeli.
4. **Kullanıcı deneyimi:** gereksiz hatalar ve tekrar denemeler olmamalı.

## Değerlendirilen seçenekler

### Seçenek 1: Optimistic concurrency ve gruptaki sayaç

`Group`'a `ActiveEnrollmentCount` alanı eklenir. Kayıt sırasında bu sayaç artırılır; `Group`'un eşzamanlılık token'ı (`xmin`) sayesinde aynı anda yapılan ikinci değişiklik `DbUpdateConcurrencyException` ile reddedilir ve istek tekrar denenir.

- **Artıları:** Kilit tutulmaz; yaygın ve iyi bilinen bir teknik.
- **Eksileri:** `Enrollments` modülü `Group` aggregate'ını değiştirmek zorunda kalır; **DEC-039'u ihlal eder**. Sayaç, kayıtlar her değiştiğinde (sonlandırma, öğrencinin ayrılması, grubun arşivlenmesi) güncellenmeli; bir yer unutulursa sayaç gerçekle uyuşmaz. Çakışan istek ya kullanıcıya hata olarak döner ya da tekrar deneme mantığı gerekir. Program çakışması kuralını çözmez.

### Seçenek 2: Pessimistic locking (`SELECT ... FOR UPDATE`)

Kayıt transaction'ı, kuralları kontrol etmeden önce grup ve öğrenci satırlarını PostgreSQL'de kilitler. Aynı satırları kilitlemeye çalışan ikinci istek, birincisi bitene kadar bekler; sonra güncel veriyi görür.

- **Artıları:** Doğru ve basit: kontrol ile yazma arasında kimse araya giremez. Grup satırı sadece kilitlenir, değiştirilmez; DEC-039'a uyar. Ek bir sayaç alanı yoktur; kontenjan her zaman gerçek kayıtlardan sayılır. Tekrar deneme mantığı gerekmez. Üç kuralın üçünü de çözer.
- **Eksileri:** PostgreSQL'e özgü SQL gerektirir (EF Core LINQ'da `FOR UPDATE` yoktur); bu bir ham SQL istisnasıdır (DEC-041). Kilitler transaction süresince tutulur; uzun transaction'lar bekleme yaratır. Kilitler farklı sırayla alınırsa deadlock oluşabilir.

### Seçenek 3: Serializable izolasyon seviyesi

Kayıt transaction'ı `SERIALIZABLE` izolasyonla çalışır. PostgreSQL, sonucu seri çalışmayla aynı olmayacak transaction'lardan birini hata ile (`40001`) iptal eder.

- **Artıları:** Genel bir çözüm; kilitlenecek satırları düşünmeye gerek yok.
- **Eksileri:** İptal edilen transaction'ı tekrar denemek için bir mekanizma şarttır. Hangi işlemlerin neden çakıştığı tahmin etmesi ve debug etmesi zor bir konudur. Sadece bu kuralı korumak için gereğinden ağır.

### Seçenek 4: Veritabanı kısıtı

Kontenjan, bir veritabanı kısıtıyla korunur (ör. her grup için önceden "koltuk" satırları oluşturmak veya bir trigger yazmak).

- **Artıları:** Son savunma hattı veritabanı olur.
- **Eksileri:** "Aktif kayıt sayısı ≤ kontenjan" basit bir kısıtla ifade edilemez. Koltuk tablosu veya trigger, modele yapay bir karmaşıklık ekler ve iş kuralını veritabanına taşır. Program çakışmasını çözmez.

### Karşılaştırma

| Kriter | Optimistic + sayaç | Pessimistic lock | Serializable | DB kısıtı |
| --- | --- | --- | --- | --- |
| Doğruluk | İyi | Çok iyi | Çok iyi | İyi (sadece kontenjan) |
| Basitlik | Orta | İyi | Kötü | Kötü |
| Modül sınırlarına uyum | Kötü | Çok iyi | Çok iyi | Orta |
| Kullanıcı deneyimi | Orta (tekrar deneme) | Çok iyi | Orta (tekrar deneme) | İyi |

## Karar

**Seçenek 2'yi seçiyoruz: kayıt sırasında grup ve öğrenci satırları `SELECT ... FOR UPDATE` ile kilitlenir.** Mükerrer kayıt için ayrıca bir veritabanı kısıtı eklenir.

**Kayıt (US-17) akışı, tek bir transaction içinde:**

1. Öğrenci satırını kilitle.
2. Grup satırını kilitle.
3. Kuralları kontrol et (`EnrollmentPolicy`): öğrenci aktif mi, doğum yılı uygun mu, grupta boş yer var mı, mükerrer kayıt var mı, öğrencinin diğer aktif kayıtlarıyla program çakışması var mı?
4. Kaydı ekle ve commit et; kilitler commit ile birlikte bırakılır.

**Kilit sırası her zaman aynıdır: önce öğrenci, sonra grup.** Tüm işlemler kilitleri aynı sırayla aldığı için iki işlem birbirini karşılıklı bekleyemez (deadlock oluşmaz).

**Kontenjan düşürme (US-11)** grup satırını aynı şekilde kilitler, sonra aktif kayıt sayısını kontrol eder. Böylece kontenjan düşürme ile yeni kayıt aynı anda çalışamaz.

**Kilitleme nasıl yapılır:**

- Kilitleme, `Application` katmanında bir arayüzle tanımlanır (ör. `IEnrollmentLocks.LockStudentAndGroupAsync(studentId, groupId)`) ve `Infrastructure`'da ham SQL ile uygulanır.
- SQL, DEC-041'e uygun olarak tenant koşulunu açıkça içerir: `SELECT 1 FROM groups.groups WHERE tenant_id = @tenantId AND id = @groupId FOR UPDATE`.
- Kilit için bekleme süresi sınırlıdır (`lock_timeout` = 5 saniye); süre aşılırsa istek `409 Conflict` ile ve "işlem şu an yapılamıyor, tekrar deneyin" mesajıyla döner.

**Veritabanı düzeyinde ek koruma (NFR-REL-08):**

- Mükerrer kayıt için kısmi benzersiz index (partial unique index): `UNIQUE (tenant_id, student_id, group_id) WHERE status = 'Active'`. Kodda bir hata olsa bile aynı öğrenci aynı gruba iki kez aktif olarak kaydedilemez.

## Sonuçlar

- **Olumlu:**
  - Kontenjan, program çakışması ve kontenjan düşürme kuralları aynı anda gelen isteklerde bile bozulamaz.
  - `Group` aggregate'ına ek alan eklenmez ve `Enrollments` modülü onu değiştirmez; modül sınırları korunur.
  - Kontenjan her zaman gerçek kayıtlardan sayıldığı için senkronize tutulması gereken bir sayaç yoktur.
  - Kullanıcı neredeyse hiçbir zaman hata görmez: ikinci istek birkaç milisaniye bekler ve doğru sonucu alır (ya başarılı olur ya da "grup dolu" der).
- **Olumsuz:**
  - PostgreSQL'e özgü ham SQL; başka bir veritabanına geçişte yeniden yazılmalıdır.
  - Kilitler transaction süresince tutulur; kayıt transaction'ı içinde yavaş bir iş (ör. dış servis çağrısı) yapılırsa diğer istekler bekler.
  - Kilit sırası bir kuraldır ve yeni bir işlem bu satırları kilitleyecekse aynı sıraya uymak zorundadır.
- **Riskler ve önlemler:**
  - *Risk:* Gelecekte başka bir işlem kilitleri ters sırayla alır ve deadlock oluşur. *Önlem:* kilit alma yalnızca `IEnrollmentLocks` üzerinden yapılır; sıra orada sabittir. PostgreSQL deadlock'u tespit edip bir işlemi iptal eder; bu durum loglanır.
  - *Risk:* Kayıt transaction'ı içine yavaş bir iş eklenir. *Önlem:* yan etkiler (ör. faz 2'deki e-posta) domain event'leri ile transaction'dan sonra çalışır.

## Uygulama ve doğrulama

- P1-26 ve P1-20: kilitleme arayüzü, ham SQL uygulaması ve kısmi benzersiz index.
- **Eşzamanlılık testi** (Testcontainers ile gerçek PostgreSQL): 19/20 dolu bir gruba aynı anda iki farklı öğrenci kaydı gönderilir; tam olarak birinin başarılı olduğu, grubun 20 kişide kaldığı doğrulanır. Aynı test, program çakışması ve kontenjan düşürme senaryoları için de yazılır.
- Kod incelemesinde: kayıt transaction'ı içinde dış servis çağrısı yoktur.

## Yeniden değerlendirme

- Kayıt işlemleri çok sık hale gelir ve kilit beklemeleri ölçülebilir bir gecikme yaratırsa.
- Bir modül servise çıkarılır ve grup ile kayıtlar farklı veritabanlarına taşınırsa (faz 5); o zaman dağıtık bir yaklaşım (rezervasyon, saga) gerekir.

## Kaynaklar

- `docs/domain/domain-model.md` §4.8, `docs/product/user-stories.md` (US-11, US-17)
- PostgreSQL dokümantasyonu: *Explicit Locking* (row-level locks), *Transaction Isolation*
