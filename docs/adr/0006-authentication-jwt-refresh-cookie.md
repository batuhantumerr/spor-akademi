# ADR-0006: Kimlik doğrulamada bellekte access token ve HttpOnly cookie'de refresh token kullan

> Repodaki yeri: `docs/adr/0006-authentication-jwt-refresh-cookie.md`

| Alan | Değer |
| --- | --- |
| Durum | `Accepted` |
| Tarih | 2026-10-27 |
| Karar veren | Batuhan Tümer |
| İlgili kararlar | DEC-004, DEC-010, DEC-026, DEC-032, DEC-034 |
| İlgili gereksinimler | US-03, US-04, US-05, NFR-SEC-03, NFR-SEC-05, NFR-SEC-06, NFR-SEC-07, NFR-SEC-14 |

## Bağlam

Kullanıcıların (Platform Admin, Okul Admini, Antrenör) nasıl giriş yapacağını ve oturumlarının nasıl sürdürüleceğini belirlememiz gerekiyor. Token sürelerine (DEC-032) ve refresh token rotation'a (US-04) zaten karar verdik. Bu ADR, açık kalan üç soruyu cevaplar:

1. **Kimlik doğrulamayı kim yapar?** Kendi kodumuz mu, ASP.NET Core Identity mi, harici bir kimlik sağlayıcı mı?
2. **Token'lar nasıl üretilir ve imzalanır?**
3. **Token'lar tarayıcıda nerede saklanır?** (NFR-SEC-06'da "refresh token JavaScript tarafından okunamaz" demiştik; yöntemi burada seçiyoruz.)

Kararı etkileyen gerçekler:

- Platformun tek bir API'si ve şimdilik tek bir web istemcisi (Angular admin) var; faz 3'te ikinci bir istemci (React veli uygulaması) eklenecek.
- `User` aggregate'ı domain modelinin bir parçası: multi-tenant (`TenantId`), kendine özgü kilitleme ve "ilk girişte şifre değiştirme" kuralları var.
- Bütçe aylık 20 USD (DEC-034); ücretli bir kimlik servisi kullanılamaz.
- Tarayıcıdaki en ciddi iki tehdit:
  - **XSS (Cross-Site Scripting):** sayfaya zararlı JavaScript enjekte edilmesi. Bu kod, JavaScript'in erişebildiği her şeyi (ör. `localStorage`) okuyabilir.
  - **CSRF (Cross-Site Request Forgery):** başka bir sitenin, kullanıcının tarayıcısındaki cookie'leri kullanarak onun adına istek göndermesi.

## Karar kriterleri

1. **Güvenlik:** bir XSS açığı uzun ömürlü bir oturumun çalınmasına yol açmamalı; CSRF'e karşı korumalı olmalı.
2. **Domain modeline uyum:** kullanıcı modeli ve kuralları bizim kontrolümüzde olmalı.
3. **Basitlik ve maliyet:** ek bir sunucu veya ücretli servis gerektirmemeli.
4. **Öğrenme değeri:** token tabanlı kimlik doğrulamanın nasıl çalıştığını gerçekten anlamak.

## Değerlendirilen seçenekler

### Soru 1: Kimlik doğrulamayı kim yapar?

#### Seçenek 1A: Harici kimlik sağlayıcı (Keycloak, Auth0, Microsoft Entra ID)

- **Artıları:** Güvenlik sorumluluğu uzmanlaşmış bir sisteme devredilir. Sosyal giriş, MFA, şifre sıfırlama hazır gelir. Kurumsal projelerde yaygın.
- **Eksileri:** Keycloak kendi sunucusunu ve bakımını ister (tek sunucu ve bütçe ile zor); bulut servisleri belirli bir kullanıcı sayısından sonra ücretlidir. Multi-tenant kullanıcı modelimiz ve kurallarımız iki sistem arasında bölünür. OAuth/OpenID Connect akışları, MVP için ciddi bir öğrenme ve yapılandırma yükü getirir.

#### Seçenek 1B: ASP.NET Core Identity (tam haliyle)

`UserManager`, `SignInManager` ve Identity'nin kendi tablolarıyla.

- **Artıları:** Microsoft'un hazır ve test edilmiş çözümü; şifre hash'leme, kilitleme, token üretimi dahil.
- **Eksileri:** Kendi tablo yapısını ve `IdentityUser` sınıfını dayatır; bizim `User` aggregate'ımız ve DDD yaklaşımımızla çakışır. Multi-tenancy ve "e-posta platform genelinde benzersiz" kuralı için özelleştirme gerekir. İçinde ne olduğunu bilmeden kullanmak öğrenme amacına ters.

