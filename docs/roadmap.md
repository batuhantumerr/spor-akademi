# Yol Haritası (Roadmap)

> Durum: Onaylandı v1.0 · Repodaki yeri: `docs/roadmap.md` · Son güncelleme: 2026-10-27

Bu doküman, **hangi fazda neyin** yapılacağının tek doğruluk kaynağıdır. Burada listelenmeyen hiçbir şey geliştirilmez. Maddeler tamamlandıkça işaretlenir.

---

## 1. Bu yol haritası nasıl işler

### 1.1 Planlama modeli: kapsam erkenden sabit, görevler zamanı gelince detaylı

- **Her fazın kapsamı** (özellikleri) şimdi, bu dokümanda belirlenir.
- **Detaylı görevler** sadece mevcut faz için yazılır. Bir sonraki fazın görevleri, önceki fazda öğrenilenler kullanılarak faz geçişinde detaylandırılır. Buna *rolling-wave planning* denir: her şeyi baştan detaylı planlamak, sıra geldiğinde yanlış olan görevler üretir.
- Bir fazın kapsamını değiştirmek sadece faz geçişinde ya da değişiklik günlüğüne (bölüm 9) kaydedilip sahip tarafından onaylanan açık bir kapsam değişikliğiyle mümkündür.

### 1.2 Madde ID'leri ve onay kutuları

- Her maddenin değişmeyen bir ID'si vardır: `P<faz>-<numara>`, ör. `P1-14`.
- `- [ ]` = yapılmadı, `- [x]` = yapıldı. "Devam ediyor" durumu burada değil, GitHub'da takip edilir.
- Bir madde ancak **Definition of Done**'ı (bölüm 1.4) karşıladığında işaretlenir. İşaretlenirken PR numarası eklenir: `- [x] **P1-14** … (#23)`.
- İptal edilen maddeler silinmez; gerekçesiyle birlikte üstü çizilir: `- [ ] ~~**P2-07** …~~ — faz 3'e taşındı, bkz. değişiklik günlüğü`.

### 1.3 Neyin nerede durduğu

| Ne | Nerede | Amacı |
| --- | --- | --- |
| Faz kapsamı ve kontrol listesi | `docs/roadmap.md` (bu dosya) | Plan ve ilerleme, tek bir yerden okunabilir |
| Detaylı görev çalışması | GitHub Issues; her roadmap maddesi için bir issue, başlığı `P1-14 · US-17 Öğrenciyi gruba kaydetme` | Tartışma, alt görevler, PR bağlantıları |
| Faz gruplaması | GitHub Milestones: `Phase 0`, `Phase 1 – MVP (v0.1.0)`, … | Faz bazında ilerleme |
| Pano | GitHub Projects: `Todo → In progress → In review → Done` | Günlük çalışma |
| Kararlar | `docs/decisions/decision-log.md` + `docs/adr/` | Ne karar verildi, neden ve ne zaman gözden geçirilecek |
| Faz geçişleri | Şablondan oluşturulan `docs/phases/phase-<n>-gate.md` | Bir sonraki faza geçiş için resmi onay |

Yol haritası ve GitHub asla birbiriyle çelişmemelidir. Kural: **bir issue'yu bir PR kapatır ve aynı PR bu dosyadaki maddeyi işaretler.**

### 1.4 Definition of Done (her madde için)

Bir madde, aşağıdakilerin hepsi doğru olduğunda tamamlanmış sayılır:

- [ ] Bağlı kullanıcı hikayelerinin tüm kabul kriterlerinin geçen otomatik testleri var.
- [ ] Kod incelendi: tüm diff'in kendi kendine incelemesi ve bir yapay zeka inceleme turu; her yorum çözüldü.
- [ ] CI yeşil (build, lint, format, birim, entegrasyon ve mimari testleri).
- [ ] Yeni uyarı yok; bağlı bir issue'su olmayan `TODO` yok.
- [ ] API değişiklikleri OpenAPI dokümanına yansıdı; Angular client yeniden üretildi.
- [ ] Davranış veya bir karar değiştiyse dokümanlar güncellendi.
- [ ] Staging'e deploy edildi ve elle kontrol edildi.
- [ ] Roadmap maddesi aynı PR'da işaretlendi.

### 1.5 Faz geçişleri (phase gate)

Bir faz sadece bir **faz geçişi** ile biter ve bir sonraki faz ancak sahip onayladıktan sonra başlar. Her geçişte:

1. **Kapsam incelemesi:** fazın her maddesi tamamlanmıştır ya da gerekçesiyle açıkça taşınmıştır.
2. **Çıkış kriterleri kontrolü:** fazın her çıkış kriteri kanıtla (test çalıştırması, bağlantı, ekran görüntüsü) doğrulanır.
3. **Karar incelemesi:** karar günlüğündeki her aktif karar yeniden sunulur ve sahip her biri için *Koru*, *Değiştir* veya *Kaldır* der. Değişiklikler yeni bir ADR üretir.
4. **Retrospektif:** ne iyi gitti, ne zorladı, süreçte ne değişmeli.
5. **Sonraki faz planlaması:** sonraki fazın detaylı görevleri bu yol haritasına yazılır ve GitHub issue'ları olarak açılır.
6. **Onay:** sahip geçiş dokümanını imzalar (ad, tarih). Sonraki faz ancak bundan sonra başlar.

Yapay zeka agent'ı geçiş dokümanını hazırlar ama bir geçişi asla onaylamaz veya kabul edilmiş bir kararı kendi başına değiştirmez.

### 1.6 Öğrenme döngüsü (her madde için)

Tamamlanan her madde için `docs/learning/journal.md` dosyasına bir iki cümle yazılır: hangi kavramı öğretti ve bir mülakatta nasıl anlatılır. Her faz geçişinde bu notlar o fazın "mülakat kartlarına" dönüşür.

---

## 2. Fazlara genel bakış

| Faz | Adı | Sürüm | Ana çıktı |
| --- | --- | --- | --- |
| 0 | Temel | `v0.0.1` | Planlar, kararlar ve production'a otomatik deploy edilen boş bir uygulama |
| 1 | MVP | `v0.1.0` | Admin ve antrenörler için okullar, gruplar, öğrenciler, kayıt ve yoklama |
| 2 | İş mantığı derinliği | `v0.2.0` | Online ödeme, bildirimler, arka plan işleri, dosya depolama, cache |
| 3 | Veli deneyimi | `v0.3.0` | React veli uygulaması, gerçek zamanlı güncellemeler, dashboard'lar, çoklu dil |
| 4 | Production sağlamlaştırma | `v0.4.0` | Güvenlik, gözlemlenebilirlik, performans, mevzuat uyumu |
| 5 | Ölçek ve mimari | `v1.0.0` | Event-driven mesajlaşma, ayrılmış bir servis, bulut ve infrastructure as code |

---

## 3. Faz 0 — Temel

**Amaç:** tam olarak neyin yapılacağını bilmek ve boş bir uygulamayı production'a otomatik olarak çıkaran bir pipeline'a sahip olmak.
**Öğrenme hedefleri:** gereksinim mühendisliği, domain modelleme, mimari kararlar, Git iş akışı, Docker, CI/CD.
**Giriş kriterleri:** yok.

### 3.1 Ürün ve domain

- [x] **P0-01** Ürün vizyonu ve MVP kapsamı → `docs/product/vision.md`
- [x] **P0-02** Kabul kriterleriyle kullanıcı hikayeleri → `docs/product/user-stories.md`
- [x] **P0-03** Domain modeli ve ortak dil → `docs/domain/domain-model.md`
- [x] **P0-04** Yol haritası ve planlama süreci → `docs/roadmap.md`
- [x] **P0-05** Karar günlüğü → `docs/decisions/decision-log.md`
- [x] **P0-06** Faz geçiş şablonu → `docs/phases/phase-gate-template.md`
- [x] **P0-07** Fonksiyonel olmayan gereksinimler (güvenlik, KVKK, performans, erişilebilirlik, yedekleme, tarayıcı desteği) → `docs/product/nfr.md`

### 3.2 Mimari

- [x] **P0-08** ADR şablonu ve ADR-0001 Clean Architecture ile modüler monolit
- [x] **P0-09** ADR-0002 Öğrenciler kullanıcı değil, profildir
- [x] **P0-10** ADR-0003 Multi-tenancy: paylaşımlı veritabanı, `TenantId`, global query filter'lar
- [x] **P0-11** ADR-0004 Command/query handler'ları, decorator'lar ve elle mapping (MediatR / AutoMapper yok)
- [x] **P0-12** ADR-0005 Grup kontenjanı için eşzamanlılık stratejisi
- [x] **P0-13** ADR-0006 Kimlik doğrulama: JWT access token + rotate edilen refresh token'lar
- [x] **P0-14** ADR-0007 Hata yönetimi: Result pattern ve ProblemDetails
- [x] **P0-15** ADR-0008 Test stratejisi: test piramidi, Testcontainers, mimari testleri
- [x] **P0-36** ADR-0009 Modüller arası iletişim ve domain event'lerinin transaction içinde işlenmesi
- [x] **P0-16** Solution ve modül yapısı → `docs/architecture/overview.md` (C4 context ve container diyagramlarıyla)
- [x] **P0-17** API'ye genel bakış: kaynaklar, endpoint'ler, kurallar (sürümleme, sayfalama, hatalar, ID'ler) → `docs/api/overview.md`

### 3.3 Çalışma kuralları

- [x] **P0-18** `CONTRIBUTING.md`: branching (kısa ömürlü branch'lerle trunk-based), Conventional Commits, PR boyutu, inceleme kontrol listesi
- [x] **P0-19** `CLAUDE.md` / `AGENTS.md`: proje bağlamı, kurallar, komutlar, dil kuralları, roadmap ve faz geçişi kuralları
- [x] **P0-20** `docs/learning/journal.md` oluşturuldu

### 3.4 Repo ve walking skeleton

- [x] **P0-21** Geliştirme ortamı kuruldu ve doğrulandı (.NET 10 SDK, Node LTS, Angular CLI, Docker, Git, IDE, Claude Code); Git kimliği sahibin adıyla ayarlandı; Claude Code'da commit ve PR attribution kapatıldı (DEC-035)
- [x] **P0-22** GitHub reposu: public (DEC-072), `main` korumalı, etiketler, milestone'lar, proje panosu, issue ve PR şablonları; AI attribution satırlarını temizleyen `commit-msg` hook'u (DEC-035)
- [ ] **P0-23** Backend solution iskeleti: `Api`, `Application`, `Domain`, `Infrastructure`, test projeleri; `Directory.Build.props`, merkezi paket yönetimi, `.editorconfig`, uyarıları hata sayan analyzer'lar
- [ ] **P0-24** Frontend iskeleti: standalone component'lerle Angular admin uygulaması, ESLint, Prettier, birim test çalıştırıcısı
- [ ] **P0-25** Docker Compose ile yerel ortam: PostgreSQL, Seq; her şeyi tek komutla başlatma
- [ ] **P0-26** Veritabanı bağlantısını kontrol eden sağlık endpoint'leri (`/health/live`, `/health/ready`)
- [ ] **P0-27** İlk EF Core migration'ı (migration geçmişi olan boş şema)
- [ ] **P0-28** CI pipeline'ı: her PR'da restore, build, format kontrolü, lint, testler (backend ve frontend)
- [ ] **P0-29** API ve admin uygulaması için production'a hazır Docker imajları (multi-stage build)
- [ ] **P0-30** Sunucu: VPS, alan adı, HTTPS (reverse proxy), güvenlik duvarı, root olmayan deploy kullanıcısı, staging ve production ortamları
- [ ] **P0-31** CD pipeline'ı: `main`'e gelen kodun staging'e otomatik deploy'u, production'a elle onayla deploy, deploy sonrası sağlık kontrolü, migration'lardan önce veritabanı yedeği
- [ ] **P0-32** Geri alma (rollback) bir kez prova edildi ve belgelendi → `docs/operations/runbook.md`

### 3.5 Backlog ve faz geçişi

- [ ] **P0-33** Faz 1'in her maddesi için GitHub issue'su açıldı
- [ ] **P0-34** `v0.0.1` etiketi
- [ ] **P0-35** Faz 0 geçişi → `docs/phases/phase-0-gate.md`, sahip tarafından onaylandı

**Çıkış kriterleri**

- Tüm planlama dokümanları onaylı ve repoda.
- Bir PR açmak CI'ı otomatik olarak çalıştırır; `main`'e merge staging'e deploy eder; production deploy'u onaydan sonra çalışır.
- Production'da `https://<alan-adı>/health/ready` sağlıklı yanıt döner.
- Yeni bir geliştirici (veya yeni bir agent oturumu) README ile projeyi 15 dakikanın altında yerelde başlatabilir.

---

## 4. Faz 1 — MVP (`v0.1.0`)

**Amaç:** bir okul kurulabilir, gruplarını yönetebilir ve tam tenant izolasyonuyla yoklama alabilir.
**Öğrenme hedefleri:** uygulamada Clean Architecture, DDD yapı taşları, derinlemesine EF Core, kimlik doğrulama ve yetkilendirme, doğrulama, hata yönetimi, test piramidi, gerçek bir backend ile Angular.
**Giriş kriterleri:** Faz 0 geçişi onaylandı.

Her kullanıcı hikayesi bir **dikey dilim (vertical slice)** olarak geliştirilir: domain, application, API, arayüz ve testler tek bir PR'da (veya birkaç küçük PR'da) birlikte. Bu, uygulamanın her maddeden sonra yayınlanabilir kalmasını sağlar.

### 4.1 İterasyon 1.1 — Yapı taşları ve kimlik doğrulama

- [ ] **P1-01** Domain yapı taşları: `Entity`, `AggregateRoot`, güçlü tipli ID'ler (UUID v7), domain event'leri, `Result` / `Error`
- [ ] **P1-02** Value object'ler: birim testleriyle `Email`, `PhoneNumber`, `Money`, `TimeRange`, `YearMonth`, `Slug`
- [ ] **P1-03** Application pipeline'ı: command/query handler arayüzleri; loglama, doğrulama (FluentValidation) ve transaction decorator'ları; Scrutor ile kayıt
- [ ] **P1-04** Kalıcılık (persistence) temeli: `DbContext`, entity yapılandırmaları, denetim interceptor'ı, `xmin` eşzamanlılık token'ları, commit sonrası domain event yayını
- [ ] **P1-05** Tenancy: JWT'den tenant bağlamı, global query filter'lar, yazma işlemlerinde koruma, `ITenantScoped`
- [ ] **P1-06** Hata yönetimi: global exception handler, ProblemDetails, doğrulama hatası formatı
- [ ] **P1-07** Mimari testleri: katman bağımlılıkları, tenant'a bağlı entity'lerde `TenantId` bulunması (US-02)
- [ ] **P1-08** Entegrasyon test altyapısı: `WebApplicationFactory`, Testcontainers PostgreSQL, test verisi builder'ları, iki tenant'lı fixture
- [ ] **P1-09** US-03 Giriş yapma, hesap kilitleme dahil
- [ ] **P1-10** US-04 Refresh token rotation, tekrar kullanım tespiti, çıkış
- [ ] **P1-11** US-05 İlk giriş ve şifre değiştirme; admin tarafından şifre sıfırlama
- [ ] **P1-12** US-01 Platform Admin'in okul ve ilk admin oluşturması; ilk Platform Admin için seed
- [ ] **P1-13** Angular: uygulama kabuğu, giriş sayfası, token yenilemeli auth interceptor, rol bazlı route guard'lar, role göre layout
- [ ] **P1-14** OpenAPI dokümanı ve build içinde üretilen Angular API client'ı

### 4.2 İterasyon 1.2 — Okul kurulumu ve gruplar

- [ ] **P1-15** US-06 Branş yönetimi
- [ ] **P1-16** US-07 Tesis yönetimi
- [ ] **P1-17** US-08 Antrenör yönetimi (`UserDeactivated` event'i dahil)
- [ ] **P1-18** US-09 Grup oluşturma
- [ ] **P1-19** US-10 Çakışma kontrolleriyle haftalık program (`ScheduleConflictChecker`)
- [ ] **P1-20** US-11 Grubu düzenleme ve arşivleme (`GroupArchived` event'i dahil)
- [ ] **P1-21** US-12 Antrenör ana ekranı: kendi grupları ve bugünkü antrenmanlar (`SessionCalendar`)

### 4.3 İterasyon 1.3 — Öğrenciler ve kayıt

- [ ] **P1-22** US-13 Öğrenci kaydetme: çocuk ve yetişkin öğrenciler, acil durum kişisi, telefon üzerinden paylaşılan veliler
- [ ] **P1-23** US-14 Sadece acil durum kişisini gösteren antrenör görünümü ve yanıt contract testi
- [ ] **P1-24** US-15 Öğrenciyi ayrıldı olarak işaretleme (`StudentLeft` event'i dahil)
- [ ] **P1-25** US-16 Öğrenci arama ve listeleme: Türkçe karakter duyarsız arama, filtreler, sayfalama
- [ ] **P1-26** US-17 Öğrenci kaydı (`EnrollmentPolicy`, ADR-0005'e göre eşzamanlılık, eşzamanlılık testi)
- [ ] **P1-27** US-18 Kaydı sonlandırma

### 4.4 İterasyon 1.4 — Yoklama

- [ ] **P1-28** US-19 Yoklama alma (antrenmanın tembel oluşturulması, idempotent kaydetme)
- [ ] **P1-29** US-20 48 saat kuralı ve değişiklik geçmişiyle yoklama düzeltme
- [ ] **P1-30** US-21 Öğrenci ve grup bazında yoklama geçmişi

### 4.5 İterasyon 1.5 — Aidatlar (Should) ve duyurular (Could)

- [ ] **P1-31** US-22 Grup başına aylık aidatlar
- [ ] **P1-32** US-23 Ödeme kaydetme ve iptal etme
- [ ] **P1-33** US-24 Ödenmemiş borçlar (`DueCalculator`)
- [ ] **P1-34** US-25 Grup duyuruları (Could; faz geçişinde faz 2'ye taşınabilir)

### 4.6 Sürüm çıkarma

- [ ] **P1-35** Tenant izolasyon test paketi: her endpoint başka bir okulun kullanıcısı olarak çağrılır
- [ ] **P1-36** Kritik akışlar için uçtan uca testler (Playwright): giriş, grup kurma, kayıt, yoklama alma
- [ ] **P1-37** MVP güvenlik kontrol listesi: şifre hash'leme, token saklama, CORS, sadece HTTPS, repoda gizli bilgi olmaması, bağımlılık güvenlik açığı taraması
- [ ] **P1-38** İki okul için gerçekçi demo verisi üreten seed betiği
- [ ] **P1-39** Sürüm notları ve `CHANGELOG.md`, `v0.1.0` etiketi, production deploy
- [ ] **P1-40** Faz 1 geçişi → `docs/phases/phase-1-gate.md`, sahip tarafından onaylandı

**Çıkış kriterleri**

- Tüm Must hikayeleri tamamlandı; Should ve Could hikayeleri tamamlandı ya da açıkça taşındı.
- `vision.md`'deki (bölüm 7) MVP başarı kriterleri karşılandı ve gösterildi.
- Tüm test paketi CI'da 10 dakikanın altında çalışıyor.

---

## 5. Faz 2 — İş mantığı derinliği (`v0.2.0`)

**Amaç:** aidatları ve iletişimi gerçek hale getirmek ve işleri arka planda güvenilir şekilde çalıştırmak.
**Öğrenme hedefleri:** üçüncü parti entegrasyonu, webhook'lar, idempotency key'ler, arka plan işleri, outbox pattern, cache, dosya depolama, e-posta gönderimi.
**Giriş kriterleri:** Faz 1 geçişi onaylandı. Detaylı görevler o geçişte yazılır.

**Kapsam**

- [ ] **P2-F1** Webhook'lar ve idempotency dahil, sandbox modunda bir ödeme sağlayıcısıyla (iyzico) online aidat ödemesi
- [ ] **P2-F2** Hangfire ile arka plan işleri ve adminler için iş paneli
- [ ] **P2-F3** Domain event'lerinin güvenilir işlenmesi için transactional outbox
- [ ] **P2-F4** E-posta bildirimleri: aidat hatırlatmaları, hoş geldin e-postaları, kendi kendine şifre sıfırlama
- [ ] **P2-F5** Her okulun saklama politikasına göre otomatik anonimleştirme işi
- [ ] **P2-F6** Tür ve boyut doğrulaması ve süreli bağlantılarla S3 uyumlu depolamaya (MinIO) belge yükleme (sağlık raporu, fotoğraf)
- [ ] **P2-F7** Okuma ağırlıklı sorgular için Redis ve `HybridCache` ile cache
- [ ] **P2-F8** Aidat kuralları: kardeş indirimi ve dönem ortasında aidat değişikliği (geçişte detaylandırılacak)

**Çıkış kriterleri (taslak):** bir veli sandbox sağlayıcısı üzerinden bir aidatı uçtan uca ödeyebilir; başarısız bir webhook çift tahsilat olmadan yeniden denenir; anonimleştirme zamanında çalışır ve her çalışma loglanır.

---

## 6. Faz 3 — Veli deneyimi (`v0.3.0`)

**Amaç:** veliler platformu doğrudan telefonlarından kullanır.
**Öğrenme hedefleri:** React, aynı API'ye ikinci bir istemci, PWA, gerçek zamanlı iletişim, veri görselleştirme, çoklu dil, erişilebilirlik.
**Giriş kriterleri:** Faz 2 geçişi onaylandı.

**Kapsam**

- [ ] **P3-F1** Davet akışıyla veli hesapları
- [ ] **P3-F2** PWA olarak React veli uygulaması: çocuklar, programlar, yoklama, borçlar, online ödeme
- [ ] **P3-F3** SignalR ile gerçek zamanlı duyurular ve yoklama güncellemeleri
- [ ] **P3-F4** Admin dashboard'ları: devam oranları, doluluk, gelir
- [ ] **P3-F5** Her iki istemcide çoklu dil (Türkçe ve İngilizce)
- [ ] **P3-F6** Önemli ekranların erişilebilirlik denetimi (WCAG 2.2 AA)
- [ ] **P3-F7** Okul başına alt alan adının değerlendirilmesi (karar ADR olarak kaydedilir)

---

## 7. Faz 4 — Production sağlamlaştırma (`v0.4.0`)

**Amaç:** platform, gerçek veriye emanet edilecek kadar güvenli, gözlemlenebilir ve hızlıdır.
**Öğrenme hedefleri:** OWASP Top 10, katmanlı savunma (defence in depth), gözlemlenebilirlik, performans analizi, operasyon.
**Giriş kriterleri:** Faz 3 geçişi onaylandı.

**Kapsam**

- [ ] **P4-F1** OWASP Top 10'a göre güvenlik incelemesi; güvenlik başlıkları; rate limiting
- [ ] **P4-F2** Tenant izolasyonunun ikinci katmanı olarak PostgreSQL Row Level Security
- [ ] **P4-F3** Kişisel verilere erişim ve değişikliklerin audit log'u
- [ ] **P4-F4** KVKK ilgili kişi başvuruları: bir kişinin verisini dışa aktarma ve silme
- [ ] **P4-F5** OpenTelemetry ile gözlemlenebilirlik: trace, metrik, log, dashboard ve alarmlar; Sentry ile hata takibi
- [ ] **P4-F6** k6 ile yük testi; N+1 sorgu ve eksik index düzeltmeleri; performans bütçesi
- [ ] **P4-F7** Yedekten geri yükleme tatbikatı; belgelenmiş kurtarma süresi
- [ ] **P4-F8** Kademeli yayın için feature flag'ler

---

## 8. Faz 5 — Ölçek ve mimari (`v1.0.0`)

**Amaç:** dağıtık sistemleri, mantıklı oldukları yerde uygulayarak ve bedellerini bilerek anlamak.
**Öğrenme hedefleri:** mesaj kuyrukları (message broker), eventual consistency, servis sınırları, bulut platformları, infrastructure as code.
**Giriş kriterleri:** Faz 4 geçişi onaylandı.

**Kapsam**

- [ ] **P5-F1** Modüller arası entegrasyon event'leri için RabbitMQ
- [ ] **P5-F2** Bildirim modülünün ayrı bir servise çıkarılması; ödünleşimlerin belgelenmesi
- [ ] **P5-F3** Barındırmanın Azure'a taşınması (Container Apps, PostgreSQL Flexible Server)
- [ ] **P5-F4** Terraform ile infrastructure as code
- [ ] **P5-F5** Son mimari inceleme ve `v1.0.0` retrospektifi: neyi farklı yapardık

---

## 9. Değişiklik günlüğü

Kapsamdaki veya onaylanmış bir plandaki her değişiklik buraya kaydedilir.

| Tarih | Değişiklik | Gerekçe | Onaylayan |
| --- | --- | --- | --- |
| 2026-10-27 | Yol haritası oluşturuldu | — | Sahip |
| 2026-10-27 | Dokümantasyon dili Türkçe olarak değiştirildi (DEC-030) | Okunabilirlik | Sahip |
| 2026-10-27 | P0-21 ve P0-22'ye yazarlık ayarları eklendi (DEC-035) | Git geçmişinde ve dokümanlarda sadece sahibin adının geçmesi | Sahip |
| 2026-10-27 | MVP kapsamına yetişkin öğrenciler eklendi (P1-22, US-13, US-14) | Okullar yetişkin kursları da veriyor | Sahip |
| 2026-10-27 | P0-36 (ADR-0009) eklendi | Mimariye genel bakış yazılırken iki tutarsızlık bulundu | Sahip |
| 2026-10-27 | P0-22: repo private yerine public (DEC-072) | GitHub Free'de branch koruması sadece public repolarda; portföy | Sahip |
| 2026-10-27 | Lisans verilmemesi kararı (DEC-073) | Ürünün ileride ticari olarak satılması planlanıyor | Sahip |