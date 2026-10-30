# API'ye Genel Bakış — MVP (v0.1.0)

> Durum: Onaylandı v1.0 · Repodaki yeri: `docs/api/overview.md` · Roadmap maddesi: P0-17

Bu doküman API'nin **sözleşmesini** tanımlar: kurallar, kaynaklar ve endpoint'ler. Alan bazındaki ayrıntılı istek/yanıt şemaları bu dokümanda değil, koddan üretilen OpenAPI dokümanında yer alır (P1-14). Bu doküman, o şemaların hangi kurallara uyması gerektiğini belirler.

## 1. Genel kurallar

### 1.1 Adresler ve sürümleme

- Tüm endpoint'ler `/api/v1` altındadır. Sağlık kontrolleri istisnadır: `/health/live`, `/health/ready` (NFR-REL-07).
- Kaynak adları **çoğul ve kebab-case**: `/api/v1/students`, `/api/v1/payment-methods`.
- Sürüm, sadece geriye dönük uyumsuz (breaking) bir değişiklikte artar (`/api/v2`). Yeni alan veya yeni endpoint eklemek uyumsuz değildir; alan silmek, yeniden adlandırmak veya anlamını değiştirmek uyumsuzdur.
- İstemciler, tanımadıkları yeni alanları yok saymalıdır.

### 1.2 Veri biçimleri

| Veri | Biçim | Örnek |
| --- | --- | --- |
| JSON alan adları | camelCase | `firstName`, `birthYearRange` |
| ID'ler | UUID v7, metin | `"0192f0c5-3b7a-7c2e-9f1a-2b3c4d5e6f70"` |
| Tarih (gün) | ISO 8601 `YYYY-MM-DD` | `"2027-01-14"` |
| Saat (gün içi) | `HH:mm`, okulun saat diliminde | `"17:00"` |
| An (zaman damgası) | ISO 8601, UTC, `Z` ile | `"2027-01-14T14:03:12Z"` |
| Dönem (ay) | `YYYY-MM` | `"2027-01"` |
| Para | Nesne; tutar **metin olarak** ondalık sayı | `{ "amount": "1250.00", "currency": "TRY" }` |
| Enum değerleri | Koddaki İngilizce adıyla, metin | `"Active"`, `"Present"` |
| Telefon | E.164 | `"+905321234567"` |

Para tutarının neden sayı değil de metin olduğu: JSON sayıları JavaScript'te kayan noktalı sayıya (double) dönüşür; `0.1 + 0.2` gibi işlemlerde küçük hatalar oluşur. Tutarı metin olarak göndermek, ondalık değerin istemciye bozulmadan ulaşmasını garanti eder (NFR'deki "para asla float değil" kuralının API'deki karşılığı).

### 1.3 Kimlik doğrulama

- Herkese açık endpoint'ler dışındaki her istek `Authorization: Bearer <access token>` başlığı taşır (NFR-SEC-07, ADR-0006).
- Herkese açık endpoint'ler: `POST /auth/login`. `POST /auth/refresh` ve `POST /auth/logout` access token yerine refresh cookie'si ve `X-Requested-With` başlığıyla çalışır (DEC-055).
- Rol yetkisi her endpoint'te açıkça tanımlanır (aşağıdaki tablolardaki "Rol" sütunu).

### 1.4 HTTP metotları ve durum kodları

| İşlem | Metot | Başarılı yanıt |
| --- | --- | --- |
| Liste / tek kayıt okuma | `GET` | `200 OK` |
| Oluşturma | `POST` | `201 Created` + `Location` başlığı + oluşturulan kaynak |
| Tamamen değiştirme (idempotent) | `PUT` | `200 OK` + güncel kaynak |
| Durum geçişi (aksiyon) | `POST /{kaynak}/{id}/{aksiyon}` | `200 OK` + güncel kaynak |
| Silme | `DELETE` | `204 No Content` |

Hata durum kodları ve yanıt biçimi ADR-0007'de tanımlıdır; tüm hata kodları `docs/api/error-codes.md`'dedir.