#### Seçenek 1C: Kendi `Identity` modülümüz, standart yapı taşlarıyla

Kullanıcı modeli, giriş, kilitleme ve token akışları bizim `Identity` modülümüzde yazılır. Kriptografik işler için ise kanıtlanmış standart bileşenler kullanılır: şifre hash'leme için ASP.NET Core'un `PasswordHasher` sınıfı, JWT üretimi ve doğrulaması için .NET'in JWT kütüphaneleri ve ASP.NET Core'un JWT Bearer kimlik doğrulama middleware'i.

- **Artıları:** Domain modeli ve kurallar tamamen bizim kontrolümüzde. Ek sunucu veya maliyet yok. Token tabanlı kimlik doğrulamanın her adımını öğretir.
- **Eksileri:** Güvenlik açısından kritik kodun sorumluluğu bizde. MFA, sosyal giriş gibi özellikler gerekirse kendimiz eklemeliyiz.

### Soru 2: Token'lar

**Access token:** kısa ömürlü (15 dk) bir JWT. API, her istekte imzasını doğrular; veritabanına gitmez.

İmzalama için iki seçenek:

- **HS256 (simetrik):** tek bir gizli anahtarla hem imzalanır hem doğrulanır. Basit. Ancak token'ı doğrulayan her servisin aynı gizli anahtarı bilmesi gerekir.
- **RS256 / ES256 (asimetrik):** özel anahtarla imzalanır, açık anahtarla doğrulanır. Başka servisler token'ı imzalama yetkisi olmadan doğrulayabilir. Anahtar yönetimi daha karmaşıktır.

**Refresh token:** uzun ömürlü (7 gün) ve her kullanımda yenilenen bir değer. JWT olması gerekmez; veritabanında karşılığı tutulur.

### Soru 3: Token'lar tarayıcıda nerede saklanır?

#### Seçenek 3A: İkisi de `localStorage`'da

- **Artıları:** En basit; birçok örnek projede böyle yapılır.
- **Eksileri:** Bir XSS açığı, iki token'ı da okuyup başka bir yere gönderebilir. Saldırgan, refresh token sayesinde oturumu 30 güne kadar sürdürebilir. **Reddedildi.**

#### Seçenek 3B: Access token bellekte, refresh token HttpOnly cookie'de

Access token sadece JavaScript belleğinde (bir değişkende) tutulur ve `Authorization` başlığıyla gönderilir. Refresh token, JavaScript'in okuyamadığı (`HttpOnly`), sadece HTTPS'te gönderilen (`Secure`), başka sitelerden gelen isteklerde gönderilmeyen (`SameSite=Strict`) ve sadece kimlik doğrulama endpoint'lerine gönderilen (`Path=/api/v1/auth`) bir cookie'de tutulur. Sayfa yenilendiğinde bellek silinir; uygulama açılırken refresh endpoint'i çağrılarak yeni bir access token alınır.

- **Artıları:** XSS açığı olsa bile refresh token okunamaz; saldırganın elde edebileceği en fazla 15 dakikalık bir access token'dır. Access token cookie'de olmadığı için normal API istekleri CSRF'e açık değildir. Ek sunucu bileşeni gerekmez.
- **Eksileri:** Refresh token cookie'de olduğu için refresh endpoint'i CSRF açısından ayrıca korunmalıdır. Sayfa her yenilendiğinde bir refresh isteği yapılır. API ve web uygulamasının aynı site (same-site) altında olması gerekir.

#### Seçenek 3C: İkisi de HttpOnly cookie'de

- **Artıları:** JavaScript hiçbir token'a erişemez.
- **Eksileri:** Her API isteği cookie ile kimlik doğruladığı için tüm durum değiştiren istekler CSRF'e açıktır ve her biri için anti-forgery token gerekir. Bearer token kullanan diğer istemcilerle (ör. ileride bir mobil uygulama) tutarsız bir model ortaya çıkar.

#### Seçenek 3D: Backend-for-Frontend (BFF)

Tarayıcı hiçbir token görmez; sadece bir oturum cookie'si taşır. Token'lar, web uygulamasının önünde duran ayrı bir sunucu katmanında (BFF) tutulur ve API'ye bu katman üzerinden gidilir. Tarayıcı tabanlı uygulamalar için güncel güvenlik önerilerinde en güçlü seçenek olarak geçer.

