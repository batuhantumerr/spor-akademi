# Kullanıcı Hikayeleri — MVP (v0.1.0)

> Durum: Onaylandı v1.1 · Repodaki yeri: `docs/product/user-stories.md` · Kaynak: `docs/product/vision.md`

## Bu doküman nasıl okunur

- Her hikaye şu kalıbı izler: **Bir** *rol* **olarak**, *bir yetenek* **istiyorum**, **böylece** *bir fayda* elde ederim.
- Kabul kriterleri Given / When / Then (Diyelim ki / Ne zaman / O zaman) yapısını kullanır. **Her kabul kriteri en az bir otomatik teste dönüşür.**
- Öncelikler, vizyon dokümanındaki MoSCoW kapsamını izler.
- Hikaye kimlikleri (ID) değişmez. GitHub issue'ları bunlara referans verir (ör. `US-17`).

## Yetki matrisi

| Yetenek | Platform Admin | Okul Admini | Antrenör |
| --- | --- | --- | --- |
| Okul oluşturma | ✅ | — | — |
| Branş ve tesis yönetimi | — | ✅ | — |
| Antrenör hesaplarını yönetme | — | ✅ | — |
| Grup ve program yönetimi | — | ✅ | Sadece kendi gruplarını görür |
| Öğrenci ve veli yönetimi | — | ✅ | — |
| Öğrencileri görüntüleme | — | ✅ | Sadece kendi grupları |
| Veli bilgilerini görüntüleme | — | ✅ | Sadece acil durum telefonu, kendi grupları |
| Öğrenci kaydı yapma / kaydı sonlandırma | — | ✅ | — |
| Yoklama alma | — | ✅ | Sadece kendi grupları |
| Yoklama geçmişini görüntüleme | — | ✅ | Sadece kendi grupları |
| Aidat ve ödemeler | — | ✅ | — |

Platform Admin, hiçbir okulun iş verisini (öğrenciler, yoklama, ödemeler) asla göremez.

---

## Epic 1 — Platform ve tenant yapısı

### US-01 · Okul oluşturma · Must

**Bir** Platform Admin **olarak**, bir okul ve o okulun ilk Okul Adminini oluşturmak **istiyorum**, **böylece** yeni bir okul platformu kullanmaya başlayabilir.

- **Diyelim ki** Platform Admin'im, **ne zaman** okul adını, benzersiz bir slug'ı, saat dilimini ve ilk adminin adı ile e-postasını gönderirsem, **o zaman** okul `Active` durumunda oluşturulur ve okul için bir Okul Admini hesabı açılır.
- **Diyelim ki** aynı slug'a sahip bir okul var, **ne zaman** gönderirsem, **o zaman** doğrulama hatası alırım ve hiçbir şey oluşturulmaz.
- **Diyelim ki** admin e-postası platformdaki herhangi bir kullanıcı tarafından zaten kullanılıyor, **ne zaman** gönderirsem, **o zaman** doğrulama hatası alırım ve hiçbir şey oluşturulmaz (işlem atomiktir: ya hepsi olur ya hiçbiri).
- **Diyelim ki** okul oluşturuldu, **o zaman** ilk Okul Admini, ilk girişte değiştirmesi zorunlu olan geçici bir şifre alır (bkz. US-05).
- **Diyelim ki** Platform Admin değilim, **ne zaman** bu işlemi çağırırsam, **o zaman** `403 Forbidden` alırım.

### US-02 · Tenant izolasyonu · Must

**Bir** Okul Admini **olarak**, okulumun verilerinin diğer tüm okullar için görünmez olmasını **istiyorum**, **böylece** çocukların ve ailelerin verileri gizli kalır.

- **Diyelim ki** A Okulu ve B Okulu kullanıcıları var, **ne zaman** A Okulu'ndan bir kullanıcı herhangi bir kaynağı listeler veya ararsa, **o zaman** sadece A Okulu kayıtları döner.
- **Diyelim ki** A Okulu kullanıcısı B Okulu'na ait bir kaydı ID'si ile isterse, **o zaman** yanıt `404 Not Found` olur (`403` değil; böylece kaydın var olduğu bile açığa çıkmaz).
- **Diyelim ki** A Okulu kullanıcısı B Okulu'na ait bir varlığa referans veren bir kayıt oluşturmaya veya güncellemeye çalışırsa (ör. B Okulu'nun öğrencisini kaydetmek), **o zaman** istek başarısız olur.
- **Diyelim ki** koda yeni bir varlık (entity) eklendi, **o zaman** bu varlık tenant'a bağlı olduğu halde `TenantId` içermiyorsa otomatik bir mimari testi başarısız olur.

---

## Epic 2 — Kimlik doğrulama ve hesaplar

### US-03 · Giriş yapma · Must

**Bir** kullanıcı **olarak**, e-postam ve şifremle giriş yapmak **istiyorum**, **böylece** rolüme ait özelliklere erişebilirim.

- **Diyelim ki** bilgiler doğru, **ne zaman** giriş yaparsam, **o zaman** kısa ömürlü bir access token ve bir refresh token alırım. Access token; kullanıcı ID'mi, rolümü ve `tenant_id`'mi içerir (Platform Admin için `tenant_id` yoktur).
- **Diyelim ki** bilgiler yanlış, **o zaman** e-postanın sistemde olup olmadığını belli etmeyen genel bir "e-posta veya şifre hatalı" mesajı alırım.
- **Diyelim ki** aynı hesap için 15 dakika içinde 5 başarısız deneme yapıldı, **o zaman** hesap 15 dakika boyunca geçici olarak kilitlenir.
- **Diyelim ki** hesabım veya okulum pasif hale getirildi, **o zaman** giriş yapamam.

### US-04 · Oturumun açık kalması ve çıkış yapma · Must

**Bir** kullanıcı **olarak**, oturumumun güvenli şekilde yenilenmesini ve çıkış yapabilmeyi **istiyorum**, **böylece** sürekli giriş yapmak zorunda kalmam ve ayrıldıktan sonra oturumum başkası tarafından kullanılamaz.

- **Diyelim ki** geçerli bir refresh token'ım var, **ne zaman** access token'ımın süresi dolarsa, **o zaman** yeni bir access token ve yeni bir refresh token alabilirim; eski refresh token geçersiz olur (rotation).
- **Diyelim ki** daha önce kullanılmış bir refresh token tekrar gönderildi, **o zaman** o kullanıcının tüm refresh token'ları iptal edilir (reuse detection: çalınmış token tespiti).
- **Diyelim ki** çıkış yaptım, **o zaman** mevcut refresh token'ım iptal edilir.

### US-05 · İlk giriş ve şifre değiştirme · Must

**Geçici şifresi olan bir** kullanıcı **olarak**, kendi şifremi belirlemeye zorlanmak **istiyorum**, **böylece** şifremi benden başka kimse bilmez.

- **Diyelim ki** hesabımda geçici şifre var, **ne zaman** giriş yaparsam, **o zaman** şifremi değiştirene kadar sadece şifre değiştirme işlemine erişebilirim.
- **Diyelim ki** şifremi değiştirdim, **o zaman** yeni şifre şifre politikasına uymalıdır (en az 10 karakter) ve diğer tüm oturumlarım iptal edilir.
- **Diyelim ki** şifremi unuttum, **o zaman** MVP'de bir Okul Admini şifremi sıfırlar ve yeni bir geçici şifre verilir. (E-posta ile kendi kendine şifre sıfırlama faz 2'dedir.)

---

## Epic 3 — Okul kurulumu

### US-06 · Branş yönetimi · Must

**Bir** Okul Admini **olarak**, okulumun sunduğu branşları tanımlamak **istiyorum**, **böylece** gruplar branşa göre düzenlenebilir.

- **Diyelim ki** bir adla branş oluşturdum, **o zaman** okulumun branşları arasında listelenir.
- **Diyelim ki** okulumda aynı ada sahip bir branş var, **o zaman** oluşturma başarısız olur (isimler global değil, okul bazında benzersizdir).
- **Diyelim ki** bir branşın grupları var, **ne zaman** onu silmeye çalışırsam, **o zaman** silme başarısız olur; bunun yerine arşivleyebilirim ve arşivlenen branş yeni grup oluştururken listede görünmez.

### US-07 · Tesis yönetimi · Must

**Bir** Okul Admini **olarak**, saha ve havuz gibi tesisleri tanımlamak **istiyorum**, **böylece** grupların nerede antrenman yaptığını bilirim.

- **Diyelim ki** ad, tür (`Field` / saha, `Pool` / havuz, `Court` / kort-salon, `Other` / diğer), adres ve isteğe bağlı kapasite ile bir tesis oluşturdum, **o zaman** listelenir.
- **Diyelim ki** bir tesis aktif bir grup tarafından kullanılıyor, **ne zaman** onu silmeye çalışırsam, **o zaman** silme başarısız olur; arşivleme yapılabilir.

### US-08 · Antrenör yönetimi · Must

**Bir** Okul Admini **olarak**, antrenör hesapları oluşturmak ve pasif hale getirmek **istiyorum**, **böylece** antrenörler yoklama alabilir ve ayrılan antrenörlerin erişimi kapanır.

- **Diyelim ki** ad, e-posta ve telefonla bir antrenör oluşturdum, **o zaman** okulumda geçici şifreli bir Antrenör hesabı açılır.
- **Diyelim ki** bir antrenörü pasif hale getirdim, **o zaman** artık giriş yapamaz ve tüm refresh token'ları iptal edilir.
- **Diyelim ki** bir antrenör pasif, **o zaman** grupları silinmez ama yeniden atama yapılana kadar "aktif antrenör yok" olarak gösterilir.

---

## Epic 4 — Gruplar ve antrenman programları

### US-09 · Grup oluşturma · Must

**Bir** Okul Admini **olarak**, bir antrenman grubu oluşturmak **istiyorum**, **böylece** öğrenciler branşa, yaşa ve saate göre düzenlenebilir.

- **Diyelim ki** branş, ad, doğum yılı aralığı (ör. 2015–2016), kontenjan, tesis ve antrenör girdim, **o zaman** grup oluşturulur.
- **Diyelim ki** doğum yılı aralığı geçersiz (başlangıç bitişten sonra) veya kontenjan 1'den küçük, **o zaman** oluşturma başarısız olur.
- Yaş uygunluğu, yaş aralığı yerine **doğum yılı aralığı** ile belirlenir. Türkiye'deki spor okulları grupları genellikle böyle oluşturur ("2015–2016 doğumlular") ve bu yöntem, bir öğrencinin sezonun ortasında yaş sınırını aşıp gruptan çıkmak zorunda kalmasını önler.

### US-10 · Haftalık program tanımlama · Must

**Bir** Okul Admini **olarak**, bir grubun haftalık antrenman saatlerini tanımlamak **istiyorum**, **böylece** antrenörler ve ofis grubun ne zaman antrenman yaptığını bilir.

- **Diyelim ki** gün, başlangıç saati ve bitiş saati ile bir zaman dilimi (slot) ekledim, **o zaman** grubun programına eklenir.
- **Diyelim ki** bitiş saati başlangıç saatinden sonra değil, **o zaman** slot reddedilir.
- **Diyelim ki** slot aynı grubun başka bir slotuyla çakışıyor, **o zaman** slot reddedilir.
- **Diyelim ki** slot, aynı tesisteki başka bir grubun slotuyla çakışıyor, **o zaman** bir uyarı görürüm ama yine de kaydedebilirim (büyük bir sahada iki grup aynı anda çalışabilir).
- **Diyelim ki** antrenörün aynı saatte başka bir grubu var, **o zaman** slot reddedilir.
- Tüm saatler okulun saat dilimine göredir.

### US-11 · Grubu düzenleme ve arşivleme · Must

**Bir** Okul Admini **olarak**, bir grubu düzenlemek veya arşivlemek **istiyorum**, **böylece** sezonlar arasında yapıyı güncel tutabilirim.