### 1.5 Durum geçişleri aksiyon endpoint'leridir

Bir kaynağın durumunu değiştiren iş işlemleri, genel bir `PATCH` veya `PUT` ile değil, **adı olan aksiyon endpoint'leri** ile yapılır:

- `POST /students/{id}/leave` — `PATCH /students/{id} { "status": "Left" }` değil.
- `POST /groups/{id}/archive`, `POST /enrollments/{id}/end`, `POST /payments/{id}/cancel`, `POST /coaches/{id}/deactivate`.

Sebebi: "öğrenci ayrıldı" bir alan güncellemesi değil, kendi kuralları (ayrılma tarihi, kayıtların sonlandırılması) ve yan etkileri (`StudentLeft` event'i) olan bir iş olayıdır. Aksiyon endpoint'i, domain'deki metotla (`student.Leave(date)`) birebir eşleşir ve yetkilendirmesi ayrı yapılabilir.

### 1.6 Sayfalama, filtreleme, sıralama

- Liste endpoint'leri her zaman sayfalıdır (NFR-PRF-05). Sayfa numarasına dayalı (offset) sayfalama kullanılır: `?page=1&pageSize=20`. Varsayılan `pageSize` 20, en fazla 100.
- Liste yanıtı biçimi:

```json
{
  "items": [ … ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 137,
  "totalPages": 7
}
```

- Filtreler sorgu parametresidir: `?status=Active&groupId=…&search=ali`.
- Sıralama `sort` parametresiyle; azalan sıra için `-` öneki: `?sort=lastName`, `?sort=-createdAt`. Her endpoint izin verdiği sıralama alanlarını belirtir; diğerleri `400` döner.

Offset sayfalamanın bilinen zayıflığı (sayfalar arasında kayıt eklenirse aynı kaydın iki sayfada görünmesi ve büyük sayfa numaralarında yavaşlama), bu projedeki liste boyutlarında (bir okulda en fazla birkaç yüz öğrenci) bir sorun değildir. İstemcinin "toplam kaç kayıt, kaç sayfa" bilgisine ihtiyacı olduğu için cursor tabanlı sayfalamaya tercih edilir.

### 1.7 Eşzamanlı düzenleme (lost update)

İki admin aynı grubu aynı anda düzenlerse, sonra kaydedenin değişikliği ilkinin değişikliğini fark etmeden ezer. Bunu önlemek için:

- Düzenlenebilir her kaynağın yanıtında bir `version` alanı bulunur (PostgreSQL `xmin` değerinden türetilir, domain modeli §8).
- `PUT` istekleri, istemcinin okuduğu `version` değerini gövdede geri gönderir.
- Kayıt bu arada başka biri tarafından değiştirilmişse yanıt `409 Conflict`, kod `Concurrency.Conflict` olur. İstemci kullanıcıya "Bu kayıt siz düzenlerken değiştirildi, sayfayı yenileyin" mesajını gösterir.

### 1.8 Uyarılar

Bazı işlemler başarılı olur ama kullanıcının bilmesi gereken bir durum vardır (ör. US-10: grubun program saati aynı tesisteki başka bir grupla çakışıyor; DEC-012 gereği bu bir hata değil uyarıdır). Bu durumda başarılı yanıt bir `warnings` dizisi içerir:

```json
{
  "id": "…",
  "slots": [ … ],
  "warnings": [ { "code": "Schedule.FacilityOverlap", "relatedId": "<diğer grubun ID'si>" } ]
}
```

Uyarı metinleri, hata metinleri gibi istemcinin dil dosyasından üretilir.

### 1.9 Her endpoint tek bir yanıt şekli döner

Aynı endpoint, çağıranın rolüne göre farklı alanlar döndürmez. Rol bazlı farklı bir görünüm gerekiyorsa, bu ayrı bir endpoint'tir. Örneğin antrenörün öğrenci görünümü (US-14), `GET /students/{id}`'nin kısıtlanmış bir hali değil, ayrı bir endpoint'tir: `GET /groups/{id}/roster`.

Sebebi: "rol antrenörse şu alanları gizle" mantığı her yanıtta unutulabilecek bir kontroldür. Ayrı endpoint'lerde, antrenöre giden yanıtın şekli koddan ve OpenAPI'den açıkça görülür ve contract testi o şekli doğrular. Veli bilgilerini içeren bir alan, antrenöre giden yanıt tipinde hiç tanımlı değildir.

## 2. Endpoint'ler

Roller: **PA** = Platform Admin, **SA** = Okul Admini, **CO** = Antrenör, **Herkes** = giriş yapmış her kullanıcı. "CO (kendi)" antrenörün sadece kendi gruplarıyla ilgili kayıtlara erişebildiğini; başka bir kayıt için `404` döndüğünü belirtir.

### 2.1 Kimlik doğrulama — `Identity`

| Metot | Yol | Açıklama | Rol | Hikaye |
| --- | --- | --- | --- | --- |
| `POST` | `/auth/login` | E-posta ve şifreyle giriş; gövdede access token, cookie'de refresh token | Herkese açık | US-03 |
| `POST` | `/auth/refresh` | Refresh token ile yeni token çifti | Refresh cookie | US-04 |
| `POST` | `/auth/logout` | Refresh token'ı iptal eder, cookie'yi siler | Refresh cookie | US-04 |
| `POST` | `/auth/change-password` | Şifre değiştirme; `pwd_change_required` olan token ile de çalışır | Herkes | US-05 |
| `GET` | `/me` | Giriş yapmış kullanıcının profili ve rolü | Herkes | — |

### 2.2 Platform — `Tenancy`

| Metot | Yol | Açıklama | Rol | Hikaye |
| --- | --- | --- | --- | --- |
| `GET` | `/platform/schools` | Okulları listeler | PA | US-01 |
| `POST` | `/platform/schools` | Okul ve ilk Okul Admini oluşturur (atomik) | PA | US-01 |
| `POST` | `/platform/schools/{id}/admins/{userId}/reset-password` | Okul Admininin şifresini sıfırlar | PA | US-05 |

### 2.3 Antrenörler — `Identity`

| Metot | Yol | Açıklama | Rol | Hikaye |
| --- | --- | --- | --- | --- |
| `GET` | `/coaches` | Antrenörleri listeler | SA | US-08 |
| `POST` | `/coaches` | Antrenör hesabı oluşturur (geçici şifre) | SA | US-08 |
| `GET` | `/coaches/{id}` | Antrenör ayrıntısı | SA | US-08 |
| `PUT` | `/coaches/{id}` | Ad, telefon günceller | SA | US-08 |
| `POST` | `/coaches/{id}/deactivate` | Pasif hale getirir, token'ları iptal eder | SA | US-08 |
| `POST` | `/coaches/{id}/reset-password` | Yeni geçici şifre verir | SA | US-05 |

### 2.4 Branşlar ve tesisler — `Organization`

| Metot | Yol | Açıklama | Rol | Hikaye |
| --- | --- | --- | --- | --- |
| `GET` | `/sports` | Branşları listeler (`?includeArchived=`) | SA | US-06 |
| `POST` | `/sports` | Branş oluşturur | SA | US-06 |
| `PUT` | `/sports/{id}` | Branşı yeniden adlandırır | SA | US-06 |
| `POST` | `/sports/{id}/archive` | Arşivler | SA | US-06 |
| `DELETE` | `/sports/{id}` | Siler; grubu varsa `409` | SA | US-06 |
| `GET` | `/facilities` | Tesisleri listeler | SA | US-07 |
| `POST` | `/facilities` | Tesis oluşturur | SA | US-07 |
| `PUT` | `/facilities/{id}` | Tesisi günceller | SA | US-07 |
| `POST` | `/facilities/{id}/archive` | Arşivler | SA | US-07 |
| `DELETE` | `/facilities/{id}` | Siler; aktif grup kullanıyorsa `409` | SA | US-07 |

### 2.5 Gruplar — `Groups`

| Metot | Yol | Açıklama | Rol | Hikaye |
| --- | --- | --- | --- | --- |
| `GET` | `/groups` | Grupları listeler (`?sportId=&coachId=&status=`) | SA | US-09 |
| `POST` | `/groups` | Grup oluşturur | SA | US-09 |
| `GET` | `/groups/{id}` | Grup ayrıntısı (program saatleri dahil) | SA, CO (kendi) | US-09, US-12 |
| `PUT` | `/groups/{id}` | Ad, doğum yılı aralığı, kontenjan, tesis, antrenör günceller | SA | US-11 |
| `PUT` | `/groups/{id}/schedule` | Haftalık program saatlerini tamamen değiştirir; tesis çakışmasında `warnings` | SA | US-10 |
| `POST` | `/groups/{id}/archive` | Arşivler; aktif kayıtları sonlandırır | SA | US-11 |
| `GET` | `/groups/{id}/sessions` | Bir tarih aralığındaki planlanan antrenmanlar (`?from=&to=`) | SA, CO (kendi) | US-12, US-19 |
| `GET` | `/groups/{id}/roster` | Aktif öğrenciler: ad, doğum yılı, acil durum telefonu. **Antrenör görünümü** | SA, CO (kendi) | US-14 |
| `GET` | `/groups/{id}/fees` | Aidat geçmişi | SA | US-22 |
| `POST` | `/groups/{id}/fees` | Bir dönemden itibaren geçerli yeni aidat tanımlar | SA | US-22 |

### 2.6 Antrenörün kendi görünümü — `Groups`, `Announcements`

| Metot | Yol | Açıklama | Rol | Hikaye |
| --- | --- | --- | --- | --- |
| `GET` | `/me/groups` | Antrenörün aktif grupları | CO | US-12 |
| `GET` | `/me/sessions` | Antrenörün bir gündeki antrenmanları (`?date=`, varsayılan bugün) | CO | US-12 |
| `GET` | `/me/announcements` | Antrenörün gruplarına yapılan duyurular | CO | US-25 |

### 2.7 Öğrenciler ve veliler — `Students`

| Metot | Yol | Açıklama | Rol | Hikaye |
| --- | --- | --- | --- | --- |
| `GET` | `/students` | Arama ve filtreleme (`?search=&status=&sportId=&groupId=`) | SA | US-16 |
| `POST` | `/students` | Öğrenci kaydı: veliler (mevcut veli ID'si veya yeni veli bilgisi) ve acil durum kişisi ile | SA | US-13 |
| `GET` | `/students/{id}` | Tam öğrenci ayrıntısı (veliler dahil). **Sadece admin** | SA | US-13 |
| `PUT` | `/students/{id}` | Öğrenci bilgileri ve acil durum kişisi günceller | SA | US-13 |
| `PUT` | `/students/{id}/guardians` | Veli bağlantılarını tamamen değiştirir | SA | US-13 |
| `POST` | `/students/{id}/leave` | Ayrıldı olarak işaretler; kayıtları sonlandırır | SA | US-15 |
| `POST` | `/students/{id}/reactivate` | Ayrılmış öğrenciyi yeniden aktif yapar | SA | US-15 |
| `GET` | `/students/{id}/enrollments` | Öğrencinin kayıt geçmişi | SA | US-18 |
| `GET` | `/students/{id}/attendance` | Antrenman bazında yoklama ve devam oranı (`?from=&to=`) | SA, CO (kendi) | US-21 |
| `GET` | `/guardians` | Telefonla veli arama (`?phone=`); kardeş kaydında mevcut veliyi bulmak için | SA | US-13 |
| `PUT` | `/guardians/{id}` | Veli bilgilerini günceller | SA | US-13 |

### 2.8 Kayıtlar — `Enrollments`

| Metot | Yol | Açıklama | Rol | Hikaye |
| --- | --- | --- | --- | --- |
| `POST` | `/enrollments` | Öğrenciyi gruba kaydeder (ADR-0005 kilitleriyle) | SA | US-17 |
| `POST` | `/enrollments/{id}/end` | Kaydı bir tarihle sonlandırır | SA | US-18 |
| `GET` | `/groups/{id}/enrollments` | Grubun kayıtları (`?status=`) | SA | US-17, US-18 |

### 2.9 Yoklama — `Attendance`

Bir antrenman, ilk yoklamaya kadar veritabanında yoktur (DEC-016). Bu yüzden adresi bir ID ile değil, onu benzersiz kılan bilgilerle oluşur: grup, tarih ve başlangıç saati. `{sessionKey}` biçimi `YYYY-MM-DDTHH:mm`'dir (ör. `2027-01-14T17:00`).

| Metot | Yol | Açıklama | Rol | Hikaye |
| --- | --- | --- | --- | --- |
| `GET` | `/groups/{id}/sessions/{sessionKey}/attendance` | O antrenmanın yoklama listesi: o tarihte aktif kayıtlı öğrenciler ve varsa mevcut durumları | SA, CO (kendi) | US-19 |
| `PUT` | `/groups/{id}/sessions/{sessionKey}/attendance` | Yoklamayı kaydeder; antrenman yoksa oluşturur, varsa günceller (idempotent). 48 saat kuralı | SA, CO (kendi) | US-19, US-20 |
| `GET` | `/groups/{id}/attendance` | Aylık antrenman × öğrenci tablosu (`?month=YYYY-MM`) | SA, CO (kendi) | US-21 |

Yoklamanın kaydedilmesi neden `POST` değil `PUT`: aynı istek iki kez gönderilirse (ör. antrenör "kaydet"e iki kez basarsa) sonuç değişmez. `PUT`'un tanımı gereği idempotent olması, US-19'daki "mükerrer yoklama oluşmaz" kriterini HTTP düzeyinde de ifade eder.

### 2.10 Ödemeler ve borçlar — `Billing`

| Metot | Yol | Açıklama | Rol | Hikaye |
| --- | --- | --- | --- | --- |
| `GET` | `/payments` | Ödemeleri listeler (`?studentId=&groupId=&period=&status=`) | SA | US-23 |
| `POST` | `/payments` | Elle ödeme kaydı | SA | US-23 |
| `POST` | `/payments/{id}/cancel` | Gerekçeyle iptal; düzenleme ve silme yok | SA | US-23 |
| `GET` | `/dues/outstanding` | Ödenmemiş borçlar (`?groupId=&period=`) | SA | US-24 |

### 2.11 Duyurular — `Announcements`

| Metot | Yol | Açıklama | Rol | Hikaye |
| --- | --- | --- | --- | --- |
| `GET` | `/groups/{id}/announcements` | Grubun duyuruları | SA, CO (kendi) | US-25 |
| `POST` | `/groups/{id}/announcements` | Duyuru yayınlar | SA | US-25 |

## 3. Kapsam kontrolü

Her Must ve Should kullanıcı hikayesinin en az bir endpoint'i vardır. Kullanıcı hikayelerinde açıkça geçmeyen ama arayüzün çalışması için gereken endpoint'ler: `GET /me`, `GET /guardians`, `POST /students/{id}/reactivate`, `PUT /guardians/{id}` ve Platform Admin'in okul listesi. Bunlar ilgili hikayelerin parçası olarak geliştirilir.

## 4. Onaylanan kararlar

| # | Karar |
| --- | --- |
| A1 | Para tutarları JSON'da metin olarak gönderilir: `{ "amount": "1250.00", "currency": "TRY" }` |
| A2 | Durum geçişleri adı olan aksiyon endpoint'leridir (`POST /students/{id}/leave`) |
| A3 | Offset sayfalama; yanıtta `items`, `page`, `pageSize`, `totalCount`, `totalPages` |
| A4 | Eşzamanlı düzenlemeye karşı `version` alanı; uyuşmazlıkta `409 Concurrency.Conflict` |
| A5 | Engelleyici olmayan durumlar başarılı yanıtta `warnings` dizisiyle döner |
| A6 | Her endpoint tek bir yanıt şekli döner; rol bazlı görünümler ayrı endpoint'tir |