- **Artıları:** En yüksek güvenlik; token'lar tarayıcıya hiç ulaşmaz.
- **Eksileri:** Ek bir sunucu bileşeni ve sunucu tarafında oturum yönetimi gerektirir. Her istek bir ara katmandan geçer. Bu projenin MVP aşaması için gereğinden karmaşık.

### Karşılaştırma (tarayıcıda saklama)

| Kriter | localStorage | Bellek + HttpOnly cookie | İkisi de cookie | BFF |
| --- | --- | --- | --- | --- |
| XSS'e karşı | Kötü | İyi | Çok iyi | Çok iyi |
| CSRF'e karşı | Çok iyi | İyi (refresh endpoint'i korunmalı) | Orta (her istek korunmalı) | İyi |
| Basitlik | Çok iyi | İyi | Orta | Kötü |
| Maliyet | Çok iyi | Çok iyi | Çok iyi | Orta |

## Karar

**Soru 1 için Seçenek 1C (kendi `Identity` modülümüz, standart yapı taşlarıyla), Soru 2 için HS256 imzalı JWT access token ve opak refresh token, Soru 3 için Seçenek 3B'yi (access token bellekte, refresh token HttpOnly cookie'de) seçiyoruz.**

**Temel kural: kriptografiyi kendimiz yazmayız.** Şifre hash'leme, JWT imzalama ve doğrulama, rastgele sayı üretimi için .NET'in standart bileşenleri kullanılır. Bizim yazdığımız kod, bu bileşenleri doğru sırayla ve doğru kurallarla birleştirir.

**Şifreler:**

- ASP.NET Core'un `PasswordHasher<TUser>` sınıfıyla (PBKDF2, tuzlanmış) hash'lenir (NFR-SEC-03). ASP.NET Core Identity'nin geri kalanı (`UserManager`, Identity tabloları) kullanılmaz.

**Access token (JWT):**

- Süre: 15 dakika (DEC-032). HS256 ile imzalanır; anahtar en az 256 bit, ortam değişkeninden gelir (NFR-SEC-09).
- Claim'ler: `sub` (kullanıcı ID'si), `role`, `tenant_id` (Platform Admin'de yok), `pwd_change_required` (ilk giriş için), `jti` (token ID'si), standart `iss`, `aud`, `exp`, `iat`.
- Kişisel veri (ad, e-posta) token'a konmaz; JWT imzalıdır ama şifreli değildir, içeriğini herkes okuyabilir.
- `pwd_change_required` claim'i taşıyan bir token ile, bir yetkilendirme politikası sayesinde sadece şifre değiştirme endpoint'ine erişilebilir (US-05).

**Refresh token:**

- Kriptografik olarak güvenli 256 bitlik rastgele bir değerdir; JWT değildir.
- Veritabanında sadece **SHA-256 hash'i** saklanır. (Şifreler için yavaş bir hash gerekir, çünkü insanların seçtiği şifreler tahmin edilebilir; 256 bitlik rastgele bir değer ise tahmin edilemez, bu yüzden hızlı bir hash yeterlidir.)
- Her kullanımda yenilenir (rotation); her token bir aileye (`FamilyId`) aittir. Daha önce kullanılmış bir token tekrar gelirse, ailenin tamamı iptal edilir (reuse detection, US-04).
- Ailenin ilk token'ından itibaren 30 gün geçtiyse yenileme yapılmaz; kullanıcı yeniden giriş yapar (mutlak oturum süresi, DEC-032).
- Cookie: `HttpOnly; Secure; SameSite=Strict; Path=/api/v1/auth`.

**Barındırma ve CSRF koruması:**

- Angular uygulaması ve API **aynı origin**'den sunulur: reverse proxy, `/api` ile başlayan istekleri API'ye, diğerlerini Angular'a yönlendirir. Böylece CORS yapılandırmasına gerek kalmaz ve `SameSite=Strict` cookie'si doğal olarak çalışır.
- Refresh ve çıkış endpoint'leri sadece `POST` kabul eder ve özel bir başlık (`X-Requested-With: XMLHttpRequest`) ister. Başka bir siteden bu başlıkla istek göndermek tarayıcının CORS kontrolüne takılır; bu, `SameSite`'a ek bir CSRF katmanıdır.

**Bilinen ve kabul edilen sınır:**

