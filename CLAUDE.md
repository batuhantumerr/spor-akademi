# CLAUDE.md

Bu dosya, bu repoda çalışan yapay zeka agent'ı için kalıcı talimatlardır. Her oturumda okunur; kısa tutulur. Ayrıntılar `docs/` altındadır ve gerektiğinde okunur.

## Proje

Çok kiracılı (multi-tenant) bir spor okulu yönetim platformu: okullar, branşlar, tesisler, gruplar, öğrenciler (çocuk ve yetişkin), kayıtlar, yoklama ve aidat. Projenin birincil amacı, sahibinin full stack geliştirici olarak öğrenmesidir. Bu yüzden **her kararın nedeni açıklanır** ve sahip, merge ettiği her satırı anlayabilmelidir.

- Backend: .NET 10, ASP.NET Core, EF Core, PostgreSQL
- Frontend: Angular (admin ve antrenör), faz 3'te React (veli)
- Mimari: Clean Architecture katmanlarıyla modüler monolit

## Dil

- Benimle **Türkçe** konuş.
- Kod, sınıf/değişken/tablo/dosya adları, enum değerleri ve **commit mesajları İngilizce**.
- Dokümanlar, ADR'lar, issue ve PR açıklamaları **Türkçe**.
- Teknik terimleri Türkçeleştirme (aggregate, tenant, handler, refresh token).

## Önce oku

