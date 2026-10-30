# ADR-0007: Beklenen hatalar için Result pattern, beklenmeyenler için exception kullan; API'de ProblemDetails dön

> Repodaki yeri: `docs/adr/0007-error-handling-result-problemdetails.md`

| Alan | Değer |
| --- | --- |
| Durum | `Accepted` |
| Tarih | 2026-10-27 |
| Karar veren | Batuhan Tümer |
| İlgili kararlar | DEC-030, DEC-042, DEC-046 |
| İlgili gereksinimler | NFR-CMP-02, NFR-CMP-04, NFR-OBS-02, NFR-PRV-03, NFR-USA-04 |

## Bağlam

Uygulamada iki tür hata vardır ve bunlar birbirinden çok farklıdır:

- **Beklenen hatalar:** iş kuralları gereği olabilecek, normal akışın parçası olan durumlar. "Grup dolu", "öğrencinin doğum yılı grubun aralığında değil", "e-posta zaten kullanılıyor", "kayıt bulunamadı". Bunlar bir hata (bug) değildir; kullanıcıya anlaşılır bir mesajla gösterilmeleri gerekir.
- **Beklenmeyen hatalar:** olmaması gereken durumlar. Veritabanına bağlanılamaması, kodda bir null referansı, yanlış bir yapılandırma. Bunlar bir hatadır; loglanmaları ve incelenmeleri gerekir, kullanıcıya ise ayrıntı gösterilmez.

Bu ADR şu soruları cevaplar:

1. Domain ve application katmanları beklenen hataları nasıl bildirir?
2. Hatalar HTTP yanıtına nasıl dönüşür?
3. Kullanıcıya gösterilen Türkçe mesajlar nerede üretilir?

Kararı etkileyen gerçekler:

- Kullanıcıya gösterilen mesajlar Türkçe ve yol gösterici olmalı; asla stack trace göstermemeli (NFR-USA-04).
- Arayüz metinleri koddan ayrı tutulmalı ki faz 3'te İngilizce eklenebilsin (NFR-CMP-02).
- API hataları ProblemDetails (RFC 9457) formatında olmalı (NFR-CMP-04) ve correlation ID içermeli (NFR-OBS-02).
- ADR-0004'te handler'ların `Result<T>` döndürdüğünü kabul ettik.

## Karar kriterleri

1. **Açıklık:** bir metodun hangi şekillerde başarısız olabileceği, imzasından anlaşılabilmeli.
2. **Tutarlılık:** her hata, API'de aynı biçimde dönmeli; istemci tek bir yöntemle işleyebilmeli.
3. **Güvenlik:** iç ayrıntılar (stack trace, SQL, sınıf adları) kullanıcıya sızmamalı.
4. **Çok dilliliğe hazırlık:** kullanıcı mesajları dil değişikliğinde kod değişikliği gerektirmemeli.
5. **Performans:** normal akışın parçası olan durumlar için pahalı mekanizmalar kullanılmamalı.

## Değerlendirilen seçenekler

### Seçenek 1: Her şey için exception

İş kuralı ihlalleri `DomainException`, `NotFoundException` gibi özel exception'lar fırlatır; global bir handler bunları HTTP yanıtına çevirir.

- **Artıları:** Yaygın ve tanıdık. Kod kısa görünür; hata yayılması otomatiktir.
- **Eksileri:** Bir metodun hangi hataları fırlatabileceği imzasından görülmez; çağıran kod habersiz kalabilir. Exception'lar "görünmez goto" gibi çalışır; akışı takip etmek zorlaşır. Normal akışın parçası olan durumlar (ör. grup dolu) için exception kullanmak, beklenen ile beklenmeyen hatayı ayırt etmeyi zorlaştırır. Exception fırlatmak görece pahalıdır.

### Seçenek 2: Hazır bir Result kütüphanesi (ErrorOr, FluentResults, OneOf)

- **Artıları:** Test edilmiş, zengin özellikli.
- **Eksileri:** Ek bir bağımlılık. İhtiyacımız olan özellik seti çok küçük; kütüphanenin API'sine bağımlı hale geliriz.

