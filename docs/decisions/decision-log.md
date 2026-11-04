# Karar Günlüğü

> Repodaki yeri: `docs/decisions/decision-log.md`

Ürünü veya mimariyi şekillendiren her karar, değişmeyen bir ID ile burada listelenir. Büyük kararlar, tüm gerekçeleriyle `docs/adr/` altında bir ADR'a sahiptir; bu günlük onların dizinidir.

**Durumlar:** `Proposed` (önerildi) → `Accepted` (kabul edildi) → (`Superseded by DEC-xxx` (yerine geçildi) | `Retired` (kaldırıldı))

**Kurallar**

- Bir karar ancak sahip onayladıktan sonra `Accepted` olur.
- Kabul edilmiş bir karar asla düzenlenmez. Değiştirmek için, onun yerine geçen yeni bir karar eklenir ve eski satırın durumu güncellenir.
- Her faz geçişinde tüm `Accepted` kararlar gözden geçirilir. *Son inceleme* sütunu, hangi geçişte incelendiğini kaydeder.
- Yapay zeka agent'ı karar önerebilir ama bir kararı asla kabul edemez, değiştiremez veya kaldıramaz.

## Ürün

| ID | Karar | Kaynak | Durum | Kabul tarihi | Son inceleme |
| --- | --- | --- | --- | --- | --- |
| DEC-001 | İlk günden multi-tenant: tek platformda birçok okul | vision §3 | Accepted | 2026-10-27 | — |
| DEC-002 | Roller: Platform Admin, Okul Admini, Antrenör; velilerin faz 3'e kadar girişi yok | vision §4 | Accepted | 2026-10-27 | — |
| DEC-003 | Öğrenciler velilere bağlı profillerdir, asla kullanıcı değildir | vision §4, ADR-0002 | Accepted | 2026-10-27 | — |
| DEC-004 | Bir kullanıcı tam olarak bir okula bağlıdır; e-posta platform genelinde benzersizdir | vision §5 | Accepted | 2026-10-27 | — |
| DEC-005 | MVP'de okul içinde şube kavramı yok | vision §5 | Accepted | 2026-10-27 | — |
| DEC-006 | Aidatlar MVP'de Should önceliğinde: sadece elle ödeme kaydı | vision §5 | Accepted | 2026-10-27 | — |
| DEC-007 | Veli bilgilerini sadece Okul Adminleri görür; antrenörler kendi öğrencileri için tek bir acil durum telefonu görür | vision §9.1 | Accepted | 2026-10-27 | — |
| DEC-008 | Öğrenci ayrıldıktan sonra kişisel veriler ayarlanabilir bir süre sonunda anonimleştirilir; ödemeler yasal süre boyunca saklanır | vision §9.2 | Accepted | 2026-10-27 | — |
| DEC-009 | Bir öğrenci birden fazla gruba kayıtlı olabilir; çakışan program saati ve aynı gruba mükerrer kayıt olamaz | vision §9.3 | Accepted | 2026-10-27 | — |
| DEC-010 | Hesap kilitleme: 15 dakika içinde 5 başarısız girişten sonra 15 dakika | US-03 | Accepted | 2026-10-27 | — |
| DEC-011 | Gruplar yaş aralığı değil, doğum yılı aralığı kullanır | US-09 | Accepted | 2026-10-27 | — |
| DEC-012 | Gruplar arası tesis çakışmaları hata değil uyarı üretir | US-10 | Accepted | 2026-10-27 | — |
| DEC-013 | Öğrenci isim araması büyük-küçük harf ve Türkçe karakter farkını gözetmez | US-16 | Accepted | 2026-10-27 | — |
| DEC-014 | Antrenörler yoklamayı antrenmandan sonraki 48 saate kadar düzenleyebilir; adminler her zaman | US-20 | Accepted | 2026-10-27 | — |

## Domain