| Ne zaman | Ne |
| --- | --- |
| Her işe başlarken | `docs/roadmap.md` (iş roadmap'te mi, hangi madde?) |
| Bir özellik geliştirirken | `docs/product/user-stories.md` (kabul kriterleri), `docs/domain/domain-model.md` |
| Endpoint yazarken | `docs/api/overview.md`, `docs/api/error-codes.md` |
| Mimari bir soru çıktığında | `docs/architecture/overview.md`, ilgili ADR (`docs/adr/`) |
| Bir kararın geçerli olup olmadığını kontrol ederken | `docs/decisions/decision-log.md` |
| Commit, branch, PR | `CONTRIBUTING.md` |

## Komutlar

> P0-23'te solution oluşturulduğunda doldurulacak.

```bash
# Yerel ortamı başlat
# Backend build / test
# Sadece bir hikayenin testlerini çalıştır: dotnet test --filter "Story=US-17"
# Format kontrolü
# Yeni migration
# Frontend test / lint
```

## Mimari kurallar

Gerekçeleri ADR'lardadır. Bunlara uymayan kod yazma; uyamıyorsan dur ve sor.

- **Katmanlar (ADR-0001):** `Domain` hiçbir projeye ve framework'e bağımlı değil. `Application` sadece `Domain`'e bağımlı. `Api`'de iş mantığı yok.
- **Modüller (ADR-0001, ADR-0009):** dokuz modül, katman projeleri içinde klasör. Bir modül başka bir modülün aggregate'ına, tablosuna veya iç tiplerine erişemez; sadece o modülün `Application/<Modül>/Contracts` arayüzlerini kullanır.
- **İş kuralları aggregate'ta yaşar.** Handler, aggregate'ı yükler, metodunu çağırır, kaydeder. Handler'da `if` ile iş kuralı yazma.
- **Command / query (ADR-0004):** handler'lar doğrudan enjekte edilir; dispatcher yok. Query'ler aggregate yüklemez; `AsNoTracking` + `Select` ile doğrudan DTO'ya projekte eder. Domain entity'leri asla API yanıtı olmaz. Mapping elle yapılır.
- **Hatalar (ADR-0007):** beklenen hatalar `Result` / `Error` ile döner, exception fırlatılmaz. Her hata kodu modülün `*Errors` sınıfında tanımlanır ve `docs/api/error-codes.md`'ye eklenir.
- **Tenant (ADR-0003):** tenant'a bağlı her entity `ITenantScoped`. `TenantId`'yi elle atama veya filtreleme; altyapı yapar. `IgnoreQueryFilters()` sadece `Tenancy` modülünde. Ham SQL istisnadır ve `tenant_id` koşulunu açıkça içerir.
- **Domain event'leri (ADR-0009):** aynı transaction içinde işlenir; işleyicileri sadece veritabanı değişikliği yapar, dış sistem (e-posta vb.) çağırmaz.
- **Kayıt kilitleri (ADR-0005):** öğrenci ve grup satırları sadece `IEnrollmentLocks` üzerinden ve her zaman önce öğrenci, sonra grup sırasıyla kilitlenir.
- **Zaman:** `TimeProvider` kullan; `DateTime.Now` / `UtcNow` yasak.
- **Diğer:** ID'ler UUID v7; para `decimal` + `Money`; genel soft delete yok, iş durumları var (`Archived`, `Left`, `Ended`, `Cancelled`).
- **API (docs/api/overview.md):** `/api/v1`, durum geçişleri aksiyon endpoint'i (`POST /students/{id}/leave`), her endpoint tek bir yanıt şekli, düzenlenebilir kaynaklarda `version`.

## Test kuralları (ADR-0008)

- **Önce test:** bir hikayeye başlarken önce kabul kriterlerinden testleri yaz, çalıştır ve kırmızı olduklarını göster. Sonra kodu yaz.
- Kabul kriteri testlerini `[Trait("Story", "US-xx")]` ile etiketle.
- Veritabanına dokunan kodu Testcontainers ile gerçek PostgreSQL üzerinde, API üzerinden test et. Kendi veritabanımızı veya repository'lerimizi mock'lama.
- Assertion için Shouldly; FluentAssertions yasak.
- **Var olan bir testi sahibin onayı olmadan asla zayıflatma, atlama (`Skip`) veya silme.** Bir test kırılıyorsa önce kodun mu testin mi yanlış olduğunu açıkla ve sor.
- Bir işi bitirdiğini söylemeden önce ilgili testleri çalıştır ve sonucu göster.

## Güvenlik ve gizlilik

- Loglara kişisel veri yazma (isim, telefon, e-posta, doğum tarihi, token). Sadece ID'ler.
- Gizli bilgi (anahtar, şifre, connection string) koda, test verisine veya loglara girmez.
- Antrenöre giden yanıt tiplerinde veli bilgisi veya öğrencinin kendi iletişim bilgisi bulunamaz (DEC-007, DEC-071).
- Kriptografi kodu yazma; standart .NET bileşenlerini kullan (ADR-0006).
- Gerçek kişisel veri kullanma; seed ve test verileri sentetik (DEC-031).

## Çalışma şekli

1. **Planla, sonra yaz.** Kod yazmadan önce kısa bir plan sun: hangi dosyalar, hangi testler, hangi kararlar. Onayımı bekle.
2. **Küçük adımlar.** Bir seferde tek bir dikey dilim. Bir PR yaklaşık 400 satırı geçmesin.
3. **Belirsizlikte sor.** Kabul kriterleri bir durumu kapsamıyorsa varsayım yapma; sor.
4. **Açıkla.** Bir işi bitirince Türkçe ve kısaca anlat: ne yaptın, hangi kavramı kullandın ve neden. Kullandığın yeni bir kavram varsa `docs/learning/journal.md` için bir taslak not öner (sahip kendi cümleleriyle yazar).
5. **Roadmap'i güncelle.** Bir madde Definition of Done'ı karşıladığında, aynı PR'da `docs/roadmap.md`'de işaretle.
6. **Commit:** Conventional Commits, İngilizce. Commit mesajlarına ve PR açıklamalarına `Co-Authored-By`, "Generated with" veya benzeri AI attribution satırları ekleme (DEC-035).

## GitHub iş akışı

GitHub işlemleri `gh` CLI ile yapılır.

1. **Issue:** Bir roadmap maddesine başlarken önce `gh issue list --search "<madde-id>"` ile issue'su var mı kontrol et. Yoksa aç:
   - Başlık: `<madde-id> · <roadmap'teki kısa başlık>` (ör. `P0-23 · Backend solution iskeleti`)
   - Gövde: `.github/ISSUE_TEMPLATE/roadmap-item.yml` formundaki alanlar (roadmap maddesi, ilgili hikayeler, amaç, alt görevler)
   - Milestone: maddenin fazı (ör. `Phase 0 – Foundation (v0.0.1)`)
   - Etiketler: bir `type:*`; varsa ilgili `module:*` veya `area:*`; faz 1 ve sonrasında hikayenin önceliğine göre `priority:*`
2. **Branch:** Güncel `main`'den (`git switch main && git pull`), `CONTRIBUTING.md` §2'deki isimle: `<tür>/<issue-no>-<kısa-açıklama>`.
3. **Commit:** `CONTRIBUTING.md` §3. Küçük ve anlamlı commit'ler.
4. **PR:** Kendi kontrolünü yapıp testleri çalıştırdıktan sonra push et ve `gh pr create` ile aç:
   - Başlık: Conventional Commits biçiminde (squash commit mesajı olacak)
   - Gövde: `.github/pull_request_template.md`'yi doldur; "İlgili" satırında `Closes #<issue-no>` olsun; kontrol listesinde gerçekten yaptıklarını işaretle
5. **Merge'i sahip yapar.** PR'ı açtıktan sonra dur ve bana bildir. İnceleme yorumlarını aynı branch'te yeni commit'lerle düzelt.
6. **Merge sonrası:** Ben "merge edildi" dediğimde `git switch main && git pull` yap ve yerel branch'i sil.

## Asla yapma

- Roadmap'te olmayan bir işe başlamak veya kapsamı kendi başına değiştirmek.
- Bir kararı (`decision-log.md`) kabul etmek, değiştirmek veya kaldırmak; bir faz geçişini onaylamak. Karar önerebilirsin (`Proposed`), onaylayan sahiptir.
- Kabul edilmiş bir ADR'ı düzenlemek.
- Onay almadan yeni bir NuGet veya npm paketi eklemek.
- Uygulanmış bir migration'ı düzenlemek veya silmek.
- Uyarıları bastırmak, analyzer kurallarını kapatmak veya CI'ı testleri geçirmek için değiştirmek.
- `main`'e doğrudan push etmek veya force push yapmak.
- Bir yeniden yapılandırmayı (refactoring) istenen işin içine gizlemek.
- Bir PR'ı merge etmek veya bir issue'yu elle kapatmak.
- `--force` ile push etmek veya `gh` komutlarında onay adımlarını atlamak.
