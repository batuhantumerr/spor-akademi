# Fonksiyonel Olmayan Gereksinimler (NFR)

> Durum: Onaylandı v1.0 · Repodaki yeri: `docs/product/nfr.md` · Roadmap maddesi: P0-07

## Bu doküman nasıl okunur

- Her gereksinimin değişmeyen bir ID'si (`NFR-SEC-03`), **ölçülebilir bir hedefi** ve bir **doğrulama yöntemi** vardır. Doğrulanamayan bir gereksinim, gereksinim değil temennidir.
- **Geçerli olduğu faz**, gereksinimin hangi fazdan itibaren bağlayıcı olduğunu gösterir. Daha önceki fazlar, o hedefe ulaşmayı zorlaştıracak bir şey yapmamalıdır.
- Kategoriler ISO/IEC 25010 kalite modelini izler.

## 1. Bağlam ve varsayımlar

Aşağıdaki hedefler, faz 3 sonundaki şu yüke göre belirlenmiştir:

| Varsayım | Değer |
| --- | --- |
| Okul (tenant) sayısı | 50 |
| Toplam aktif öğrenci | 5.000 (en büyük okul: 500) |
| Personel kullanıcı (admin ve antrenör) | 400 |
| Veli kullanıcı (faz 3 ve sonrası) | 6.000 |
| Yoğun saatler | Hafta içi 16:00–20:00 ve hafta sonu 09:00–14:00; antrenmanların çoğu bu saatlerde yapılır ve antrenörler yoklama alır |
| Yoğun saatte eşzamanlı kullanıcı | 300 |

**Faz 4'e kadar sadece sentetik veri.** Faz 4'teki KVKK gereksinimleri tamamlanana kadar staging ve production ortamlarında gerçek çocuk veya aile verisi değil, sadece üretilmiş demo verisi bulunur. Bu, öğrenme projesi aşamasında projeyi hukuki açıdan güvende tutar.

## 2. Güvenlik (SEC)

| ID | Gereksinim | Hedef | Doğrulama | Geçerli olduğu faz |
| --- | --- | --- | --- | --- |
| NFR-SEC-01 | Güvenlik standardı | MVP'de OWASP ASVS Seviye 1, faz 4'e kadar Seviye 2 | Faz geçişinde kontrol listesi | Faz 1 |
| NFR-SEC-02 | İletişim güvenliği | Sadece HTTPS; HTTP istekleri HTTPS'e yönlendirilir; HSTS açık; TLS 1.2 ve üzeri | Deploy sonrası otomatik kontrol | Faz 0 |
| NFR-SEC-03 | Şifre saklama | ASP.NET Core `PasswordHasher` (PBKDF2) ile tuzlanmış (salted), yavaş hash; düz metin şifre asla saklanmaz ve loglanmaz | Kod incelemesi, birim testi | Faz 1 |
| NFR-SEC-04 | Şifre politikası | En az 10 karakter; zorunlu karakter türü kuralı yok; yaygın şifreler listesine karşı kontrol edilir | Birim testi | Faz 1 |
| NFR-SEC-05 | Token süreleri | Access token 15 dakika; refresh token 7 gün ve her kullanımda yenilenir (rotation); oturumun mutlak üst sınırı 30 gün | Entegrasyon testi | Faz 1 |
| NFR-SEC-06 | Tarayıcıda token saklama | Refresh token hiçbir zaman JavaScript tarafından okunamaz. Access token sadece bellekte tutulur. Kesin yöntem ADR-0006'da belirlenecek | Kod incelemesi, e2e test | Faz 1 |
| NFR-SEC-07 | Varsayılan olarak yetki | Açıkça "public" işaretlenmedikçe her endpoint kimlik doğrulama ister (deny by default) | Mimari testi | Faz 1 |
| NFR-SEC-08 | Tenant izolasyonu | Hiçbir istek başka bir okulun verisini okuyamaz veya değiştiremez | Tenant izolasyon test paketi (P1-35) | Faz 1 |
| NFR-SEC-09 | Gizli bilgiler (secrets) | Repoda, Docker imajlarında veya loglarda hiçbir gizli bilgi bulunmaz; gizli bilgiler ortam değişkenlerinden / GitHub secrets'tan gelir | CI'da secret taraması | Faz 0 |
| NFR-SEC-10 | Veritabanı yetkileri | Uygulama, şemayı değiştiremeyen bir kullanıcıyla bağlanır; migration'lar ayrı bir kullanıcıyla çalışır | Deploy betiği incelemesi | Faz 0 |
| NFR-SEC-11 | Bağımlılıklar | Sürüm çıkarken bağımlılıklarda bilinen yüksek veya kritik seviye güvenlik açığı yoktur | CI'da otomatik tarama (NuGet ve npm audit, Dependabot) | Faz 0 |
| NFR-SEC-12 | Girdi sınırları | İstek gövdesi en fazla 1 MB (dosyalar faz 2'de ayrıca ele alınır); tüm girdiler API sınırında doğrulanır | Entegrasyon testi | Faz 1 |
| NFR-SEC-13 | Güvenlik başlıkları | CSP, `X-Content-Type-Options`, `Referrer-Policy`, frame koruması | Otomatik başlık kontrolü | Faz 4 |
| NFR-SEC-14 | Rate limiting | Giriş için IP ve hesap bazında sınır; API için kullanıcı bazında sınır | Entegrasyon testi | Giriş faz 1'de, diğerleri faz 4'te |

## 3. Gizlilik ve KVKK (PRV)

| ID | Gereksinim | Hedef | Doğrulama | Geçerli olduğu faz |
| --- | --- | --- | --- | --- |
| NFR-PRV-01 | Veri minimizasyonu | Sadece bir kullanıcı hikayesinin ihtiyaç duyduğu veri toplanır; her kişisel veri alanı `docs/domain/personal-data-inventory.md` dosyasında listelenir | Her faz geçişinde inceleme | Faz 1 |
| NFR-PRV-02 | Bilmesi gereken kadar erişim | Antrenörler sadece DEC-007'nin izin verdiğini görür; yanıt yapısı üzerindeki contract testlerle zorunlu kılınır | Contract testleri | Faz 1 |
| NFR-PRV-03 | Loglarda kişisel veri yok | Loglar sadece ID içerir; isim, telefon, e-posta, doğum tarihi veya token asla loglanmaz | Log incelemesi, otomatik log temizleme testi | Faz 1 |
| NFR-PRV-04 | Saklama süresi | DEC-008'e göre anonimleştirme | İş (job) logları | Faz 2 |
| NFR-PRV-05 | İlgili kişi başvuruları | Talep üzerine bir kişinin verisini dışa aktarma ve silme | Entegrasyon testi | Faz 4 |
| NFR-PRV-06 | Aydınlatma metni | Gerçek veri kullanılmadan önce okullara ve velilere bir aydınlatma metni gösterilir | Doküman mevcut ve arayüzde bağlantısı var | Faz 4 |
| NFR-PRV-07 | Verinin konumu | Gerçek veriler için barındırma konumu KVKK dikkate alınarak belirlenir (kişisel verilerin yurt dışına aktarımı ek koşullara tabidir); bir ADR ile kayda geçer | ADR | Faz 4 |

## 4. Performans (PRF)

Bölüm 1'deki yoğun saat yükünde, production sunucusunda ölçülür.

| ID | Gereksinim | Hedef | Doğrulama | Geçerli olduğu faz |
| --- | --- | --- | --- | --- |
| NFR-PRF-01 | API gecikmesi, okuma | p95 < 300 ms | k6 yük testi, sonrasında production metrikleri | Faz 4'te ölçülür, faz 1'den itibaren buna göre tasarlanır |
| NFR-PRF-02 | API gecikmesi, yazma | p95 < 500 ms | k6 yük testi | Faz 4 |
| NFR-PRF-03 | Yoklama akışı | Bir antrenör, telefonundan bugünkü antrenmanı açıp 20 öğrencinin yoklamasını 30 saniyenin altında kaydedebilir | Süre ölçülerek manuel test, e2e test | Faz 1 |
| NFR-PRF-04 | Ön yüz yükleme | Antrenör ekranlarında, orta seviye bir telefonda 4G üzerinden Largest Contentful Paint < 2,5 sn | CI'da Lighthouse | Faz 1 (antrenör ekranları) |
| NFR-PRF-05 | Liste endpoint'leri | Her zaman sayfalı; sayfa başına en fazla 100 kayıt; sınırsız sorgu yok | Mimari / kod incelemesi | Faz 1 |
| NFR-PRF-06 | Veritabanı sorguları | Liste endpoint'lerinde N+1 sorgu yok; tüm foreign key'ler ve filtre sütunları index'li | Entegrasyon testlerinde EF Core sorgu loglaması | Faz 1 |

## 5. Güvenilirlik ve kurtarma (REL)

| ID | Gereksinim | Hedef | Doğrulama | Geçerli olduğu faz |
| --- | --- | --- | --- | --- |
| NFR-REL-01 | Erişilebilirlik (availability) | Tek sunucuda aylık %99,5 (yaklaşık 3,5 saat kesintiye izin verir) | Uptime monitörü | Faz 1 |
| NFR-REL-02 | Deploy zamanı ve kesinti | Production deploy'ları antrenman saatleri dışında, hafta içi 09:00–14:00 (Europe/Istanbul) arasında yapılır; deploy başına kesinti < 1 dakika | Deploy logları | Faz 1 |
| NFR-REL-03 | Yedekleme | Günlük otomatik veritabanı yedeği; şifrelenmiş, sunucu dışında saklanan, 30 gün tutulan | Yedekleme işi logları | Faz 0 |
| NFR-REL-04 | Kurtarma noktası hedefi (RPO) | MVP'de en fazla 24 saatlik veri kaybı; faz 4'e kadar 15 dakika (point-in-time recovery) | Geri yükleme tatbikatı | Faz 1 / Faz 4 |
| NFR-REL-05 | Kurtarma süresi hedefi (RTO) | Servis 4 saat içinde geri gelir | Her faz geçişinde geri yükleme tatbikatı | Faz 1 |
| NFR-REL-06 | Migration öncesi yedek | Her production migration'ından önce yedek alınır | CD pipeline adımı | Faz 0 |
| NFR-REL-07 | Sağlık kontrolleri | `/health/live` (süreç ayakta) ve `/health/ready` (veritabanına erişilebiliyor) | Deploy sonrası otomatik kontrol | Faz 0 |
| NFR-REL-08 | Veritabanında veri bütünlüğü | Önemli kurallar sadece kodda değil, veritabanı kısıtlarıyla da (unique index, foreign key, check constraint) korunur | Entegrasyon testleri | Faz 1 |

## 6. Ölçeklenebilirlik (SCL)

| ID | Gereksinim | Hedef | Doğrulama | Geçerli olduğu faz |
| --- | --- | --- | --- | --- |
| NFR-SCL-01 | Durumsuz (stateless) API | Bellekte oturum durumu tutulmaz; her API örneği her isteğe hizmet verebilir, böylece yeni örnekler eklenebilir | Faz 4'te proxy arkasında iki örnekle test | Faz 1 |
| NFR-SCL-02 | Tenant büyümesi | Yeni bir okul eklemek deploy veya yapılandırma değişikliği gerektirmez | US-01 testi | Faz 1 |

## 7. Bakım yapılabilirlik (MNT)

| ID | Gereksinim | Hedef | Doğrulama | Geçerli olduğu faz |
| --- | --- | --- | --- | --- |
| NFR-MNT-01 | Build kalitesi | Derleyici ve analyzer uyarıları hata sayılır; kod formatı CI'da zorunlu tutulur | CI | Faz 0 |
| NFR-MNT-02 | Mimari kurallar | Katman bağımlılıkları mimari testlerle zorunlu tutulur | CI | Faz 1 |
| NFR-MNT-03 | Test kapsamı | `Domain` ve `Application` için en az %80 satır kapsamı; kapsam bir hedef değil, bir sinyaldir | CI'da kapsam raporu | Faz 1 |
| NFR-MNT-04 | CI hızı | Tüm CI pipeline'ı < 10 dakika | CI süreleri | Faz 1 |
| NFR-MNT-05 | PR boyutu | PR başına < 400 değişen satır hedeflenir (üretilmiş kod hariç) | PR incelemesi | Faz 1 |
| NFR-MNT-06 | Projeye başlama süresi | README takip edilerek, sıfırdan klonlanan proje tek komutla 15 dakikanın altında yerelde çalışır | Her faz geçişinde test edilir | Faz 0 |

## 8. Gözlemlenebilirlik (OBS)

| ID | Gereksinim | Hedef | Doğrulama | Geçerli olduğu faz |
| --- | --- | --- | --- | --- |
| NFR-OBS-01 | Yapılandırılmış loglama | Zaman damgası, seviye, correlation ID, tenant ID ve kullanıcı ID içeren JSON loglar | Log incelemesi | Faz 1 |
| NFR-OBS-02 | İstek takibi (correlation) | Her isteğin bir correlation ID'si olur; yanıt başlığında döner ve hata yanıtlarına eklenir | Entegrasyon testi | Faz 1 |
| NFR-OBS-03 | Uptime izleme | `/health/ready` her 5 dakikada bir dışarıdan kontrol edilir, sorun olursa e-posta ile uyarı gelir | Monitör kurulu | Faz 1 |
| NFR-OBS-04 | Trace, metrik, hata takibi | OpenTelemetry, dashboard'lar, alarmlar, Sentry | Dashboard'lar mevcut | Faz 4 |

## 9. Kullanılabilirlik ve erişilebilirlik (USA)

| ID | Gereksinim | Hedef | Doğrulama | Geçerli olduğu faz |
| --- | --- | --- | --- | --- |
| NFR-USA-01 | Responsive tasarım | Tüm ekranlar 360 px genişlikten itibaren kullanılabilir; antrenörler sahada telefon kullandığı için antrenör ekranları mobil öncelikli tasarlanır | Telefonda manuel kontrol, mobil ekran boyutunda e2e testler | Faz 1 |
| NFR-USA-02 | Temel erişilebilirlik | Semantik HTML, klavye ile gezinme, görünür odak (focus), tüm girdilerde etiket, WCAG AA kontrast | E2e testlerde otomatik axe kontrolü | Faz 1 |
| NFR-USA-03 | Tam erişilebilirlik denetimi | WCAG 2.2 AA | Denetim raporu | Faz 3 |
| NFR-USA-04 | Hata mesajları | Kullanıcıya gösterilen mesajlar Türkçe ve yol gösterici; asla stack trace veya iç hata kodu göstermez | İnceleme | Faz 1 |

## 10. Uyumluluk ve yerelleştirme (CMP)

| ID | Gereksinim | Hedef | Doğrulama | Geçerli olduğu faz |
| --- | --- | --- | --- | --- |
| NFR-CMP-01 | Tarayıcılar | Chrome, Edge, Firefox ve Safari'nin (masaüstü ve mobil) son iki ana sürümü | Playwright testleri Chromium, Firefox ve WebKit üzerinde çalışır | Faz 1 |
| NFR-CMP-02 | Arayüz dili | MVP'de Türkçe; tüm arayüz metinleri koddan ayrı tutulur, böylece faz 3'te kod değişmeden İngilizce eklenebilir | Lint kuralı / inceleme | Faz 1 |
| NFR-CMP-03 | Formatlar | Tarih `gg.AA.yyyy`, 24 saat formatı, para `₺1.250,00`; saat dilimi okul bazında (varsayılan `Europe/Istanbul`) | Birim testleri | Faz 1 |
| NFR-CMP-04 | API kuralları | `/api/v1` altında sürümlü; hatalar ProblemDetails (RFC 9457) formatında; JSON camelCase; tarihler ISO 8601 | Contract testleri | Faz 1 |

## 11. Maliyet (CST)

| ID | Gereksinim | Hedef | Doğrulama | Geçerli olduğu faz |
| --- | --- | --- | --- | --- |
| NFR-CST-01 | Barındırma bütçesi | Faz 5'e kadar staging ve production birlikte aylık en fazla 20 USD | Aylık fatura | Faz 0 |
| NFR-CST-02 | Önce ücretsiz katmanlar | GitHub Actions, uptime izleme ve hata takibi, yettiği sürece ücretsiz katmanlarda kullanılır | Faz geçişlerinde inceleme | Faz 0 |

## 12. Onaylanan kararlar

| # | Karar |
| --- | --- |
| N1 | Faz 4'teki KVKK çalışmaları tamamlanana kadar staging ve production'da sadece sentetik veri |
| N2 | Token süreleri: access 15 dk, refresh 7 gün (rotation ile), mutlak oturum süresi 30 gün |
| N3 | Tek sunucuda %99,5 erişilebilirlik hedefi; faz 4'ten önce kesintisiz (zero-downtime) deploy yok |
| N4 | Faz 5'e kadar aylık en fazla 20 USD barındırma bütçesi |
