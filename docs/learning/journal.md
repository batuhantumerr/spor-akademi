# Öğrenme Günlüğü

> Repodaki yeri: `docs/learning/journal.md` · Roadmap maddesi: P0-20

Tamamlanan her roadmap maddesinden sonra buraya kısa bir not yazılır: hangi kavramı öğrendim, projede nerede kullandım ve bir mülakatta nasıl anlatırım. Notları **kendi cümlelerinle** yaz; agent bir taslak önerebilir ama kopyalanmaz. Bir kavramı kendi kelimelerinle yazamıyorsan, henüz tam anlamamışsın demektir; bu da değerli bir bilgidir.

Her faz geçişinde bu notlar, o fazın "mülakat kartları"na dönüşür (faz geçiş şablonu §6).

## Not şablonu

```markdown
### <Kavram>

- **Tarih / madde:** YYYY-AA-GG · P<faz>-<no>
- **Nedir (1–2 cümle):**
- **Projede nerede:** (dosya, ADR, hikaye)
- **Neden bu yolu seçtik, alternatifi neydi:**
- **Bedeli / zayıf yanı:**
- **30 saniyelik mülakat cevabı:**
- **Hâlâ emin olmadığım:**
```

## Örnek not

### Pessimistic locking ve deadlock

- **Tarih / madde:** 2026-10-27 · P0-12
- **Nedir:** Bir işlem, okuyacağı satırları değiştirmeden önce veritabanında kilitler (`SELECT ... FOR UPDATE`); aynı satırlara erişmek isteyen diğer işlemler, ilki bitene kadar bekler.
- **Projede nerede:** ADR-0005; öğrenci kaydında kontenjan kontrolü (US-17).
- **Neden bu yolu seçtik, alternatifi neydi:** Optimistic concurrency için gruba bir sayaç eklememiz gerekirdi; bu da `Enrollments` modülünün `Group` aggregate'ını değiştirmesi anlamına gelir ve modül kuralımızı (DEC-039) bozar. Serializable izolasyon ise tekrar deneme mantığı gerektirir.
- **Bedeli / zayıf yanı:** Kilitler transaction boyunca tutulur; transaction yavaşsa diğer istekler bekler. Kilitler farklı sırayla alınırsa deadlock oluşabilir; bu yüzden her zaman önce öğrenci, sonra grup kilitleniyor.
- **30 saniyelik mülakat cevabı:** "Kontenjan kontrolü klasik bir check-then-act yarışıydı: iki istek aynı anda 19 kişi görüp ikisi de kaydediyordu. Kayıt yaparken öğrenci ve grup satırını `FOR UPDATE` ile kilitledim; ikinci istek bekleyip güncel sayıyı görüyor. Deadlock'u önlemek için kilitleri her zaman aynı sırayla alıyorum. Ayrıca mükerrer kayda karşı veritabanında kısmi bir unique index var; son savunma hattı veritabanı."
- **Hâlâ emin olmadığım:** Kilit zaman aşımı (`lock_timeout`) ile deadlock tespiti arasındaki fark.

## Notlar

<!-- Yeni notları buraya, en yeni en üstte olacak şekilde ekle. -->
