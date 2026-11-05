# Faz 0 Geçişi — Temel

> Repodaki yeri: `docs/phases/phase-0-gate.md` (`docs/phases/phase-gate-template.md` şablonundan kopyalandı)
> Hazırlayan: Batuhan Tümer · Hazırlanma tarihi: 2026-11-05 · Durum: `Taslak`

Bölüm 7 sahip tarafından imzalanmadan bir sonraki faz başlamaz.

Bu geçiş **hafif** tutuldu (sahibin talebi). Bölüm 4'te 90'dan fazla kararın hepsi değil, sadece değişmesi önerilenler var. Listelenmeyen her `Accepted` karar için öneri: *Koru*.

## 1. Kapsam incelemesi

| Madde | Başlık | Sonuç | Kanıt |
| --- | --- | --- | --- |
| P0-01 – P0-07 | Ürün ve domain: vizyon, kullanıcı hikayeleri, domain modeli, roadmap, karar günlüğü, faz geçiş şablonu, NFR'ler | Tamamlandı | `docs/product/`, `docs/domain/`, `docs/roadmap.md`, `docs/decisions/`, `docs/phases/` |
| P0-08 – P0-17, P0-36, P0-37 | Mimari: ADR-0001 – ADR-0009 ve ADR-0011, mimariye genel bakış, API'ye genel bakış | Tamamlandı | `docs/adr/`, `docs/architecture/overview.md`, `docs/api/overview.md`; ADR-0011: #23 |
| P0-18 – P0-20 | Çalışma kuralları: `CONTRIBUTING.md`, `CLAUDE.md`, öğrenme günlüğü | Tamamlandı | #1, #3 ve sonraki kural PR'ları (bölüm 5) |
| P0-21, P0-22 | Geliştirme ortamı, GitHub reposu | Tamamlandı | #1 |
| P0-23 | Backend solution iskeleti | Tamamlandı | #5 |
| P0-24 | Frontend iskeleti | Tamamlandı | #7 |
| P0-25 | Docker Compose ile yerel ortam | Tamamlandı | #19; 2026-11-05'te PostgreSQL Alpine'a geçti (#62) |
| P0-26 | Sağlık endpoint'leri | Tamamlandı | #32 |
| P0-27 | İlk EF Core migration'ı | Tamamlandı | #25, #27 |
| P0-28 | CI pipeline'ı | Tamamlandı | #35, #36, #40 (#41, #44 düzeltmeler) |
| P0-29 | Production'a hazır API ve admin imajları | Tamamlandı | #49, #52 |
| P0-38 | NuGet lock dosyaları | Tamamlandı | #55 |
| P0-30 | Sunucu | Taşındı | Deploy bloğu (roadmap §9, DEC-094) |
| P0-31 | CD pipeline'ı | Kısmen tamamlandı, kalanı taşındı | Tamamlanan: GHCR'ye yayımlama (#58), migrator imajı (#61). Kalan: deploy workflow'u, Deploy bloğunda |
| P0-32 | Geri alma provası | Taşındı | Deploy bloğu |
| P0-33 | Faz 1'in her maddesi için issue | İptal edildi | Issue'lar her maddeye başlanırken açılıyor (CLAUDE.md, GitHub iş akışı) |
| P0-34 | `v0.0.1` etiketi | Bu geçiş onaylanınca | — |
| P0-35 | Faz 0 geçişi | Bu doküman | #65 |

Tamamlanmayan maddeler ve nedenleri:

| Madde | Taşındığı yer | Gerekçe |
| --- | --- | --- |
| P0-30, P0-31'in kalanı, P0-32 | Deploy bloğu (roadmap §9), milestone "Deploy (unscheduled)" (#53, #56) | Sunucu alımının ne zaman yapılacağı belli değil; faz geçişleri ona bağlı değil (DEC-094) |
| Deploy hazırlığının kalan iki parçası: production compose ve Caddy, şifreli yedek betiği | Deploy bloğu | Aynı gerekçe. Üçüncü taraf imaj taraması ve istisna kararları da blokla birlikte bekliyor (#53'teki yorum) |

## 2. Çıkış kriterleri

| Çıkış kriteri (roadmap'ten) | Karşılandı mı? | Kanıt |
| --- | --- | --- |
| Tüm planlama dokümanları onaylı ve repoda | Evet | Bölüm 1, P0-01 – P0-20 |
| CI tüm kontrolleri zorunlu kılıyor: zorunlu check'ler geçmeden `main`'e merge edilemiyor | Evet | `main`'in ruleset'i `backend`, `frontend`, `repo-checks` ve `images`'ı (kaynak GitHub Actions) zorunlu kılıyor; "branch güncel olmalı" kuralı açık (`gh api repos/batuhantumerr/spor-akademi/rules/branches/main`, 2026-11-05). `images`'ı sahip 2026-11-05'te ekledi. Bu taslak hazırlanırken `images` zorunlu değildi; duman testleri ve Trivy (DEC-080) merge'i engellemiyordu. CLAUDE.md'deki "merge'e hazır" listesi buna göre güncellendi. |
| İmajlar (API, admin, migrator) `main`'den GHCR'de yayımlanıyor | Evet | `main` run 42025184307 (`5fa28a4`) başarılı. Paketlerin public olduğu ve kimlik bilgisi olmadan çekilebildiği #58 ve #61'de doğrulandı. |
| Uygulama yerelde, README'deki komutlarla çalışıyor | Evet | Temiz klon, sadece README izlenerek (2026-11-05, `28f0160`): `/health/ready` 200, admin `http://localhost:4200/` 200. Ayrıntılar aşağıda. |
| Yeni bir geliştirici README ile projeyi 15 dakikanın altında yerelde başlatabilir | Evet | Aynı ölçüm: klondan admin açılana kadar **88 sn** |

**Temiz klon ölçümü (2026-11-05).** Repo geçici bir klasöre klonlandı ve README adım adım izlendi. Ölçüm iki kez yapıldı.

Ortam, yeni bir makineye mümkün olduğunca yakın kuruldu:
- NuGet paket klasörü ve npm önbelleği boş geçici klasörlere yönlendirildi.
- PostgreSQL ve Seq imajları silinip yeniden çekildi.

Sınırlar:
- Ön koşullar (Docker, .NET SDK, Node) kuruluydu, süreye dahil değil.
- NuGet'in HTTP önbelleği sıcaktı. Gerçekten yeni bir makinede restore birkaç dakika daha sürebilir.

Aynı makinede çalıştığı için klonun compose proje adı farklıydı (`COMPOSE_PROJECT_NAME`); README'den tek sapma bu.

**İlk deneme (`5fa28a4`): README yetmedi.** İki eksik bulundu:
1. **Restore adımı yoktu.** `scripts/ef.sh database update`'ten önce restore veya build adımı olmadığı için komut `NETSDK1004` ile başarısız oldu (`project.assets.json` yok); `dotnet ef` kendi restore'unu yapmıyor.
2. **Admin uygulaması anlatılmıyordu.** README admin uygulamasının nasıl çalıştırılacağını hiç anlatmıyordu.

İki eksik README dışından giderilince süre 84 sn oldu.

**Düzeltme (bu PR):** README'ye `dotnet restore` adımı ve "Admin uygulaması" bölümü (`npm ci`, `npm start`, Node sürümü `.nvmrc`'de) eklendi.

**İkinci deneme (`28f0160`): sadece README izlendi.** README dışından hiçbir adım gerekmedi.

| Adım (README sırası) | Süre (başlangıçtan) | Sonuç |
| --- | --- | --- |
| `git clone`, `git config core.hooksPath .githooks` | 4 sn | Tamam |
| `cp .env.example .env`, şifreler `openssl rand -hex 24` ile | 5 sn | Tamam. Bu makinede 5432'yi Windows'a kurulu başka bir PostgreSQL kullanıyor; README'deki not izlenerek `POSTGRES_HOST_PORT=5433` yapıldı. |
| `docker compose up -d --wait` (imajlar çekildi) | 25 sn | Tamam |
| `dotnet tool restore`, `dotnet restore` | 31 sn | Tamam |
| `scripts/ef.sh --check-env`, `scripts/ef.sh database update` | 39 sn | Tamam |
| `scripts/run-api.sh` ile `/health/live` ve `/health/ready` | 44 sn | 200, 200 |
| `cd web/admin`, `npm ci`, `npm start` | 88 sn | `http://localhost:4200/` 200 |

**Not, eksik değil:** 5432'yi başka bir PostgreSQL kullanıyorsa `docker compose up` açık bir hatayla durur ve README port notunu veriyor. Ama bu adım atlanırsa API o PostgreSQL'e bağlanmaya çalışır ve sadece `/health/ready` 503 görülür.

## 3. Kalite özeti

| Ölçüt | Değer |
| --- | --- |
| Testler (birim / entegrasyon / mimari / e2e) | Backend 9 (`dotnet test`), frontend 2 (`npm run test:ci`). Entegrasyon testi henüz yok (P1-08), e2e yok (P1-36). |
| CI süresi | ~3 dk (run 42025941179). En uzun job `images`, 3 dk; `backend` 1 dk; `frontend` 1 dk 15 sn |
| Açık hatalar (önem derecesine göre) | Yok |
| Bilinen teknik borç (bağlı issue'lar) | #12 (exit code 8 istisnası, Faz 1); #39 (Dependabot gözlemleri; Faz 1 milestone'una taşındı); `type:idea`: #57, #59, #60 |
| Bağımlılık güvenlik açıkları | NuGet ve npm denetimi CI'da high/critical 0. Kendi imajlarımızda (API, admin, migrator) Trivy high/critical 0. Üçüncü taraf imajların bulguları #53'te (Deploy bloğu) |
| Geri yükleme tatbikatı sonucu (RTO) | Yok; P0-32 ile Deploy bloğunda |

## 4. Karar incelemesi

Sadece değişmesi önerilen kararlar. Listelenmeyen her `Accepted` karar için öneri: *Koru*.

| ID | Karar | Hâlâ doğru mu? Bu fazdan notlar | Sahibin seçimi: Koru / Değiştir / Kaldır |
| --- | --- | --- | --- |
| DEC-063 | "… uçtan uca testler staging deploy'undan sonra; tüm tarayıcılarda gecelik" | Hayır. Staging yok (DEC-094); bu haliyle P1-36'daki Playwright testleri çalışacak bir yer bulamaz. | **Değiştir** → DEC-095: staging yokken uçtan uca testler CI'da başlatılan uygulamaya karşı çalışır; Deploy bloğu açılınca staging'e döner. Karar günlüğüne işlendi (DEC-063 `Superseded by DEC-095`). |
| DEC-027 | "MediatR veya AutoMapper yok: projeye ait dispatcher ve elle mapping (DEC-046 ile netleştirildi: ayrı dispatcher sınıfı yok)" | İlk cümle ("projeye ait dispatcher") ile DEC-046 ("ayrı dispatcher sınıfı yok") çelişiyor; CLAUDE.md DEC-046'yı izliyor. | **Değiştir** → `Superseded by DEC-046`. Karar günlüğüne işlendi. |

**Not, değişiklik değil:**
- DEC-082 – DEC-091 (barındırma, alan adı, Caddy, R2, Seq, alt alan adları, GHCR, ayrı PostgreSQL, age) Deploy bloğuyla birlikte uykuda. Öneri: *Koru*; blok açıldığında DEC-082'deki 2026-11-04 tarihli fiyat ve stok bilgileri yeniden doğrulanır.
- DEC-080'in amacı (high/critical bulgu CI'ı kırar), `images` zorunlu check olunca kendi imajlarımız için tam uygulanmış oldu (bölüm 2).

Sonraki faz için önerilen yeni kararlar:

| Öneri | Neden | Değerlendirilen seçenekler | Sahibin seçimi: Kabul / Ret / Ertele |
| --- | --- | --- | --- |
| `images` job'ı zorunlu check olur | Taslakta: Trivy ve duman testleri merge'i engellemiyordu | (a) `images` zorunlu olur (CI ~3 dk, en uzun job). (b) Olduğu gibi kalır; bulgu sadece `main`'deki yayımlamayı durdurur. | **Kabul (a).** Sahip 2026-11-05'te ruleset'e ekledi; CLAUDE.md güncellendi. |
| Faz 1 boyunca altyapı ve güvenlik araçlarıyla ilgili yeni fikirler, bir şeyi gerçekten kırmadıkça `type:idea` olarak bekler | Sahibin talebi (2026-11-05); Faz 0'ın uzamasının ana nedeni (bölüm 5) | Kural olarak CLAUDE.md'nin "Kapsam disiplini" bölümüne de eklenir | **Kabul.** CLAUDE.md'nin "Kapsam disiplini" bölümüne eklendi. |

## 5. Retrospektif

- **Ne iyi gitti:**
  - Her şey geri izlenebilir: 95 karar, 10 ADR. Her PR bir roadmap maddesine bağlı.
  - CI ve imajlar yeniden üretilebilir: lock dosyaları, tag ve digest ile sabitlenmiş imajlar. `main`'e giden imaj, test edilen imajla birebir aynı.
  - Gizli bilgi repoya, imajlara veya loglara girmedi.
  - "Önce doğrula" ve kırmızı demo alışkanlığı iki yanlış varsayımı merge'den önce yakaladı:
    - Dependabot'un `compose.yaml`'ı izlemediği;
    - boş migration'da tablo kontrolünün hiçbir şeyi kontrol etmediği.
- **Ne zorladı: Faz 0 neden uzadı:**
  1. **Sunucu yokken deploy hazırlığı.** 2026-11-04'teki kural şuydu: "sunucu olmadan test edilemeyen hiçbir şey şimdi hazırlanmaz". Buna rağmen deploy için 12 karar (DEC-082 – DEC-093) ve 5 PR (#54, #58, #61, #62, #64) çıktı. Bunların bir kısmı sonunda zamanlanmamış bloğa taşındı.
  2. **Tedarik zinciri ve güvenlik araçlarının derinliği MVP ihtiyacını aştı.** Bunlar arasında şunlar vardı:
     - Trivy action'ının ele geçirilmesinden sonra commit hash'iyle sabitleme;
     - imajların digest ile sabitlenmesi;
     - faz ortasında eklenen P0-38 (lock dosyaları);
     - slim imaj (DEC-092);
     - RID'li kilitli restore (DEC-093);
     - üçüncü taraf imaj taraması.

     Her keşif yeni bir karar ve takip işi üretti.
  3. **Kural PR'ları.** Yedi PR sadece agent'ın çalışma kurallarını değiştirdi (#3, #11, #14, #29, #34, #38, #51); çoğu bir hatadan sonra yazıldı.
  4. **Kapsam faz içinde büyüdü.** P0-36, P0-37 ve P0-38 faz ortasında eklendi; P0-28 ve P0-29 ikişer-üçer PR sürdü.
  5. **Öğrenme günlüğü boş kaldı.** §1.6 uygulanmadı: taslak notlar önerildi ama günlüğe yazılmadı. Bölüm 6 bu yüzden boş.
- **Sonraki fazda süreçte neyi değiştiriyoruz:**
  - Altyapı ve güvenlik fikirleri, bir şeyi gerçekten kırmadıkça `type:idea` olarak bekler (sahibin talebi).
  - Bir maddenin öğrenme notu, o maddenin PR'ında yazılır. Öneri: PR şablonuna "journal notu yazıldı" kutusu eklenir.
  - Faz 1 dikey dilimlerle ilerler. Bir hikayenin kabul kriterleri karşılandığında iş biter; "daha iyi olabilir" yeni bir issue'dur.
- **Yapay zeka destekli geliştirme nasıl gitti:**
  - **İşe yarayanlar:**
    - Plan → onay → küçük PR döngüsü.
    - Sürüm ve davranış iddialarının bir komutla veya resmi dokümanla doğrulanması.
    - GitHub durumunun her işlemden önce kontrol edilmesi.
    - CI sonucunun PR'a yorum olarak yazılması.
  - **Ters gidenler:**
    - Agent birkaç kez doğrulanmamış bir varsayımla ilerledi. Örnekler: yerelde üretilen bundle'ın lock dosyalarına `win-x64` yazması, Dependabot'un kapsamı.
    - Kontrol yükü çok arttı: bir değişikliği doğrulamak, değişikliğin kendisinden uzun sürdü.
  - **CLAUDE.md'ye eklenenler:** "Kapsam disiplini", GitHub durum kontrolleri, "bundle yerelde üretilmez".

## 6. Öğrenme özeti

`docs/learning/journal.md`'de henüz not yok. Faz 0'da kullanılan ve mülakat kartına dönüşebilecek kavramlar aşağıda. Açıklamaları sahip kendi cümleleriyle yazar.

| Kavram | Projede nerede kullanıldı | Tek cümlede açıklaması |
| --- | --- | --- |
| Rolling-wave planning ve faz geçişleri | `docs/roadmap.md` §1, bu doküman | |
| En az yetki ilkesi ile veritabanı rolleri | `docker/postgres/initdb/10-create-roles.sh`, ADR-0011 | |
| Migration bundle ve "aynı build'den SQL ile bundle" | `docker/migrator/Dockerfile`, DEC-093 | |
| Multi-stage build, root olmayan kullanıcı, digest ile sabitleme | `docker/`, DEC-081, DEC-092 | |
| Lock dosyaları ve kilitli restore (tedarik zinciri) | `packages.lock.json`, P0-38 | |
| Test edilen imajın yayımlanması (build once, deploy many) | `.github/workflows/ci.yml`, DEC-089 | |
| Collation: glibc ve musl farkı | `compose.yaml`, #62 | |

## 7. Sonraki faz planı ve onay

- [x] Faz 1'in detaylı görevleri `docs/roadmap.md`'ye yazıldı (P1-01 – P1-40)
- [ ] Faz 1 için GitHub issue'ları ve milestone oluşturuldu: milestone var (`Phase 1 – MVP (v0.1.0)`). Issue'lar her maddeye başlanırken açılıyor (P0-33 iptal).
- [x] Bölüm 4'teki karar değişiklikleri uygulandı: DEC-095 eklendi; DEC-063 ve DEC-027'nin yerine geçildi. Yeni ADR gerekmedi; ADR-0008 kabul edilmiş olduğu için düzenlenmedi, DEC-095 onu netleştiriyor.
- [ ] Sürüm etiketlendi: `v0.0.1`

**Sahibin karar vermesi gereken açık noktalar:** yok.

**Sahibin kararlarıyla kapananlar (2026-11-05):** DEC-063 → DEC-095; DEC-027 → `Superseded by DEC-046`; `images` zorunlu check; #39 Faz 1 milestone'unda; README'deki iki eksik düzeltildi ve temiz klonla yeniden ölçüldü; `type:idea` kuralı CLAUDE.md'de.

**Onay**

Onaylayan: Batuhan Tümer · Tarih: ____________

Yorumlar:
