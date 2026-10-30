# ADR-0002: Öğrencileri profil olarak modelle; reşit olmayanlar için veli zorunlu, yetişkinler için değil

> Repodaki yeri: `docs/adr/0002-students-are-profiles.md`

| Alan | Değer |
| --- | --- |
| Durum | `Accepted` |
| Tarih | 2026-10-27 |
| Karar veren | Batuhan Tümer |
| İlgili kararlar | DEC-002, DEC-003, DEC-007, DEC-008 |
| İlgili gereksinimler | US-13, US-14, US-15, NFR-PRV-01 |

## Bağlam

Platformun merkezindeki kişi öğrenci. Öğrencinin sistemde nasıl temsil edileceği; kimlik doğrulama tasarımını, kişisel verilerin nerede tutulduğunu ve faz 3'teki uygulamaları doğrudan etkiliyor.

Kararı etkileyen gerçekler:

- Öğrencilerin çoğu 4–17 yaş arasındaki çocuklar; onlarla ilgili iletişim ve ödemeler veliler üzerinden yürüyor.
- **Okullar yetişkin öğrenci de kabul ediyor** (ör. yetişkin yüzme kursları). Yetişkin bir öğrencinin velisi yoktur; iletişim ve ödeme doğrudan öğrenciyle yapılır.
- Çocukların kişisel verileri KVKK açısından özellikle dikkat gerektiriyor; veri minimizasyonu ilkesi (NFR-PRV-01) gereği ihtiyaç olmayan veri toplanmamalı.
- MVP'de veliler bile sisteme giriş yapmıyor (DEC-002); faz 3'te giriş eklenecek.
- Bir veli birden fazla çocuğun velisi olabilir (kardeşler); bir çocuğun birden fazla velisi olabilir.
- Antrenmanda bir kaza olduğunda antrenörün arayacağı kişi her zaman bir veli değildir: çocuk için bir büyükanne, yetişkin için eşi olabilir. Yetişkin bir öğrencinin acil durumda *kendisinin* aranması da anlamsızdır.

## Karar kriterleri

1. **Gizlilik:** özellikle çocuklar için gereksiz veri (e-posta, şifre, giriş geçmişi) tutulmamalı; antrenör sadece ihtiyaç duyduğunu görmeli.
2. **Gerçeğe uygunluk:** model hem çocuk hem yetişkin öğrencileri ve okulların gerçek işleyişini yansıtmalı.
3. **Basitlik:** tek bir öğrenci kavramı olmalı; kimlik doğrulama sistemi gerektiğinden fazla kişi türü taşımamalı.
4. **Gelişebilirlik:** faz 3'te giriş yetkisi eklemek, modeli yeniden yazmayı gerektirmemeli.

## Değerlendirilen seçenekler

### Öğrencinin temsili

#### Seçenek 1: Öğrenci bir kullanıcıdır

Her öğrencinin e-posta ve şifreli bir `User` kaydı vardır.

- **Artıları:** Büyük öğrenciler ve yetişkinler ileride kendi bilgilerini görebilir.
- **Eksileri:** Küçük çocuklar için e-posta ve şifre anlamsızdır. Kimlik doğrulama sistemi, hiç giriş yapmayacak binlerce hesap taşır. Çocuklara ait gereksiz veri toplanır.

#### Seçenek 2: Öğrenci bir profildir; çocuklar ve yetişkinler için ayrı tipler

`ChildStudent` ve `AdultStudent` diye iki ayrı varlık tanımlanır.

- **Artıları:** Her tipin kuralları ayrı ve nettir.
- **Eksileri:** Kayıt, yoklama ve aidat gibi her özellik iki tiple çalışmak zorunda kalır. Bir çocuk 18 yaşına bastığında kaydın bir tipten diğerine taşınması gerekir. Yaş, öğrencinin *ne olduğunu* değil, hangi *kuralların* geçerli olduğunu belirler.

#### Seçenek 3: Tek bir öğrenci profili; veli kuralı yaşa bağlı

Tek bir `Student` varlığı vardır. Reşit olmayan bir öğrencinin en az bir velisi olmak zorundadır; yetişkin bir öğrencinin velisi olmayabilir, bunun yerine kendi iletişim bilgileri zorunludur.

- **Artıları:** Kayıt, yoklama ve aidat tek bir öğrenci kavramıyla çalışır. Bir çocuğun 18 yaşına basması hiçbir veri taşıma işlemi gerektirmez. Çocuklar için gereksiz veri toplanmaz.
- **Eksileri:** Öğrencinin invariant'ları yaşa, dolayısıyla zamana bağlı hale gelir; testlerde zaman kontrol edilmelidir.

#### Seçenek 4: Tek bir `Person` tablosu ve roller (party model)

- **Artıları:** Bir kişinin birden fazla rolü olabilir (antrenör aynı zamanda veli).
- **Eksileri:** Bu proje için gereksiz derecede soyut; her rolün farklı kuralları tek bir varlıkta karışır.

### Acil durum kişisinin temsili

Mevcut domain modelinde (v1.0) acil durum kişisi, velilerden birinin üzerindeki bir işaretti (`IsEmergencyContact`). Bu, yetişkin öğrenciler için çalışmaz ve çocuklar için de gerçeği tam yansıtmaz.

#### Seçenek A: Velilerden biri acil durum kişisidir (mevcut model)