- Access token'lar veritabanına sorulmadan doğrulanır. Bu nedenle pasif hale getirilen bir kullanıcının elindeki access token, en fazla 15 dakika daha geçerli kalabilir. Refresh token'ları anında iptal edildiği için (US-08) bu süreden sonra erişimi tamamen kesilir. 15 dakikalık bu pencere, her istekte veritabanı sorgusu yapmanın maliyetine karşı bilinçli olarak kabul edilmiştir.

## Sonuçlar

- **Olumlu:**
  - XSS açığında bile uzun ömürlü oturum çalınamaz; saldırganın elde edebileceği en fazla 15 dakikalık bir token olur.
  - Normal API istekleri cookie kullanmadığı için CSRF'e açık değildir; sadece iki endpoint'in ayrıca korunması gerekir.
  - Çalınan bir refresh token'ın kullanılması, reuse detection ile fark edilir ve tüm oturum ailesi kapanır.
  - Kullanıcı modeli ve kuralları tamamen domain modelimizde kalır; ek sunucu veya maliyet yoktur.
  - Mülakatların klasik konuları (JWT yapısı, imzalama, XSS ve CSRF, token rotation, şifre hash'leme) projede somut olarak uygulanmış olur.
- **Olumsuz:**
  - Güvenlik açısından kritik akışların sorumluluğu bizdedir; testleri özenle yazılmalıdır.
  - Pasif hale getirilen kullanıcı 15 dakikaya kadar erişimini sürdürebilir.
  - HS256'da token'ı doğrulayan her servis gizli anahtarı bilmek zorundadır; faz 5'te bir servis ayrıldığında bu yeniden değerlendirilmelidir.
  - Faz 3'teki React uygulaması da aynı site altında (ör. bir alt alan adında) yayınlanmalıdır; aksi halde cookie gönderilmez.
  - MFA ve sosyal giriş yoktur.
- **Riskler ve önlemler:**
  - *Risk:* JWT doğrulamasında yanlış yapılandırma (ör. imzasız token'ın kabul edilmesi, `aud`/`iss` kontrolünün unutulması). *Önlem:* imzasız, süresi dolmuş, yanlış imzalı ve yanlış hedefli token'lar için ayrı entegrasyon testleri.
  - *Risk:* Gizli anahtarın sızması. *Önlem:* anahtar sadece ortam değişkeninde; repoda ve loglarda bulunmaz; sızıntıda anahtar değiştirilir ve tüm refresh token'lar iptal edilir (runbook'ta belgelenir).
  - *Risk:* Uygulamada bir XSS açığı. *Önlem:* Angular'ın varsayılan çıktı kodlaması; `innerHTML` ve `bypassSecurityTrust...` kullanımına inceleme kuralı; faz 4'te Content Security Policy (NFR-SEC-13).

## Uygulama ve doğrulama

- P1-09 – P1-11: giriş, refresh, çıkış ve şifre değiştirme akışları ve testleri.
- P1-13: Angular'da bellekteki access token, 401 yanıtında otomatik refresh yapan interceptor ve aynı anda gelen birden fazla 401'de tek bir refresh isteği gönderilmesi.
- Entegrasyon testleri: süresi dolmuş, imzasız, yanlış imzalı, yanlış `aud`/`iss` taşıyan token'ların reddedilmesi; refresh token rotation ve reuse detection; mutlak 30 günlük sınır; `pwd_change_required` kısıtı.
- Mimari testi: `AllowAnonymous` olarak işaretlenmiş endpoint'ler bilinen bir listeyle karşılaştırılır (NFR-SEC-07).

## Yeniden değerlendirme

- Faz 3'te veli uygulaması farklı bir alan adında yayınlanmak zorunda kalırsa.
- Faz 4'te MFA veya sosyal giriş ihtiyacı doğarsa (harici kimlik sağlayıcı yeniden değerlendirilir).
- Faz 5'te token'ı doğrulayacak ikinci bir servis eklendiğinde (asimetrik imzalamaya geçiş).
- Güvenlik denetiminde BFF modeline geçiş önerilirse.

## Kaynaklar

- `docs/product/user-stories.md` (US-03 – US-05), `docs/product/nfr.md` (NFR-SEC)
- OWASP Cheat Sheet Series: *JSON Web Token*, *Cross-Site Request Forgery Prevention*, *Password Storage*
- IETF, *OAuth 2.0 for Browser-Based Applications* (BFF ve token saklama önerileri)