| ID | Karar | Kaynak | Durum | Kabul tarihi | Son inceleme |
| --- | --- | --- | --- | --- | --- |
| DEC-015 | Antrenör, `Coach` rolüne sahip bir User'dır; ayrı bir varlık yoktur | domain D1 | Accepted | 2026-10-27 | — |
| DEC-016 | Antrenmanlar ilk yoklamada, saat aralığının anlık görüntüsüyle tembel olarak oluşturulur | domain D2 | Accepted | 2026-10-27 | — |
| DEC-017 | En az bir aktif günü olan her ay için tam aylık aidat; MVP'de kıst hesaplama yok | domain D3 | Accepted | 2026-10-27 | — |
| DEC-018 | Enrollment ayrı bir aggregate'tır | domain D4 | Accepted | 2026-10-27 | — |
| DEC-019 | Genel soft delete yok; bunun yerine iş anlamı taşıyan durumlar | domain D5 | Accepted | 2026-10-27 | — |
| DEC-020 | Cinsiyet değerleri `Female` / `Male` | domain D6 | Accepted | 2026-10-27 | — |
| DEC-021 | Borçlar hesaplanır, asla saklanmaz | domain §4.10 | Accepted | 2026-10-27 | — |
| DEC-022 | ID'ler UUID v7'dir | domain §8 | Accepted | 2026-10-27 | — |

## Teknoloji ve mimari