- **Eksileri:** Velisi olmayan yetişkinler için çalışmaz. Velilerden biri olmayan bir yakın (büyükanne) acil durum kişisi olamaz.

#### Seçenek B: Acil durum kişisi öğrenciye ait ayrı bir bilgidir

Her öğrencinin bir `EmergencyContact` value object'i vardır: ad, telefon ve yakınlık. Bu kişi bir veli olabilir de olmayabilir de.

- **Artıları:** Çocuklar ve yetişkinler için aynı şekilde çalışır. Antrenör görünümü veli tablosuna hiç dokunmaz; böylece antrenörün veli verisi görmesi teknik olarak imkansız hale gelir (DEC-007'nin uygulanması basitleşir).
- **Eksileri:** Acil durum kişisi bir veli ise bilgileri iki yerde durur; velinin telefonu değişirse acil durum telefonu kendiliğinden güncellenmez.

## Karar

**Öğrencinin temsili için Seçenek 3'ü, acil durum kişisi için Seçenek B'yi seçiyoruz.**

**Öğrenci:**

- Tek bir `Student` aggregate'ı vardır. Öğrenciler kimlik doğrulama sisteminin parçası değildir.
- **Reşit olmama kuralı:** öğrenci, kontrolün yapıldığı tarihte okulun saat dilimine göre 18 yaşından küçükse reşit değildir.
- **Reşit olmayan öğrenci:** en az bir velisi olmak zorundadır. Kendi telefonu ve e-postası isteğe bağlıdır ve varsayılan olarak toplanmaz.
- **Yetişkin öğrenci:** velisi olması gerekmez. Kendi telefonu zorunlu, e-postası isteğe bağlıdır.
- Kurallar, öğrenci oluşturulurken veya velileri ve iletişim bilgileri değiştirilirken kontrol edilir. Bir öğrencinin 18 yaşına basması mevcut kaydı geçersiz kılmaz; veli bağlantıları kalır ve isteğe bağlı hale gelir.
- **Faz 3:** Veliler giriş yetkisi kazandığında `Guardian`'a isteğe bağlı bir `UserId` eklenir. Yetişkin öğrencilere giriş yetkisi verilmesi aynı yöntemle (`Student`'a isteğe bağlı `UserId`) faz 3'te değerlendirilir. Reşit olmayan öğrenciler hiçbir zaman kullanıcı olmaz.

**Acil durum kişisi:**

- Her öğrencinin, istisnasız, tam olarak bir `EmergencyContact`'ı vardır: ad, telefon, yakınlık (ör. "Anne", "Eşi", "Büyükanne").
- Acil durum kişisi bir veli olabilir; arayüz, kayıt sırasında bir velinin bilgilerini kopyalamayı kolaylaştırır. Ancak bilgiler öğrenci üzerinde ayrı tutulur ve birbirine bağlı değildir.
- Antrenör, DEC-007'ye uygun olarak sadece acil durum kişisinin **telefonunu** görür.

## Sonuçlar

- **Olumlu:**
  - Çocuk ve yetişkin öğrenciler tek bir modelle desteklenir; kayıt, yoklama ve aidat özellikleri ayrım yapmak zorunda kalmaz.
  - Çocuklar için e-posta, şifre veya giriş verisi tutulmaz.
  - Antrenör görünümü veli tablosuna hiç erişmez; veli verisinin antrenöre sızması mimari olarak imkansızdır.
  - Velisi olmayan biri (büyükanne, eş) acil durum kişisi olabilir.
- **Olumsuz:**
  - Öğrenci invariant'ları zamana bağlıdır; "bugün" `TimeProvider`'dan alınmalı ve testlerde sabitlenmelidir.
  - Acil durum kişisi bir veli olduğunda, velinin telefonu değişince acil durum telefonu ayrıca güncellenmelidir. Arayüz bu durumda bir uyarı gösterebilir (faz 1'de isteğe bağlı).
  - Aidat takibinde iletişim kurulacak kişi öğrenciye göre değişir: reşit olmayanlar için veliler, yetişkinler için öğrencinin kendisi.
- **Riskler ve önlemler:**
  - *Risk:* Yaş hesabında saat dilimi veya gün sınırı hatası (doğum gününde 17 mi 18 mi?). *Önlem:* yaş hesabı tek bir yerde (`Student` veya bir value object) yapılır ve doğum günü sınırları için ayrı birim testleri yazılır.

## Uygulama ve doğrulama

- `Student` invariant'ları birim testleriyle korunur: reşit olmayan + velisiz → hata; yetişkin + telefonsuz → hata; acil durum kişisi olmadan → hata; doğum günü sınırları.
- Mimari testi: `Students` modülü `Identity` modülüne bağımlı değildir.
- US-14 yanıt contract testi, antrenöre giden yanıtta veli bilgisi ve öğrencinin kendi iletişim bilgilerinin bulunmadığını doğrular.

## Yeniden değerlendirme

- Faz 3'te yetişkin öğrencilere veya büyük öğrencilere giriş yetkisi verilmesi gündeme geldiğinde.
- Okullar, 18 yaş dışında bir sınır (ör. 16 yaş üstünün kendi kaydını yaptırması) isterse; sınır o zaman okul bazında ayarlanabilir hale getirilir.

## Kaynaklar

- `docs/product/vision.md` §4, `docs/domain/domain-model.md` §4.6–4.7, `docs/product/nfr.md` §3
