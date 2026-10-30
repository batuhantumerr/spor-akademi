# ADR-0008: Test stratejisi olarak domain birim testleri ve gerçek veritabanıyla API entegrasyon testlerini temel al

> Repodaki yeri: `docs/adr/0008-testing-strategy.md`

| Alan | Değer |
| --- | --- |
| Durum | `Accepted` |
| Tarih | 2026-10-27 |
| Karar veren | Batuhan Tümer |
| İlgili kararlar | DEC-025, DEC-037, DEC-040, DEC-049 |
| İlgili gereksinimler | US-02, US-14, US-17, NFR-MNT-02, NFR-MNT-03, NFR-MNT-04, NFR-SEC-08 |

## Bağlam

Kullanıcı hikayelerinde "her kabul kriteri en az bir otomatik teste dönüşür" kuralını koyduk. Ayrıca projede yapay zeka agent'ları kod yazacak; testler, agent'ın yazdığı kodun doğru olduğunu kanıtlamanın ve sonraki değişikliklerde bir şeyin bozulmadığını bilmenin ana yolu.

Bu ADR şu soruları cevaplar:

1. Hangi tür testlerden ne kadar yazılır? (test piramidi)
2. Veritabanına dokunan kod nasıl test edilir: sahte (mock) nesnelerle mi, gerçek veritabanıyla mı?
3. Hangi araçlar kullanılır?
4. Testler CI'da ne zaman çalışır?

Kararı etkileyen gerçekler:

- İş kurallarının bir kısmı domain'de (aggregate invariant'ları), bir kısmı veritabanında (bileşik foreign key'ler, kısmi benzersiz index'ler, `FOR UPDATE` kilitleri, global query filter'lar).
- En kritik iki güvenlik gereksinimi (tenant izolasyonu ve antrenörün veli verisini görmemesi) sadece uçtan uca bir istekte doğrulanabilir.
- CI toplam 10 dakikanın altında kalmalı (NFR-MNT-04).
- Fluent Assertions gibi bazı yaygın test kütüphaneleri 2025'te ticari lisansa geçti.

## Karar kriterleri

1. **Güven:** testler geçtiğinde, sistemin production'da da çalışacağına güvenebilmeliyiz.
2. **Hız:** geliştirici ve agent saniyeler içinde geri bildirim almalı; CI 10 dakikayı aşmamalı.
3. **Bakım kolaylığı:** testler uygulamanın iç yapısına değil davranışına bağlı olmalı; kod yeniden düzenlendiğinde (refactoring) gereksiz yere kırılmamalı.
4. **İzlenebilirlik:** her kabul kriterinin hangi testle doğrulandığı bulunabilmeli.

## Değerlendirilen seçenekler

### Veritabanına dokunan kodu test etme

#### Seçenek 1: Mock'lar (sahte repository ve `DbContext`)

Handler'lar, sahte repository'lerle birim testi olarak test edilir.

- **Artıları:** Çok hızlı; Docker gerektirmez.
- **Eksileri:** Bu projedeki en önemli kuralların bir kısmı veritabanında yaşıyor (tenant filtreleri, bileşik foreign key'ler, kilitler, benzersiz index'ler); mock'lar bunların hiçbirini test etmez. Mock'lar uygulamanın iç yapısını taklit ettiği için kod her yeniden düzenlendiğinde testler de kırılır. Test geçer ama production'da hata çıkar.

#### Seçenek 2: EF Core in-memory provider veya SQLite

- **Artıları:** Hızlı; Docker gerektirmez.
- **Eksileri:** Microsoft da in-memory provider'ı test için önermiyor. PostgreSQL'e özgü özellikler (`FOR UPDATE`, kısmi index, `xmin`, şemalar) çalışmaz veya farklı davranır. Yanlış bir güven duygusu verir.

#### Seçenek 3: Testcontainers ile gerçek PostgreSQL

Testler, Docker'da çalışan gerçek bir PostgreSQL'e karşı ve API'nin tamamı üzerinden (`WebApplicationFactory`) çalışır.

- **Artıları:** Production ile aynı veritabanı; tüm kurallar gerçekten test edilir. Testler HTTP üzerinden davranışı doğrular; iç yapı değiştiğinde kırılmaz. Eşzamanlılık testleri (ADR-0005) ancak böyle mümkün.
- **Eksileri:** Birim testlerinden daha yavaş (konteyner başlatma birkaç saniye, test başına milisaniyeler). Docker gerektirir (yerelde ve CI'da).

### Test piramidinin şekli

- **Klasik piramit:** çok sayıda birim testi, daha az entegrasyon testi, çok az uçtan uca test.
- **Test kupası (testing trophy):** ağırlık entegrasyon testlerinde; çünkü en çok güveni onlar verir.

## Karar

**Veritabanına dokunan kodu Seçenek 3 ile (Testcontainers ile gerçek PostgreSQL ve API üzerinden) test ediyoruz.** Piramit, iki geniş katmana dayanır: hızlı **domain birim testleri** ve gerçek veritabanıyla **API entegrasyon testleri**. Diğer test türleri bunları tamamlar.

**Test türleri:**

| Tür | Neyi test eder | Nasıl | Miktar | Proje |
| --- | --- | --- | --- | --- |
| Domain birim testleri | Aggregate invariant'ları, value object'ler, domain servisleri | Saf C#; veritabanı, mock veya DI yok; zaman `FakeTimeProvider` ile | Çok | `Domain.UnitTests` |
| Application birim testleri | Veritabanına dokunmayan application mantığı: decorator'lar, doğrulayıcılar | Saf C#; gerekirse az sayıda sahte nesne | Az | `Application.UnitTests` |
| API entegrasyon testleri | Kullanıcı hikayelerinin kabul kriterleri, uçtan uca bir HTTP isteği | `WebApplicationFactory` + Testcontainers PostgreSQL | Çok | `IntegrationTests` |
| Tenant izolasyon testleri | Her endpoint'in başka bir okulun kullanıcısıyla çağrılması (US-02) | İki tenant'lı fixture | Her endpoint için | `IntegrationTests` |
| Contract testleri | Yanıtın yapısı: antrenöre giden yanıtta yasak alan yok (US-14), ProblemDetails biçimi | Yanıtın JSON şeması üzerinde | Kritik endpoint'ler | `IntegrationTests` |
| Eşzamanlılık testleri | Aynı anda gelen kayıt istekleri (ADR-0005) | Paralel HTTP istekleri, gerçek PostgreSQL | Kritik senaryolar | `IntegrationTests` |
| Mimari testleri | Katman ve modül bağımlılıkları, isimlendirme, `ITenantScoped` | NetArchTest | Kurallar kadar | `ArchitectureTests` |
| Frontend birim testleri | Angular servisleri, interceptor'lar, karmaşık component'ler | Angular CLI'nin varsayılan test çalıştırıcısı | Orta | `web/admin` |
| Uçtan uca testler | En kritik kullanıcı akışları: giriş, grup kurma, kayıt, yoklama | Playwright, gerçek tarayıcı | Az (5–10 akış) | `web/e2e` |
| Yük testleri | Performans hedefleri (NFR-PRF) | k6 | Faz 4 | `tests/load` |

**Kurallar:**

- **Mock'lar istisnadır.** Sadece kontrol edemediğimiz dış sistemler (faz 2'deki ödeme sağlayıcısı, e-posta servisi) ve birim testlerinde zaman için kullanılır. Kendi veritabanımız, kendi repository'lerimiz mock'lanmaz.
- **Testler birbirinden bağımsızdır.** Her entegrasyon testi temiz bir veritabanıyla başlar; testler arasında veritabanı Respawn ile hızlıca temizlenir. Konteyner tüm test çalışması boyunca bir kez başlatılır.
- **Test verisi builder'larla oluşturulur** (`new StudentBuilder().Adult().WithoutGuardian().Build()`); testler sadece kendileri için önemli olan alanları belirtir.
- **İsimlendirme davranışı anlatır:** `Enroll_WhenGroupIsFull_ReturnsGroupFullError`.
- **İzlenebilirlik:** kabul kriterlerini doğrulayan testler hikaye ID'siyle etiketlenir: `[Trait("Story", "US-17")]`. Böylece bir hikayenin tüm testleri tek komutla çalıştırılabilir ve faz geçişinde hangi kriterin hangi testle karşılandığı raporlanabilir.
- **Önce test:** bir hikaye geliştirilirken, kabul kriterlerinden türetilen testler kod yazılmadan önce yazılır ve kırmızı oldukları görülür. Agent ile çalışırken bu, "testleri yaz → kırmızı olduğunu göster → kodu yaz → yeşil olduğunu göster" döngüsüdür.

**Araçlar** (tamamı açık kaynak, ticari lisans gerektirmeyen):

| İş | Araç |
| --- | --- |
| Test çatısı | xUnit |
| Doğrulama ifadeleri (assertion) | Shouldly |
| Sahte nesneler (gerektiğinde) | NSubstitute |
| Gerçek veritabanı | Testcontainers (PostgreSQL) |
| Testler arası veritabanı temizliği | Respawn |
| Zaman kontrolü | `Microsoft.Extensions.TimeProvider.Testing` (`FakeTimeProvider`) |
| Mimari testleri | NetArchTest |
| Kapsam ölçümü | Coverlet |
| Uçtan uca | Playwright |
| Yük | k6 (faz 4) |

Fluent Assertions, 8. sürümden itibaren ticari lisans gerektirdiği için kullanılmaz.

**CI'da ne zaman çalışır:**

| Ne zaman | Çalışan testler | Hedef süre |
| --- | --- | --- |
| Her PR | Birim, mimari, entegrasyon (tenant izolasyonu, contract ve eşzamanlılık dahil), frontend birim testleri | < 10 dk |
| Staging deploy'undan sonra | Playwright uçtan uca testleri, staging ortamına karşı | < 5 dk |
| Gecelik | Uçtan uca testlerin üç tarayıcıda tamamı (NFR-CMP-01) | — |
| Faz 4 | Yük testleri | — |

**Kapsam:** `Domain` ve `Application` için en az %80 satır kapsamı (NFR-MNT-03). Kapsam bir sinyal olarak izlenir; hedefi tutturmak için anlamsız testler yazılmaz.

## Sonuçlar

- **Olumlu:**
  - Testler production ile aynı veritabanında çalıştığı için, geçtiklerinde sisteme gerçekten güvenebiliriz.
  - En kritik güvenlik ve tutarlılık kuralları (tenant izolasyonu, bileşik foreign key'ler, kilitler, kısmi index'ler) doğrudan test edilir.
  - HTTP üzerinden yazılan testler, iç yapı değiştiğinde kırılmaz; yeniden düzenleme (refactoring) güvenle yapılabilir.
  - Her kabul kriterinden teste kadar izlenebilirlik vardır.
  - Agent'ın yazdığı kodun doğruluğu, agent'ın kendi beyanına değil, testlere dayanır.
- **Olumsuz:**
  - Entegrasyon testleri birim testlerinden yavaştır; test sayısı arttıkça CI süresi izlenmelidir.
  - Yerelde ve CI'da Docker gerekir.
  - Uçtan uca testler kırılgan olabilir; sayıları bilinçli olarak az tutulur.
- **Riskler ve önlemler:**
  - *Risk:* CI süresi 10 dakikayı aşar. *Önlem:* testlerin paralel çalıştırılması, tek konteyner ve Respawn; süre her faz geçişinde raporlanır.
  - *Risk:* Agent, testi geçirmek için testi değiştirir. *Önlem:* `CLAUDE.md` kuralı: "Var olan bir testi, sahibin onayı olmadan zayıflatma veya silme"; PR incelemesinde test değişiklikleri ayrıca kontrol edilir.
  - *Risk:* Testler sıraya bağımlı hale gelir. *Önlem:* testler rastgele sırada çalıştırılır; her test kendi verisini oluşturur.

## Uygulama ve doğrulama

- P0-23: test projeleri ve araçların kurulumu.
- P1-07: mimari testleri.
- P1-08: entegrasyon test altyapısı (`WebApplicationFactory`, Testcontainers, Respawn, iki tenant'lı fixture, builder'lar).
- P1-35, P1-36: tenant izolasyon test paketi ve Playwright testleri.
- Faz geçişlerinde: test sayıları, kapsam ve CI süresi raporlanır (faz geçiş şablonu §3).

## Yeniden değerlendirme

- CI süresi, paralelleştirmeye rağmen hedefi aşarsa.
- Faz 5'te bir modül servise ayrıldığında; servisler arası sözleşmeler için consumer-driven contract testleri (ör. Pact) değerlendirilir.
- Faz 4'te mutation testing (Stryker.NET) ile testlerin kalitesi ölçülebilir.

## Kaynaklar

- `docs/product/user-stories.md`, `docs/product/nfr.md` (NFR-MNT)
- Microsoft Learn, EF Core: *Choosing a testing strategy*
- Martin Fowler, *The Practical Test Pyramid*