| ID | Karar | Kaynak | Durum | Kabul tarihi | Son inceleme |
| --- | --- | --- | --- | --- | --- |
| DEC-023 | Backend: .NET 10 (LTS) üzerinde ASP.NET Core, EF Core, PostgreSQL | teknoloji görüşmesi | Accepted | 2026-10-27 | — |
| DEC-024 | Admin uygulaması Angular; veli uygulaması React (faz 3) | teknoloji görüşmesi | Accepted | 2026-10-27 | — |
| DEC-025 | Clean Architecture katmanlarıyla modüler monolit | ADR-0001 | Accepted | 2026-10-27 | — |
| DEC-026 | Paylaşımlı veritabanı, `TenantId` ve EF Core global query filter'ları ile multi-tenancy; tenant JWT'den alınır | ADR-0003 | Accepted | 2026-10-27 | — |
| DEC-027 | MediatR veya AutoMapper yok: projeye ait dispatcher ve elle mapping (DEC-046 ile netleştirildi: ayrı dispatcher sınıfı yok) | ADR-0004 | Accepted | 2026-10-27 | — |
| DEC-028 | Kod, dokümanlar ve commit'ler İngilizce; agent ile konuşma Türkçe | dil görüşmesi | Superseded by DEC-030 | 2026-10-27 | — |
| DEC-029 | Sahip tarafından onaylanan faz geçişleriyle rolling-wave planlama | roadmap §1 | Accepted | 2026-10-27 | — |
| DEC-036 | MVP'de dokuz modül: Tenancy, Identity, Organization, Groups, Students, Enrollments, Attendance, Billing, Announcements | ADR-0001 | Accepted | 2026-10-27 | — |
| DEC-037 | Modüller ayrı projeler değil, katman projeleri içinde klasörlerdir; sınırları mimari testleri korur | ADR-0001 | Accepted | 2026-10-27 | — |
| DEC-038 | Tek `DbContext`; her modülün tabloları kendi PostgreSQL şemasında; MVP'de modüller arası foreign key'lere izin var | ADR-0001 | Accepted | 2026-10-27 | — |
| DEC-039 | Bir modül başka bir modülün aggregate'ını değiştiremez; değişiklikler domain event'leriyle, okumalar modülün yayınladığı okuma arayüzleriyle yapılır (DEC-065 ile genişletildi) | ADR-0001 | Accepted | 2026-10-27 | — |
| DEC-040 | Tenant'a bağlı tablolar arasındaki foreign key'ler `TenantId`'yi içerir (bileşik foreign key); okullar arası referans veritabanı düzeyinde imkansızdır | ADR-0003 | Accepted | 2026-10-27 | — |
| DEC-041 | `IgnoreQueryFilters()` sadece `Tenancy` modülünde kullanılabilir; ham SQL istisnadır ve tenant koşulunu açıkça içermelidir | ADR-0003 | Accepted | 2026-10-27 | — |
| DEC-042 | Başka bir okula ait kayıt istendiğinde yanıt `404`'tür | ADR-0003 | Accepted | 2026-10-27 | — |
| DEC-043 | Yetişkin öğrenciler desteklenir: veli sadece reşit olmayanlar (kontrol tarihinde 18 yaş altı, okulun saat dilimine göre) için zorunlu; yetişkinler için kendi telefonu zorunlu. DEC-003'ü genişletir | ADR-0002 | Accepted | 2026-10-27 | — |
| DEC-044 | Acil durum kişisi, velilerden bağımsız olarak her öğrenci için zorunlu bir value object'tir (ad, telefon, yakınlık) | ADR-0002 | Accepted | 2026-10-27 | — |
| DEC-045 | Reşit olmayan öğrenciler hiçbir zaman kullanıcı olmaz; yetişkin öğrencilerin girişi faz 3'te değerlendirilir | ADR-0002 | Accepted | 2026-10-27 | — |
| DEC-046 | Ayrı bir dispatcher sınıfı yok: endpoint'ler handler arayüzlerini doğrudan alır; loglama, doğrulama ve transaction decorator'larla yapılır (bu sırayla); kayıt için Scrutor. DEC-027'yi netleştirir | ADR-0004 | Accepted | 2026-10-27 | — |
| DEC-047 | Command'lar aggregate üzerinden durumu değiştirir; query'ler aggregate yüklemeden doğrudan DTO'ya projekte eder (`AsNoTracking`) | ADR-0004 | Accepted | 2026-10-27 | — |
| DEC-048 | Mapping elle yapılır; domain entity'leri asla API yanıtı olarak dönmez; `MediatR` ve `AutoMapper` CI'da yasaklı paketlerdir | ADR-0004 | Accepted | 2026-10-27 | — |
| DEC-049 | Kayıt ve kontenjan değişikliği sırasında öğrenci ve grup satırları `SELECT ... FOR UPDATE` ile, her zaman önce öğrenci sonra grup sırasıyla kilitlenir; kilit bekleme süresi 5 saniye | ADR-0005 | Accepted | 2026-10-27 | — |
| DEC-050 | Mükerrer aktif kayda karşı kısmi benzersiz index: `(tenant_id, student_id, group_id) WHERE status = 'Active'` | ADR-0005 | Accepted | 2026-10-27 | — |
| DEC-051 | Kimlik doğrulama kendi `Identity` modülümüzde; şifreler için ASP.NET Core `PasswordHasher`, JWT için .NET'in standart kütüphaneleri; ASP.NET Core Identity'nin geri kalanı ve harici kimlik sağlayıcı kullanılmaz | ADR-0006 | Accepted | 2026-10-27 | — |
| DEC-052 | Access token: HS256 imzalı JWT; claim'ler `sub`, `role`, `tenant_id`, `pwd_change_required`, `jti`; kişisel veri içermez | ADR-0006 | Accepted | 2026-10-27 | — |
| DEC-053 | Refresh token: 256 bitlik rastgele opak değer, veritabanında SHA-256 hash'i; `HttpOnly; Secure; SameSite=Strict; Path=/api/v1/auth` cookie'de. Access token sadece bellekte | ADR-0006 | Accepted | 2026-10-27 | — |
| DEC-054 | Angular uygulaması ve API, reverse proxy arkasında aynı origin'den sunulur (`/api`); CORS yapılandırması yok | ADR-0006 | Accepted | 2026-10-27 | — |
| DEC-055 | Refresh ve çıkış endpoint'leri sadece `POST` ve `X-Requested-With` başlığıyla çalışır | ADR-0006 | Accepted | 2026-10-27 | — |
| DEC-056 | Pasif hale getirilen kullanıcının access token'ı en fazla 15 dakika geçerli kalabilir (kabul edilen sınır) | ADR-0006 | Accepted | 2026-10-27 | — |
| DEC-057 | Beklenen hatalar projeye ait `Result<T>` / `Error` ile döner; exception sadece beklenmeyen hatalar için | ADR-0007 | Accepted | 2026-10-27 | — |
| DEC-058 | Hata türleri ve HTTP karşılıkları: Validation 400, Unauthorized 401, Forbidden 403, NotFound 404, Conflict 409, BusinessRule 422; beklenmeyen hatalar 500 ve iç ayrıntı içermez | ADR-0007 | Accepted | 2026-10-27 | — |
| DEC-059 | API, sabit hata kodlarıyla (`<Modül>.<Hata>`) ProblemDetails döner; kullanıcıya gösterilen Türkçe metni istemci, kendi dil dosyasından üretir | ADR-0007 | Accepted | 2026-10-27 | — |
| DEC-060 | Veritabanına dokunan kod mock'larla değil, Testcontainers ile gerçek PostgreSQL üzerinde ve API üzerinden test edilir; mock'lar sadece dış sistemler için | ADR-0008 | Accepted | 2026-10-27 | — |
| DEC-061 | Test araçları: xUnit, Shouldly, NSubstitute, Testcontainers, Respawn, FakeTimeProvider, NetArchTest, Coverlet, Playwright, k6; Fluent Assertions kullanılmaz | ADR-0008 | Accepted | 2026-10-27 | — |
| DEC-062 | Kabul kriteri testleri hikaye ID'siyle etiketlenir (`[Trait("Story", "US-xx")]`); testler koddan önce yazılır | ADR-0008 | Accepted | 2026-10-27 | — |
| DEC-063 | Her PR'da birim, mimari ve entegrasyon testleri; uçtan uca testler staging deploy'undan sonra; tüm tarayıcılarda gecelik | ADR-0008 | Accepted | 2026-10-27 | — |
| DEC-064 | Domain event'leri aynı transaction içinde, commit'ten önce işlenir; herhangi bir adım başarısız olursa her şey geri alınır. Event işleyicileri sadece veritabanı içi değişiklik yapar; dış sistem çağrıları faz 2'de outbox ile | ADR-0009 | Accepted | 2026-10-27 | — |
| DEC-065 | Her modül `Contracts` klasöründe okuma arayüzleri ve (istisnai olarak) senkron komutlar yayınlar; modüller birbirine sadece bunlarla erişir; döngüsel bağımlılık yok. DEC-039'u genişletir | ADR-0009 | Accepted | 2026-10-27 | — |
| DEC-066 | Para tutarları JSON'da metin olarak gönderilir: `{ "amount": "1250.00", "currency": "TRY" }` | API A1 | Accepted | 2026-10-27 | — |
| DEC-067 | Durum geçişleri adı olan aksiyon endpoint'leridir (`POST /students/{id}/leave`) | API A2 | Accepted | 2026-10-27 | — |
| DEC-068 | Offset sayfalama; yanıtta `items`, `page`, `pageSize`, `totalCount`, `totalPages` | API A3 | Accepted | 2026-10-27 | — |
| DEC-069 | Eşzamanlı düzenlemeye karşı `version` alanı; uyuşmazlıkta `409 Concurrency.Conflict` | API A4 | Accepted | 2026-10-27 | — |
| DEC-070 | Engelleyici olmayan durumlar başarılı yanıtta `warnings` dizisiyle döner | API A5 | Accepted | 2026-10-27 | — |
| DEC-071 | Her endpoint tek bir yanıt şekli döner; rol bazlı görünümler ayrı endpoint'tir | API A6 | Accepted | 2026-10-27 | — |
| DEC-030 | Dokümanlar, ADR'lar, issue/PR açıklamaları ve `CLAUDE.md` Türkçe; kod, tanımlayıcılar, dosya adları, enum değerleri, commit mesajları ve teknik terimler İngilizce | sahibin talebi | Accepted | 2026-10-27 | — |
| DEC-074 | `CA1707` (isimde alt çizgi) ve `CA1515` (Exe'deki public tipler `internal` olmalı) sadece `tests/` altındaki dosyalar için `.editorconfig`'te kapatılır: test isimleri `Metot_Durum_Sonuç` biçimindedir (ADR-0008) ve xUnit sadece public test sınıflarını bulur. `src/` altında hiçbir analyzer kuralı kapatılmaz | P0-23, sahibin onayı | Accepted | 2026-10-30 | — |
| DEC-075 | Node.js 26 LTS; `.nvmrc` ve `engines` ile sabitlenir. Angular 22 `>=26.0.0`'ı destekliyor; Node 24 2026-10-20'de Maintenance'a geçti (EOL 2028-04-30), Node 26 Active LTS (EOL 2029-04-30) | P0-24, sahibin onayı | Accepted | 2026-10-30 | — |
| DEC-076 | .NET testleri Microsoft.Testing.Platform (MTP) ile çalışır: `global.json`'da `"test": { "runner": "Microsoft.Testing.Platform" }` ile `dotnet test`'in MTP uygulaması seçilir. Gerekçe: .NET 10 SDK'sında `dotnet test`'in hem VSTest hem MTP uygulaması var; xUnit v3 MTP kullanıyor ve MTP projeleri .NET 10 SDK'sında VSTest tabanlı `dotnet test` ile çalıştırılamıyor (Microsoft Learn, "Migrating from Microsoft.Testing.Platform (MTP) v1 to v2"). VSTest paketleri (`Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `coverlet.collector`) kullanılmaz; kapsam ölçümü `coverlet.MTP` ile yapılır; filtreler `--filter-trait`, `--filter-class` gibi MTP seçenekleridir. Geçici istisna: henüz testi olmayan üç test projesi (`Domain.UnitTests`, `Application.UnitTests`, `IntegrationTests`) kendi csproj dosyalarında "sıfır test" çıkış kodunu (8) yoksayar; solution geneli bir ayar yoktur. İlk testler eklendiğinde kaldırılır (#12) | P0-23 (#5), sahibin onayı | Accepted | 2026-10-30 | — |
| DEC-077 | Metin dosyalarının satır sonu her yerde LF'tir: `.gitattributes`'ta `* text=auto eol=lf` (çalışma kopyası da `core.autocrlf` ayarından bağımsız olarak LF), `.editorconfig`'te `end_of_line = lf`, Prettier'da `endOfLine: "lf"`. P0-24'te Git for Windows'un `core.autocrlf=true` ayarı çalışma kopyasını CRLF yaptığı için Prettier'da kullanılan `endOfLine: "auto"` istisnasının yerine geçer | P0-24 (#16), sahibin onayı | Accepted | 2026-11-02 | — |
| DEC-078 | Production'da migration'lar uygulama başlarken çalıştırılmaz (`Migrate()` / `EnsureCreated()` yok). Deploy sırasında tek seferlik bir migration bundle olarak, `spor_akademi_migrator` kullanıcısıyla uygulanır. Sıra: yedek → bundle → yeni API → sağlık kontrolü. Çalışacak SQL PR'da `migrations script` ile incelenir. Staging ve production'da aynı bundle kullanılır. Production'da `Down` çalıştırılmaz; her migration bir önceki API sürümüyle uyumludur (expand–contract) | ADR-0011 (#23), sahibin onayı | Accepted | 2026-11-02 | — |
| DEC-079 | Veritabanındaki tüm adlar snake_case'tir: tablolar, sütunlar, primary key, foreign key, index ve constraint'ler, EF Core'un migration geçmişi tablosu (`public.__ef_migrations_history`) dahil. Şemalar modül adının snake_case hâlidir (`groups`), tablo adları DbSet adından gelir ve çoğuldur (`groups.groups`). Uygulama: `EFCore.NamingConventions` paketi (Apache-2.0) ve `UseSnakeCaseNamingConvention()`; elle `ToTable` / `HasColumnName` yazılmaz | P0-27, ADR-0003, ADR-0005, sahibin onayı | Accepted | 2026-11-02 | — |
| DEC-080 | Bağımlılık güvenliği ve tedarik zinciri. **Eşik:** high ve critical güvenlik açıkları CI'ı ve build'i kırar; low ve moderate bulgular raporlanır ve her faz geçişinde gözden geçirilir. .NET'te her restore doğrudan ve dolaylı paketleri denetler: `TreatWarningsAsErrors` altında `NU1903`/`NU1904` hata, `NU1901`/`NU1902` `WarningsNotAsErrors` ile uyarı olarak kalır; uyarılar gizlenmez, sadece build'i kırmaz (`Directory.Build.props`). npm'de `npm audit --audit-level=high`. **Fail-closed:** açık veritabanına ulaşılamazsa (`NU1900`, `NU1905`) build geçmez; "kontrol edemedim" ile "kontrol ettim, temiz" aynı sonucu vermez. **İstisna:** hemen düzeltilemeyen bir high veya critical açık için kontrol gevşetilmez; sadece o açığın advisory URL'siyle bir `NuGetAuditSuppress` öğesi eklenir. Her istisna sahibin onayını, gerekçeyi ve bir gözden geçirme tarihini gerektirir ve öğenin yanında yazılır. npm için istisna yöntemi gerektiğinde ayrıca kararlaştırılır. **Sonuç:** dün geçen bir build, kodda değişiklik olmadan, yeni yayınlanan bir advisory yüzünden bugün kırılabilir; çözüm Dependabot'un güvenlik PR'ı veya paketin elle güncellenmesidir. **Tedarik zinciri:** GitHub Actions commit hash'iyle sabitlenir (yorumda sürüm); yeni sürümler hemen alınmaz, Dependabot sürüm güncellemelerinde `cooldown` ile bir bekleme süresi uygulanır (güvenlik güncellemelerine uygulanmaz). **İmaj taraması (P0-29, #47):** container imajları Trivy ile, aynı eşikle taranır (high ve critical kırar). Mart 2026'daki tedarik zinciri saldırısı (GHSA-69fq-xp46-6x23, CVE-2026-33634: trivy-action'ın 76 tag'i ve setup-trivy'nin tüm tag'leri ele geçirildi, zararlı Trivy v0.69.4 yayınlandı) nedeniyle: trivy-action, olay sonrası yayınlanmış immutable bir sürümün (v0.36.0) commit hash'iyle sabitlenir ve setup-trivy'yi de commit hash'iyle çağırır; Trivy binary sürümü açıkça verilir (`latest` asla kullanılmaz), kurulum betiği tarball'ı release'in SHA-256 checksum'ına karşı doğrular ve Trivy release'leri immutable'dır; tarama job'ı gizli bilgi kullanmaz, token salt okunurdur ve diske yazılmaz | P0-28 (#30), P0-29 (#47), NFR-SEC-11, NuGet "Auditing package dependencies", GitHub "Dependabot options reference", GHSA-69fq-xp46-6x23, sahibin onayı | Accepted | 2026-11-03 | — |
| DEC-081 | Container imajları. Base imajlar tag **ve** digest ile sabitlenir (`imaj:tag@sha256:…`) ve Dependabot'un `docker` ekosistemiyle birlikte güncellenir. İmajlar root olmayan kullanıcıyla çalışır. Build context'i izin listesi biçimindeki `.dockerignore` ile sınırlanır; `.env` gibi gizli bilgiler imaja giremez (NFR-SEC-09). Bağlantı bilgisi imajda değil, çalışma anında ortam değişkeniyle gelir. **API:** build `mcr.microsoft.com/dotnet/sdk` (noble), çalışma `mcr.microsoft.com/dotnet/aspnet` **noble-chiseled-extra**: shell ve paket yöneticisi yok; `-extra` varyantı okul saat dilimleri ve tr-TR biçimleri için tzdata ve ICU içerir. API başlarken `Europe/Istanbul` ve `tr-TR`'yi yükleyemezse açık bir hatayla durur (fail-fast); varsayılan chiseled imaja geçiş sessizce production'a gidemez. Chiseled imajda shell olmadığı için Dockerfile'da `HEALTHCHECK` yoktur; sağlık kontrolü dışarıdan `/health/*` ile yapılır. **Admin uygulaması:** `nginxinc/nginx-unprivileged` (P0-30'da reverse proxy seçimiyle yeniden değerlendirilir) | P0-29 (#47), NFR-SEC-09, dotnet-docker "Ubuntu Chiseled" ve "image-variants", sahibin onayı | Accepted | 2026-11-03 | — |
| DEC-082 | Barındırma: staging ve production **tek VPS'te** çalışır. Birincil seçenek Hetzner Cloud **CX33** (4 vCPU, 8 GB RAM, 80 GB NVMe, eu-central; €8,99/ay, KDV hariç, IPv4 dahil); stokta yoksa OVHcloud **VPS-2** (4 vCore, 8 GB RAM, 75 GB NVMe; $8,50/ay'dan başlayan fiyat). Fiyatlar 2026-11-04'te sağlayıcıların sitelerinden alındı; o gün Hetzner'in sayfasında paylaşımlı planlar "currently unavailable" görünüyordu. Satın alma Faz 0'ın sonunda, sunucu gerektiren işlerle birlikte yapılır | P0-30 (#53), DEC-033, DEC-034, sahibin kararı | Accepted | 2026-11-04 | — |
| DEC-083 | Alan adı **Cloudflare Registrar**'dan alınır (fiyata kâr eklenmez). DNS kayıtları "**DNS only**" olur (Cloudflare proxy'si kapalı): trafik doğrudan sunucuya gelir, TLS sunucuda sonlanır | P0-30 (#53), sahibin kararı | Accepted | 2026-11-04 | — |
| DEC-084 | Reverse proxy **Caddy**: Let's Encrypt sertifikasını kendisi alıp yeniler ve HTTP'yi HTTPS'e yönlendirir; NFR-SEC-02 ek bir araç (certbot gibi) olmadan karşılanır | P0-30 (#53), DEC-054, NFR-SEC-02, sahibin kararı | Accepted | 2026-11-04 | — |
| DEC-085 | Sunucu dışı yedek deposu **Cloudflare R2**. Yedekler sunucuda şifrelenir, R2'ye şifreli hâlde gönderilir; yedek, sunucudan farklı bir sağlayıcıda durur. Ücretsiz katman (10 GB/ay) sentetik veriyle (DEC-031) yeterli olmalı (NFR-CST-02) | P0-30 (#53), NFR-REL-03, sahibin kararı | Accepted | 2026-11-04 | — |
| DEC-086 | Staging ve production logları sunucudaki **Seq**'te toplanır. Seq dışarıya açılmaz; sadece SSH tüneliyle erişilir | P0-30 (#53), NFR-OBS-01, sahibin kararı | Accepted | 2026-11-04 | — |
| DEC-087 | Alt alan adları: production `app.<alan-adı>`, staging `staging.<alan-adı>`; kök alan adı `app.`'e yönlendirilir | P0-30 (#53), DEC-083, sahibin kararı | Accepted | 2026-11-04 | — |
| DEC-088 | Admin uygulamasının imajı `nginxinc/nginx-unprivileged` olarak kalır: Caddy önde TLS'i ve `/api` yönlendirmesini yapar, nginx sadece statik dosyaları sunar. DEC-081'deki "P0-30'da reverse proxy seçimiyle yeniden değerlendirilir" notu bu kararla kapanır | P0-30 (#53), DEC-081, DEC-084, sahibin kararı | Accepted | 2026-11-04 | — |
| DEC-089 | İmaj dağıtımı: imajlar **GHCR**'de **public** paket olarak yayımlanır (repo public, imajlarda gizli bilgi yok; sunucu token olmadan çeker). Tag olarak **sadece commit SHA'sı** kullanılır; `latest` gibi değişen tag yoktur. Migration bundle'ı ayrı bir **migrator imajı** olarak yayımlanır; API ve admin imajları gibi duman testinden ve Trivy taramasından geçer (DEC-080) | P0-31, ADR-0011, DEC-080, DEC-081, sahibin kararı | Accepted | 2026-11-04 | — |
| DEC-090 | Staging ve production'ın **ayrı PostgreSQL instance'ları** vardır: birini sıfırlamak veya yedekten geri yüklemek diğerine dokunmaz | P0-30 (#53), DEC-082, sahibin kararı | Accepted | 2026-11-04 | — |
| DEC-091 | Yedekler **age** ile, açık anahtara (recipient) şifrelenir. Sunucuda ve repoda sadece açık anahtar bulunur. Gizli anahtarı sahip oluşturur ve iki ayrı yerde saklar; gizli anahtar repoya, sunucuya ve yapay zeka agent'ıyla yapılan sohbete asla girmez. Testler, çalışma anında üretilip atılan geçici bir anahtar çifti kullanır | P0-31, NFR-REL-03, DEC-085, sahibin kararı | Accepted | 2026-11-04 | — |
| DEC-092 | Admin imajının çalışma aşaması `nginxinc/nginx-unprivileged`'ın **`-alpine-slim`** varyantını kullanır (tag ve digest ile sabit, DEC-081). **Gerekçe:** imajda kullanılmayan paketleri güncellemek yerine hiç barındırmamayı tercih ediyoruz. Statik dosya sunan bir container `curl`'e ve yazı tipi kütüphanelerine (`freetype`, `fontconfig`; bunların getirdiği `brotli-libs` ve `libexpat`) ihtiyaç duymaz; slim varyantında bu paketler yoktur ve açıkları imaja hiç girmez. Tetikleyen olay: `1.30.6-alpine` imajında Trivy'nin bulduğu 1 critical ve 5 high açık (`brotli-libs`, `curl`/`libcurl`, `libexpat`) CI'ı kırdı (DEC-080); düzeltilmiş paketleri içeren bir upstream imaj henüz yoktu. Slim'e geçince paket sayısı 70'ten 21'e indi ve Trivy 0 buldu. Slim varyantı ileride yetersiz kalırsa (ör. düzeltme bekleyen bir açık) yöntem `apk upgrade` ile yeni bir kararla belirlenir | P0-29 (#47), DEC-080, DEC-081, DEC-088, sahibin kararı | Accepted | 2026-11-04 | — |

## Fonksiyonel olmayan gereksinimler

| ID | Karar | Kaynak | Durum | Kabul tarihi | Son inceleme |
| --- | --- | --- | --- | --- | --- |
| DEC-031 | Faz 4'teki KVKK çalışmaları tamamlanana kadar staging ve production'da sadece sentetik veri | nfr N1 | Accepted | 2026-10-27 | — |
| DEC-032 | Token süreleri: access 15 dk, refresh 7 gün (rotation ile), mutlak oturum süresi 30 gün | nfr N2 | Accepted | 2026-10-27 | — |
| DEC-033 | Tek sunucuda %99,5 erişilebilirlik; faz 4'ten önce kesintisiz deploy yok | nfr N3 | Accepted | 2026-10-27 | — |
| DEC-034 | Faz 5'e kadar aylık en fazla 20 USD barındırma bütçesi | nfr N4 | Accepted | 2026-10-27 | — |

## Çalışma şekli

| ID | Karar | Kaynak | Durum | Kabul tarihi | Son inceleme |
| --- | --- | --- | --- | --- | --- |
| DEC-035 | Git geçmişinde, PR'larda ve dokümanlarda yazar, sahip ve onaylayan olarak sadece sahibin adı geçer; Claude Code attribution'ı kapalıdır ve bir `commit-msg` hook'u AI attribution satırlarını temizler | sahibin talebi | Accepted | 2026-10-27 | — |
| DEC-072 | GitHub reposu geliştirme boyunca **public** olur: GitHub Free'de branch koruması ve environment onay kuralları sadece public repolarda var; Actions dakikaları public repolarda ücretsiz; portföy olarak görünür. Repoda gizli bilgi ve gerçek kişisel veri bulunmaz (NFR-SEC-09, DEC-031). Ürün ticari olarak yayına alındığında repo private yapılır (ücretli plan gerekir) | P0-22 planlaması, sahibin kararı | Accepted | 2026-10-27 | — |
| DEC-073 | Repoya açık kaynak lisansı eklenmez; README'de "Tüm hakları saklıdır" notu bulunur. Public olması, kodun kullanım hakkı verildiği anlamına gelmez | Sahibin kararı | Accepted | 2026-10-27 | — |

## Bekleyenler

| ID | Konu | Karar verileceği yer |
| --- | --- | --- |
| — | Şu an bekleyen karar yok | — |
