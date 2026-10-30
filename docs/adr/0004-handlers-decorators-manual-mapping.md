# ADR-0004: Command/query handler'ları ve decorator'lar kullan; MediatR ve AutoMapper kullanma

> Repodaki yeri: `docs/adr/0004-handlers-decorators-manual-mapping.md`

| Alan | Değer |
| --- | --- |
| Durum | `Accepted` |
| Tarih | 2026-10-27 |
| Karar veren | Batuhan Tümer |
| İlgili kararlar | DEC-025, DEC-027, DEC-037 |
| İlgili gereksinimler | NFR-MNT-01, NFR-MNT-02, NFR-PRF-06 |

## Bağlam

ADR-0001'e göre her kullanım senaryosu (ör. "öğrenci kaydet") `Application` katmanında bir command veya query olarak yazılacak. İki soruyu cevaplamamız gerekiyor:

1. **Bir HTTP isteği, ilgili kullanım senaryosunun koduna nasıl ulaşır?** Ve doğrulama, loglama, transaction gibi her senaryoda tekrar eden işler (cross-cutting concerns) nerede yapılır?
2. **Nesneler katmanlar arasında nasıl dönüştürülür?** (istek → command, aggregate → yanıt DTO'su)

Kararı etkileyen gerçekler:

- .NET dünyasında bu iki iş için standart haline gelmiş kütüphaneler MediatR ve AutoMapper. İkisi de 2025'te ticari lisansa geçti; küçük kuruluşlar ve bireyler için ücretsiz kalmaya devam ediyorlar. Yani bu projede yasal bir engel yok.
- Projenin birincil amacı öğrenme. Bir kalıbı kütüphanesiz uygulamak, o kalıbı gerçekten anlamayı sağlar.
- Yapay zeka agent'ları, eğitim verilerinde çok sık gördükleri için bu kütüphaneleri sorulmadan eklemeye eğilimli.
- DEC-027 bu kütüphaneleri kullanmama kararını zaten aldı; bu ADR, yerlerine ne kullanılacağını belirler.

## Karar kriterleri

1. **Anlaşılırlık:** bir isteğin hangi koddan geçtiği kolayca takip edilebilmeli; "sihir" olmamalı.
2. **Tekrarın önlenmesi:** doğrulama, loglama ve transaction her handler'da tekrar yazılmamalı.
3. **Bağımlılık ve lisans riski:** az sayıda dış paket, açık kaynak lisanslı.
4. **Öğrenme değeri.**
5. **Performans ve hata yakalama:** hatalar mümkünse çalışma zamanında değil, derleme zamanında yakalanmalı.

## Değerlendirilen seçenekler

### Senaryoya ulaşma ve cross-cutting işler

#### Seçenek 1: MediatR

Endpoint, `IMediator.Send(command)` çağırır; MediatR ilgili handler'ı bulur. Cross-cutting işler "pipeline behavior" olarak yazılır.

- **Artıları:** Sektörde en yaygın yaklaşım; çok sayıda örnek ve dokümantasyon.
- **Eksileri:** Ticari lisans; ileride kullanım koşulları değişebilir. Endpoint'in hangi handler'a gittiği koddan doğrudan görülmez; "Go to definition" handler'a götürmez. Eksik bir handler kaydı ancak çalışma zamanında fark edilir.

#### Seçenek 2: Servis sınıfları (mediator yok, decorator yok)

`StudentService.Register(...)` gibi klasik servis metotları.

- **Artıları:** En basit ve en bilinen yaklaşım.
- **Eksileri:** Servis sınıfları zamanla büyür ve çok sayıda bağımlılık toplar. Doğrulama ve transaction her metotta elle tekrarlanır veya unutulur.

#### Seçenek 3: Projeye ait bir dispatcher sınıfı

MediatR'ın küçük bir kopyası: `IDispatcher.Send(command)`, handler'ı DI container'dan bulur.

- **Artıları:** Lisans bağımlılığı yok; mediator kalıbını öğretir.
- **Eksileri:** MediatR'ın "hangi handler çalışıyor?" belirsizliği aynen devam eder. Reflection kodu yazmak ve test etmek gerekir. Getirdiği tek fayda (tek bir giriş noktası) bu projede gerçek bir ihtiyaç değil.

#### Seçenek 4: Handler arayüzlerinin doğrudan enjekte edilmesi ve decorator'lar

Her senaryo bir handler'dır ve ortak bir arayüzü uygular (`ICommandHandler<TCommand, TResponse>`). Endpoint, ihtiyaç duyduğu handler'ı doğrudan parametre olarak alır. Cross-cutting işler, handler'ı saran **decorator**'lar ile yapılır: DI container, endpoint'e asıl handler yerine onu saran decorator zincirini verir.

```
Endpoint → [Logging → Validation → Transaction → RegisterStudentHandler]
```

- **Artıları:** Endpoint'in hangi handler'ı kullandığı kodda açıkça yazılıdır. Eksik bir kayıt uygulama başlarken yakalanır (DI doğrulaması). Decorator kalıbı, Open/Closed prensibinin gerçek bir uygulamasıdır. Dispatcher veya reflection kodu yoktur.
- **Eksileri:** Decorator'ları kaydetmek için ya çok sayıda elle kayıt satırı ya da küçük bir yardımcı kütüphane (Scrutor, MIT lisanslı) gerekir. Decorator'ların sırası önemlidir ve belgelenmelidir.

### Mapping

#### Seçenek M1: AutoMapper

- **Artıları:** Az kod.
- **Eksileri:** Ticari lisans. Eşleme kuralları çalışma zamanında, gizli biçimde uygulanır; bir alanın adı değiştiğinde hata derlemede değil, çalışırken (veya hiç) ortaya çıkar. Debug etmesi zordur.

#### Seçenek M2: Mapperly (source generator)

Derleme sırasında eşleme kodunu üreten, MIT lisanslı bir kütüphane.

- **Artıları:** Derleme zamanında güvenli, hızlı, az kod.
- **Eksileri:** Ek bir bağımlılık ve ek bir kavram. Bu projedeki eşlemelerin çoğu basit olduğu için kazancı sınırlı.

#### Seçenek M3: Elle mapping

Eşleme, açıkça yazılmış kodla yapılır: yanıt DTO'ları için küçük `ToResponse()` metotları; sorgularda ise doğrudan EF Core projeksiyonu (`Select(s => new StudentListItem(...))`).

- **Artıları:** Bağımlılık yok. Bir alanın adı değiştiğinde derleyici hatayı hemen gösterir. Her eşleme okunabilir ve debug edilebilir. Agent ile yazılması zaten çok hızlı.
- **Eksileri:** Daha fazla kod yazılır.

## Karar

**Senaryoya ulaşmak için Seçenek 4'ü (handler'ların doğrudan enjekte edilmesi ve decorator'lar), mapping için Seçenek M3'ü (elle mapping) seçiyoruz.** Bu, DEC-027'deki "projeye ait dispatcher" ifadesini netleştirir: ayrı bir dispatcher sınıfı olmayacak.

**Arayüzler** (`Application/Abstractions/Messaging`):

```csharp
public interface ICommand<TResponse>;
public interface IQuery<TResponse>;

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
```

**Decorator'lar ve sırası** (dıştan içe):

| Sıra | Decorator | Uygulandığı yer | Görevi |
| --- | --- | --- | --- |
| 1 | `LoggingDecorator` | Command ve query | Senaryonun başlangıcını, süresini ve sonucunu loglar (kişisel veri olmadan, NFR-PRV-03) |
| 2 | `ValidationDecorator` | Command ve query | FluentValidation kurallarını çalıştırır; hata varsa handler'a hiç gidilmez |
| 3 | `TransactionDecorator` | Sadece command | Handler'ı tek bir transaction içinde çalıştırır, sonra commit eder ve domain event'lerini yayınlar |

**Command ve query ayrımı:**

- **Command'lar** durumu değiştirir. Aggregate'ı yükler, aggregate'ın metodunu çağırır (iş kuralları orada çalışır), kaydeder.
- **Query'ler** durumu asla değiştirmez. Aggregate yüklemez; veritabanından ihtiyaç duyulan alanları doğrudan yanıt DTO'suna projekte eder (`AsNoTracking` + `Select`). Bu, okuma tarafında N+1 ve gereksiz veri yükleme sorunlarını en baştan önler (NFR-PRF-06).

Bu yaklaşım CQRS'in en hafif biçimidir: aynı veritabanı, ama yazma ve okuma için farklı yollar.

**Klasör yapısı** (bir senaryo, bir klasör):

```
Application/Students/RegisterStudent/
    RegisterStudentCommand.cs
    RegisterStudentCommandValidator.cs
    RegisterStudentCommandHandler.cs
```

**Kayıt:** handler'lar ve decorator'lar Scrutor ile assembly taranarak kaydedilir. Uygulama başlarken DI doğrulaması (`ValidateOnBuild`) açıktır; eksik bir kayıt uygulamanın başlamasını engeller.

**Mapping:**

- HTTP isteği → command: endpoint içinde açıkça.
- Aggregate → yanıt: `Application` katmanında küçük statik `ToResponse()` metotları.
- Query sonuçları: doğrudan EF Core projeksiyonu.
- Domain entity'leri asla doğrudan API yanıtı olarak dönmez.

## Sonuçlar

- **Olumlu:**
  - Endpoint'ten handler'a giden yol kodda açıkça görülür; IDE ile tek tıkla takip edilir.
  - Doğrulama, loglama ve transaction hiçbir handler'da tekrar edilmez ve unutulamaz.
  - Eksik kayıtlar ve hatalı eşlemeler çalışma zamanında değil, başlangıçta veya derlemede yakalanır.
  - Ticari lisanslı bağımlılık yok.
  - Decorator kalıbı, dependency injection ve CQRS mülakatlarda somut örneklerle anlatılabilir.
- **Olumsuz:**
  - Elle mapping daha fazla satır kod demektir.
  - Decorator sırası bir kuraldır ve yanlış kaydedilirse (ör. transaction'ın doğrulamadan önce açılması) sessizce yanlış çalışabilir.
  - Sektördeki birçok projede MediatR kullanıldığı için, iş yerinde ona alışmak gerekebilir. Bu projede kalıbın kendisini öğrenmek, kütüphaneyi öğrenmeyi kolaylaştırır.
- **Riskler ve önlemler:**
  - *Risk:* Agent MediatR veya AutoMapper ekler. *Önlem:* `CLAUDE.md` kuralı ve merkezi paket yönetiminde izin verilmeyen paketler için bir kontrol.
  - *Risk:* Decorator sırası bozulur. *Önlem:* sırayı doğrulayan bir entegrasyon testi.
  - *Risk:* Bir query yanlışlıkla durum değiştirir. *Önlem:* query handler'larında `AsNoTracking` zorunluluğu ve kod incelemesi.

## Uygulama ve doğrulama

- P1-03: arayüzler, üç decorator ve kayıt kodu.
- Mimari testleri: her command handler'ın adı `CommandHandler` ile, her query handler'ın adı `QueryHandler` ile biter; handler'lar `internal sealed`'dır; `Application` hiçbir handler'ı başka bir handler'dan çağırmaz.
- Paket kontrolü: `MediatR` ve `AutoMapper` paketleri CI'da yasaklı listededir.

## Yeniden değerlendirme

- Senaryo sayısı arttıkça elle mapping kodu belirgin bir yük haline gelirse (Mapperly değerlendirilir).
- Faz 5'te modüller arası mesajlaşma için bir mesajlaşma kütüphanesi seçildiğinde, aynı kütüphanenin süreç içi kullanım için de uygun olup olmadığı değerlendirilir.

## Kaynaklar

- ADR-0001; `docs/product/nfr.md`
- Gamma ve diğerleri, *Design Patterns* (1994), Decorator kalıbı