### Seçenek 3: Projeye ait `Result` ve `Error` tipleri; beklenmeyen hatalar için exception

Beklenen hatalar `Result<T>` olarak döner; bir metodun imzası başarısız olabileceğini açıkça gösterir. Beklenmeyen hatalar exception olarak kalır ve tek bir yerde yakalanır.

- **Artıları:** Başarısızlık imzada görünür. Beklenen ve beklenmeyen hatalar net biçimde ayrılır. Bağımlılık yok; yaklaşık 50 satırlık bir kod. Kalıbı öğretir.
- **Eksileri:** Her çağrıda sonucun kontrol edilmesi gerekir; kod biraz daha uzundur. C#'ta bunu zorunlu kılan bir dil özelliği yoktur; kontrol disipline ve incelemeye bağlıdır.

### Kullanıcı mesajları nerede üretilir?

- **API Türkçe mesaj döner:** basit, ama mesajlar API'ye gömülür; faz 3'te çoklu dil için API'nin değişmesi gerekir.
- **API sabit bir hata kodu döner, metni istemci üretir:** istemci, `Enrollment.GroupFull` gibi kodu kendi dil dosyasındaki Türkçe metne çevirir. Dil değişikliği sadece istemcinin dil dosyasını etkiler.

## Karar

**Seçenek 3'ü seçiyoruz: projeye ait `Result<T>` ve `Error` tipleri beklenen hatalar için, exception'lar sadece beklenmeyen hatalar için.** Kullanıcı mesajlarını **istemci, API'nin döndüğü sabit hata kodlarından** üretir.

**`Error` tipi** (`Domain/SharedKernel`):

| Alan | Açıklama |
| --- | --- |
| `Code` | Sabit, makine tarafından okunur kod: `<Modül>.<Hata>`, ör. `Enrollment.GroupFull`, `Student.GuardianRequired`. Bir kez yayınlandıktan sonra değişmez |
| `Description` | Geliştirici için İngilizce açıklama; loglarda ve ProblemDetails `detail` alanında kullanılır. Kişisel veri içermez |
| `Type` | Hatanın türü; HTTP durum kodunu belirler |

**Hata türleri ve HTTP karşılıkları:**

| `Type` | Ne zaman | HTTP |
| --- | --- | --- |
| `Validation` | İstek biçimsel olarak geçersiz (zorunlu alan eksik, format yanlış) | `400 Bad Request` |
| `Unauthorized` | Kimlik doğrulanmamış | `401 Unauthorized` |
| `Forbidden` | Kimlik doğrulanmış ama rol bu işlem için yetkili değil | `403 Forbidden` |
| `NotFound` | Kayıt yok ya da başka bir okula ait (DEC-042) | `404 Not Found` |
| `Conflict` | Mevcut durumla çakışma: mükerrer kayıt, eşzamanlılık çakışması, kilit zaman aşımı | `409 Conflict` |
| `BusinessRule` | İstek geçerli ama bir iş kuralını ihlal ediyor: grup dolu, doğum yılı uygun değil, gelecekteki antrenmana yoklama | `422 Unprocessable Content` |

**Kurallar:**

- Aggregate metotları ve value object fabrika metotları, iş kuralı ihlallerinde `Result` döner; exception fırlatmaz. (ör. `Email.Create(string)` → `Result<Email>`, `group.ChangeCapacity(int)` → `Result`.)
- Programcı hataları (null parametre, imkansız bir durum) exception fırlatmaya devam eder; bunlar bug'dır.
- Hatalar her modülde statik olarak tanımlanır: `EnrollmentErrors.GroupFull`. Aynı hata iki yerde ayrı ayrı yazılmaz.
- Handler'lar her zaman `Result<T>` döner. Endpoint'ler, sonucu tek bir ortak yardımcı metotla HTTP yanıtına çevirir.

**API yanıt biçimi (ProblemDetails, RFC 9457):**