- **Diyelim ki** kontenjanı aktif kayıt sayısının altına düşürmeye çalıştım, **o zaman** değişiklik reddedilir.
- **Diyelim ki** bir grubu arşivledim, **o zaman** gruptaki tüm aktif kayıtlar bugünün tarihiyle sonlanır ve grup antrenör ekranlarında artık görünmez.

### US-12 · Antrenörün kendi gruplarını görmesi · Must

**Bir** Antrenör **olarak**, gruplarımı ve bugünkü antrenmanlarımı görmek **istiyorum**, **böylece** nerede olmam gerektiğini bilirim ve hızlıca yoklama alabilirim.

- **Diyelim ki** antrenörüm, **ne zaman** ana ekranımı açarsam, **o zaman** okulumun saat dilimine göre sadece kendi aktif gruplarımı ve bugünkü antrenmanları görürüm.
- **Diyelim ki** bana ait olmayan bir grubu istedim, **o zaman** `404 Not Found` alırım.

---

## Epic 5 — Öğrenciler ve veliler

### US-13 · Öğrenci kaydetme (çocuk ve yetişkin) · Must

**Bir** Okul Admini **olarak**, çocuk veya yetişkin bir öğrenciyi kaydetmek **istiyorum**, **böylece** okul ihtiyaç duyduğu bilgilere sahip olur.

- **Diyelim ki** öğrencinin adını, soyadını, doğum tarihini, cinsiyetini ve bir acil durum kişisini (ad, telefon, yakınlık) girdim, **o zaman** öğrenci `Active` durumunda oluşturulur.
- **Diyelim ki** öğrenci kayıt tarihinde 18 yaşından küçük ve hiç veli girilmedi, **o zaman** oluşturma başarısız olur.
- **Diyelim ki** öğrenci 18 yaşında veya büyük, **o zaman** veli girmek zorunlu değildir ama öğrencinin kendi telefonu zorunludur.
- **Diyelim ki** acil durum kişisi girilmedi, **o zaman** oluşturma başarısız olur.
- **Diyelim ki** okulumda aynı telefona sahip bir veli zaten var, **o zaman** yeni bir kopya oluşturulmaz, mevcut veli bağlanır (kardeşler aynı veliyi paylaşır).
- **Diyelim ki** acil durum kişisini girerken velilerden birini seçtim, **o zaman** o velinin adı ve telefonu acil durum kişisi alanlarına kopyalanır (bilgiler sonradan birbirinden bağımsızdır).
- **Diyelim ki** öğrencinin yaşı tam 18'e bastığı gün kayıt yapılıyor, **o zaman** öğrenci yetişkin sayılır (doğum günü sınırı okulun saat dilimine göre hesaplanır).

### US-14 · Antrenörün acil durum kişisini görmesi · Must

**Bir** Antrenör **olarak**, gruplarımdaki öğrencilerin acil durum telefonunu görmek **istiyorum**, **böylece** bir öğrenci sakatlanırsa hızlı hareket edebilirim.

- **Diyelim ki** öğrenci gruplarımdan birine aktif olarak kayıtlı, **ne zaman** öğrenciyi görüntülersem, **o zaman** sadece adını, doğum yılını ve acil durum kişisinin telefon numarasını görürüm.
- **Diyelim ki** öğrenci benim gruplarımda değil, **o zaman** `404 Not Found` alırım.
- **Diyelim ki** antrenöre giden herhangi bir yanıt, **o zaman** bu yanıt hiçbir zaman veli bilgisi, öğrencinin kendi telefonu veya e-postası ya da acil durum kişisinin telefonu dışındaki bilgilerini içermez (yanıtın yapısı üzerinde bir contract test ile doğrulanır).

### US-15 · Öğrenciyi ayrıldı olarak işaretleme · Must

**Bir** Okul Admini **olarak**, bir öğrenciyi ayrıldı olarak işaretlemek **istiyorum**, **böylece** aktif listelerde görünmez ve verileri ileride anonimleştirilebilir.

- **Diyelim ki** bir öğrenciyi bir tarihle `Left` (ayrıldı) olarak işaretledim, **o zaman** tüm aktif kayıtları o tarihte sonlanır.
- **Diyelim ki** öğrenci `Left` durumunda, **o zaman** yeniden aktifleştirilene kadar hiçbir gruba kaydedilemez.
- **Diyelim ki** öğrenci `Left` durumunda, **o zaman** geçmiş yoklamaları geçmiş ekranında görünmeye devam eder.

### US-16 · Öğrenci arama ve listeleme · Must

**Bir** Okul Admini **olarak**, öğrencileri aramak ve filtrelemek **istiyorum**, **böylece** bir öğrenciyi hızlıca bulabilirim.

- **Diyelim ki** ismin bir kısmıyla arama yaptım, **o zaman** büyük-küçük harf ve Türkçe karakter farkı gözetmeden eşleşen öğrenciler döner ("Şükrü" ile "sukru" eşleşir).
- **Diyelim ki** duruma, branşa veya gruba göre filtreledim, **o zaman** sonuçlar buna göre süzülür.
- **Diyelim ki** sonuçlar bir sayfayı aşıyor, **o zaman** sayfalanır (varsayılan 20, sayfa başına en fazla 100).

---

## Epic 6 — Kayıt (Enrollment)

### US-17 · Öğrenciyi bir gruba kaydetme · Must

**Bir** Okul Admini **olarak**, bir öğrenciyi bir gruba kaydetmek **istiyorum**, **böylece** antrenmanlara katılabilir.

- **Diyelim ki** öğrenci `Active`, grubun doğum yılı aralığında doğmuş, grupta boş yer var ve öğrencinin diğer aktif kayıtlarında çakışan bir saat yok, **o zaman** başlangıç tarihli aktif bir kayıt oluşturulur.
- **Diyelim ki** grup dolu, **o zaman** kayıt "grup dolu" hatasıyla başarısız olur.
- **Diyelim ki** öğrencinin doğum yılı aralığın dışında, **o zaman** kayıt başarısız olur.
- **Diyelim ki** öğrenci bu gruba zaten aktif olarak kayıtlı, **o zaman** kayıt başarısız olur.
- **Diyelim ki** öğrencinin başka bir aktif kaydında çakışan bir saat var, **o zaman** kayıt başarısız olur ve hata mesajında çakışan grubun adı belirtilir.
- **Diyelim ki** iki admin aynı anda iki farklı öğrenciyi son boş yere kaydetmeye çalışıyor, **o zaman** sadece biri başarılı olur (eşzamanlılığa karşı güvenli).

### US-18 · Kaydı sonlandırma · Must

**Bir** Okul Admini **olarak**, bir öğrencinin bir gruptaki kaydını sonlandırmak **istiyorum**, **böylece** yer boşalır.

- **Diyelim ki** aktif bir kayıt var, **ne zaman** onu bir tarihle sonlandırırsam, **o zaman** kayıt `Ended` olur ve yer o tarihten itibaren boşalır.
- **Diyelim ki** kayıt sonlandı, **o zaman** öğrencinin geçmişinde saklanmaya devam eder.

---

## Epic 7 — Yoklama

### US-19 · Yoklama alma · Must

**Bir** Antrenör **olarak**, grubumun bir antrenmanı için yoklama kaydetmek **istiyorum**, **böylece** okul kimin katıldığını bilir.

- **Diyelim ki** grubumun bir antrenmanı bugün veya geçmişte, **ne zaman** onu açarsam, **o zaman** o tarihte gruba aktif olarak kayıtlı tüm öğrencileri görürüm.
- **Diyelim ki** her öğrenciyi `Present` (geldi), `Absent` (gelmedi), `Late` (geç kaldı) veya `Excused` (mazeretli) olarak işaretleyip kaydettim, **o zaman** yoklama, kimin ve ne zaman kaydettiği bilgisiyle birlikte saklanır.
- **Diyelim ki** antrenman gelecekte, **o zaman** yoklama kaydedemem.
- **Diyelim ki** o antrenman için zaten yoklama var, **o zaman** kaydetmek yeni bir kopya oluşturmaz, mevcut yoklamayı günceller (idempotent).