```json
{
  "type": "https://sporakademi.example/errors/enrollment.group-full",
  "title": "Business rule violation",
  "status": 422,
  "detail": "The group has reached its capacity.",
  "code": "Enrollment.GroupFull",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

Doğrulama hatalarında, alan bazında hata kodları da döner:

```json
{
  "status": 400,
  "title": "Validation failed",
  "code": "Validation.Failed",
  "errors": {
    "firstName": [ { "code": "Student.FirstNameRequired" } ],
    "emergencyContact.phone": [ { "code": "PhoneNumber.Invalid" } ]
  },
  "traceId": "…"
}
```

**Beklenmeyen hatalar:**

- ASP.NET Core'un `IExceptionHandler` arayüzüyle tek bir global handler yakalar.
- Exception, correlation ID ile birlikte tüm ayrıntılarıyla loglanır (kişisel veri olmadan, NFR-PRV-03).
- İstemciye sadece `500`, genel bir mesaj ve `traceId` döner; stack trace, exception adı veya SQL asla dönmez.
- PostgreSQL'in benzersizlik ihlali (ör. ADR-0005'teki kısmi index) veya kilit zaman aşımı gibi bilinen veritabanı hataları, `409 Conflict` olarak ayrıca eşlenir.

**Kullanıcı mesajları:**

- Angular uygulaması, hata kodlarını kendi dil dosyasında Türkçe metinlere eşler (ör. `Enrollment.GroupFull` → "Grup kontenjanı dolu. Başka bir grup seçin veya kontenjanı artırın.").
- Eşlemesi olmayan bir kod gelirse genel bir mesaj ve `traceId` gösterilir; bu durum istemci tarafında loglanır.
- Tüm hata kodları `docs/api/error-codes.md` dosyasında listelenir.

## Sonuçlar

- **Olumlu:**
  - Bir handler'ın veya aggregate metodunun başarısız olabileceği imzasından anlaşılır.
  - İstemci her hatayı tek bir biçimde işler; hata kodu sayesinde mesajları dil dosyasından üretir ve faz 3'teki çoklu dil desteği API'yi değiştirmez.
  - İç ayrıntılar kullanıcıya hiçbir zaman sızmaz; her hata, `traceId` ile loglardaki kaydına bağlanabilir.
  - Beklenen durumlar (grup dolu) exception maliyeti olmadan işlenir.
- **Olumsuz:**
  - Her `Result`'ın kontrol edilmesi gerekir; bu, exception tabanlı koda göre daha uzun kod demektir.
  - Hata kodları bir sözleşmedir: yayınlandıktan sonra değiştirilmeleri istemciyi bozar.
  - Kullanıcı mesajları ve hata kodları iki ayrı yerde (API ve istemcinin dil dosyası) tutulur ve senkronize olmalıdır.
- **Riskler ve önlemler:**
  - *Risk:* Bir `Result` kontrol edilmeden yok sayılır. *Önlem:* kod inceleme maddesi; `Result` döndüren metotların dönüş değerinin kullanılmamasını uyarı olarak işaretleyen bir analyzer kuralı değerlendirilir.
  - *Risk:* API'ye eklenen bir hata kodunun istemcide karşılığı yoktur. *Önlem:* `error-codes.md` ile istemcinin dil dosyasını karşılaştıran bir CI kontrolü (faz 1 sonunda).

## Uygulama ve doğrulama

- P1-01: `Result`, `Result<T>`, `Error`, `ErrorType`.
- P1-06: global exception handler, `Result` → ProblemDetails eşlemesi, doğrulama hatası formatı, bilinen veritabanı hatalarının eşlenmesi.
- Entegrasyon testleri: her hata türü için durum kodu ve yanıt biçimi; `500` yanıtının iç ayrıntı içermediği; her yanıtta `traceId` bulunduğu.
- Mimari testi: `Domain` katmanında `Exception`'dan türeyen iş kuralı exception'ları tanımlanmaz.

## Yeniden değerlendirme

- C#'a ayrımlı birleşim tipleri (discriminated unions) gibi, sonuç kontrolünü dil düzeyinde zorunlu kılan bir özellik eklenirse.
- Hata kodlarının istemcide eşlenmesi bakım yükü haline gelirse.

## Kaynaklar

- RFC 9457, *Problem Details for HTTP APIs*
- `docs/product/nfr.md` (NFR-CMP-02, NFR-CMP-04, NFR-USA-04)