### US-20 · Yoklamayı düzeltme · Must

**Bir** Antrenör **olarak**, yoklamadaki bir hatayı düzeltmek **istiyorum**, **böylece** kayıtlar doğru olur.

- **Diyelim ki** antrenmanın bitişinin üzerinden 48 saatten az geçti, **o zaman** yoklamayı düzenleyebilirim.
- **Diyelim ki** 48 saatten fazla geçti, **o zaman** sadece bir Okul Admini düzenleyebilir.
- **Diyelim ki** herhangi bir düzenleme yapıldı, **o zaman** önceki değer, kimin ve ne zaman değiştirdiği saklanır.

### US-21 · Yoklama geçmişi · Must

**Bir** Okul Admini veya Antrenör **olarak**, yoklama geçmişini görmek **istiyorum**, **böylece** bir öğrencinin katılımını takip edebilirim.

- **Diyelim ki** bir öğrenci seçtim, **o zaman** antrenman bazında yoklamasını ve seçilen tarih aralığı için devam oranını görürüm.
- **Diyelim ki** bir grup seçtim, **o zaman** seçilen ay için antrenman × öğrenci şeklinde bir tablo görürüm.
- **Diyelim ki** Antrenör'üm, **o zaman** sadece kendi gruplarımın geçmişini görürüm.

---

## Epic 8 — Aidatlar · Should

### US-22 · Aylık aidat tanımlama · Should

**Bir** Okul Admini **olarak**, grup başına aylık bir aidat belirlemek **istiyorum**, **böylece** borçlar hesaplanabilir.

- **Diyelim ki** bir grup için bir tutar (TL) belirledim, **o zaman** bu tutar seçilen aydan itibaren geçerli olur; geçmiş aylar eski tutarı korur.

### US-23 · Ödeme kaydetme · Should

**Bir** Okul Admini **olarak**, nakit veya havale ile yapılan bir ödemeyi kaydetmek **istiyorum**, **böylece** kimin ödediğini bilirim.

- **Diyelim ki** öğrenci, kayıt, ay, tutar, ödeme yöntemi ve tarih ile bir ödeme kaydettim, **o zaman** ödeme saklanır ve o kaydın o aylık borcu azalır.
- **Diyelim ki** bir ödeme hatalı girildi, **o zaman** (bir gerekçe ile) iptal edilebilir ama asla düzenlenemez veya silinemez; böylece geçmiş denetlenebilir kalır.
- Para değerleri her zaman ondalık (decimal) olarak saklanır, asla kayan noktalı sayı (float) olarak değil.

### US-24 · Ödenmemiş aidatlar · Should

**Bir** Okul Admini **olarak**, ödenmemiş borçların listesini görmek **istiyorum**, **böylece** ailelerle iletişime geçebilirim.

- **Diyelim ki** bir kayıt bir ayda aktifti ve o ayın borcu tamamen ödenmedi, **o zaman** kalan tutarla birlikte ödenmemişler listesinde görünür.
- **Diyelim ki** listeyi açtım, **o zaman** gruba ve aya göre filtreleyebilirim.

---

## Epic 9 — Duyurular · Could

### US-25 · Grup duyuruları · Could

**Bir** Okul Admini **olarak**, bir gruba duyuru yayınlamak **istiyorum**, **böylece** grubun antrenörü bilgilendirilir.

- **Diyelim ki** bir gruba duyuru yayınladım, **o zaman** grubun antrenörü bunu ana ekranında görür.

---

## Onaylanan kararlar

| Hikaye | Karar |
| --- | --- |
| US-03 | 15 dakika içinde 5 başarısız girişten sonra hesap 15 dakika kilitlenir |
| US-09 | Gruplar yaş aralığı yerine doğum yılı aralığı kullanır |
| US-10 | Tesis çakışmaları hata değil uyarı verir |
| US-16 | İsim aramasında büyük-küçük harf ve Türkçe karakter farkı gözetilmez |
| US-20 | Antrenörler yoklamayı antrenmandan sonraki 48 saate kadar düzenleyebilir |
