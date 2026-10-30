# Hata ve Uyarı Kodları

> Durum: Onaylandı v1.0 · Repodaki yeri: `docs/api/error-codes.md` · Kaynak: ADR-0007

API'nin döndüğü tüm hata ve uyarı kodlarının kataloğu. Kurallar:

- Kod biçimi `<Alan>.<Durum>`'dur; İngilizce ve PascalCase'tir.
- **Yayınlanmış bir kod asla değiştirilmez veya başka bir anlamda kullanılmaz.** Kullanımdan kalkan kod "Kullanımdan kalktı" olarak işaretlenir.
- Kodun HTTP durumu, türünden gelir (ADR-0007): `Validation` 400, `Unauthorized` 401, `Forbidden` 403, `NotFound` 404, `Conflict` 409, `BusinessRule` 422.
- "Türkçe mesaj" sütunu, Angular dil dosyasındaki metnin başlangıç önerisidir; asıl kaynak istemcinin dil dosyasıdır.
- Yeni bir kod eklendiğinde bu dosya ve istemcinin dil dosyası aynı PR'da güncellenir.

## Genel

| Kod | Tür | Türkçe mesaj | Kaynak |
| --- | --- | --- | --- |
| `Validation.Failed` | Validation | Formda düzeltilmesi gereken alanlar var. | ADR-0007 |
| `Concurrency.Conflict` | Conflict | Bu kayıt siz düzenlerken başka biri tarafından değiştirildi. Sayfayı yenileyip tekrar deneyin. | API §1.7 |
| `Lock.Timeout` | Conflict | İşlem şu anda yapılamıyor. Lütfen birkaç saniye sonra tekrar deneyin. | ADR-0005 |
| `Request.TooLarge` | Validation | Gönderilen veri çok büyük. | NFR-SEC-12 |
| `Unexpected.Error` | — (500) | Beklenmeyen bir hata oluştu. Sorun devam ederse bu kodu destek ekibine iletin: {traceId} | ADR-0007 |

## Value object'ler (alan bazında doğrulama)

| Kod | Tür | Türkçe mesaj |
| --- | --- | --- |
| `Email.Invalid` | Validation | Geçerli bir e-posta adresi girin. |
| `PhoneNumber.Invalid` | Validation | Geçerli bir telefon numarası girin. |
| `Slug.Invalid` | Validation | Sadece küçük harf, rakam ve tire kullanın. |
| `TimeRange.Invalid` | Validation | Bitiş saati başlangıç saatinden sonra olmalı. |
| `BirthYearRange.Invalid` | Validation | Başlangıç yılı bitiş yılından büyük olamaz. |
| `Money.Invalid` | Validation | Geçerli bir tutar girin. |
| `Field.Required` | Validation | Bu alan zorunludur. |
| `Field.TooLong` | Validation | Bu alan çok uzun. |

## Kimlik doğrulama — `Identity`

| Kod | Tür | Türkçe mesaj | Hikaye |
| --- | --- | --- | --- |
| `Auth.InvalidCredentials` | Unauthorized | E-posta veya şifre hatalı. | US-03 |
| `Auth.AccountLocked` | Unauthorized | Çok fazla hatalı deneme yapıldı. Hesabınız 15 dakika kilitlendi. | US-03 |
| `Auth.AccountInactive` | Unauthorized | Hesabınız aktif değil. Okul yöneticinizle iletişime geçin. | US-03 |
| `Auth.SessionExpired` | Unauthorized | Oturumunuzun süresi doldu. Lütfen tekrar giriş yapın. | US-04 |
| `Auth.PasswordChangeRequired` | Forbidden | Devam etmek için şifrenizi değiştirmeniz gerekiyor. | US-05 |
| `Auth.CurrentPasswordIncorrect` | BusinessRule | Mevcut şifreniz hatalı. | US-05 |
| `Auth.PasswordTooWeak` | Validation | Şifre en az 10 karakter olmalı ve çok yaygın bir şifre olmamalı. | US-05 |
| `User.EmailTaken` | Conflict | Bu e-posta adresi zaten kullanılıyor. | US-01, US-08 |
| `User.NotFound` | NotFound | Kullanıcı bulunamadı. | US-08 |
| `User.AlreadyInactive` | BusinessRule | Bu kullanıcı zaten pasif. | US-08 |

## Platform — `Tenancy`

| Kod | Tür | Türkçe mesaj | Hikaye |
| --- | --- | --- | --- |
| `School.SlugTaken` | Conflict | Bu kısa ad başka bir okul tarafından kullanılıyor. | US-01 |
| `School.InvalidTimeZone` | Validation | Geçerli bir saat dilimi seçin. | US-01 |
| `School.NotFound` | NotFound | Okul bulunamadı. | US-01 |

## Branşlar ve tesisler — `Organization`

| Kod | Tür | Türkçe mesaj | Hikaye |
| --- | --- | --- | --- |
| `Sport.NameTaken` | Conflict | Bu adda bir branş zaten var. | US-06 |
| `Sport.InUse` | Conflict | Bu branşa bağlı gruplar olduğu için silinemez. Arşivleyebilirsiniz. | US-06 |
| `Sport.Archived` | BusinessRule | Arşivlenmiş bir branş için yeni grup oluşturulamaz. | US-06 |
| `Sport.NotFound` | NotFound | Branş bulunamadı. | US-06 |
| `Facility.InUse` | Conflict | Bu tesisi aktif gruplar kullandığı için silinemez. Arşivleyebilirsiniz. | US-07 |
| `Facility.Archived` | BusinessRule | Arşivlenmiş bir tesis gruplara atanamaz. | US-07 |
| `Facility.NotFound` | NotFound | Tesis bulunamadı. | US-07 |

## Gruplar ve programlar — `Groups`

| Kod | Tür | Türkçe mesaj | Hikaye |
| --- | --- | --- | --- |
| `Group.NotFound` | NotFound | Grup bulunamadı. | US-09, US-12 |
| `Group.InvalidCapacity` | Validation | Kontenjan en az 1 olmalı. | US-09 |
| `Group.CapacityBelowActiveEnrollments` | BusinessRule | Kontenjan, gruptaki aktif öğrenci sayısının ({count}) altına düşürülemez. | US-11 |
| `Group.Archived` | BusinessRule | Bu grup arşivlenmiş. | US-11 |
| `Group.CoachNotFound` | BusinessRule | Seçilen antrenör bulunamadı veya aktif değil. | US-09 |
| `Schedule.SlotOverlap` | BusinessRule | Grubun program saatleri birbiriyle çakışıyor. | US-10 |
| `Schedule.CoachUnavailable` | BusinessRule | Antrenörün bu saatte başka bir grubu var: {groupName}. | US-10 |
| `Fee.PeriodInPast` | BusinessRule | Geçmiş bir dönemin aidatı değiştirilemez. | US-22 |
| `Fee.AlreadyDefined` | Conflict | Bu dönem için aidat zaten tanımlı. | US-22 |

**Uyarılar** (başarılı yanıttaki `warnings` dizisinde döner):

| Kod | Türkçe mesaj | Hikaye |
| --- | --- | --- |
| `Schedule.FacilityOverlap` | Bu saatte aynı tesiste başka bir grup var: {groupName}. Kaydedildi, ancak kontrol etmeniz önerilir. | US-10 |

## Öğrenciler ve veliler — `Students`

| Kod | Tür | Türkçe mesaj | Hikaye |
| --- | --- | --- | --- |
| `Student.NotFound` | NotFound | Öğrenci bulunamadı. | US-13 – US-16 |
| `Student.GuardianRequired` | BusinessRule | 18 yaşından küçük öğrenciler için en az bir veli girilmelidir. | US-13 |
| `Student.PhoneRequiredForAdult` | BusinessRule | Yetişkin öğrenciler için öğrencinin kendi telefonu zorunludur. | US-13 |
| `Student.EmergencyContactRequired` | Validation | Acil durum kişisi bilgileri zorunludur. | US-13 |
| `Student.AlreadyLeft` | BusinessRule | Bu öğrenci zaten ayrılmış olarak işaretli. | US-15 |
| `Student.NotLeft` | BusinessRule | Sadece ayrılmış öğrenciler yeniden aktifleştirilebilir. | US-15 |
| `Student.InvalidLeaveDate` | BusinessRule | Ayrılma tarihi, aktif kayıtların başlangıç tarihinden önce olamaz. | US-15 |
| `Guardian.NotFound` | NotFound | Veli bulunamadı. | US-13 |
| `Guardian.PhoneTaken` | Conflict | Bu telefon numarasıyla kayıtlı başka bir veli var. | US-13 |

## Kayıtlar — `Enrollments`

| Kod | Tür | Türkçe mesaj | Hikaye |
| --- | --- | --- | --- |
| `Enrollment.NotFound` | NotFound | Kayıt bulunamadı. | US-18 |
| `Enrollment.StudentNotActive` | BusinessRule | Ayrılmış bir öğrenci gruba kaydedilemez. | US-17 |
| `Enrollment.BirthYearOutOfRange` | BusinessRule | Öğrencinin doğum yılı ({year}) bu grubun aralığında ({from}–{to}) değil. | US-17 |
| `Enrollment.GroupFull` | BusinessRule | Grup kontenjanı dolu. Başka bir grup seçin veya kontenjanı artırın. | US-17 |
| `Enrollment.AlreadyEnrolled` | Conflict | Öğrenci bu gruba zaten kayıtlı. | US-17 |
| `Enrollment.ScheduleConflict` | BusinessRule | Öğrencinin bu saatte başka bir grubu var: {groupName}. | US-17 |
| `Enrollment.GroupArchived` | BusinessRule | Arşivlenmiş bir gruba kayıt yapılamaz. | US-17 |
| `Enrollment.AlreadyEnded` | BusinessRule | Bu kayıt zaten sonlandırılmış. | US-18 |
| `Enrollment.InvalidEndDate` | BusinessRule | Bitiş tarihi, başlangıç tarihinden önce olamaz. | US-18 |

## Yoklama — `Attendance`

| Kod | Tür | Türkçe mesaj | Hikaye |
| --- | --- | --- | --- |
| `Attendance.SessionInFuture` | BusinessRule | Henüz gerçekleşmemiş bir antrenman için yoklama alınamaz. | US-19 |
| `Attendance.SessionNotInSchedule` | NotFound | Bu tarih ve saatte grubun bir antrenmanı yok. | US-19 |
| `Attendance.InvalidSessionKey` | Validation | Antrenman tarihi ve saati geçerli değil. | US-19 |
| `Attendance.StudentNotEnrolled` | BusinessRule | Bu öğrenci, antrenman tarihinde gruba kayıtlı değil. | US-19 |
| `Attendance.EditWindowExpired` | Forbidden | Bu yoklama 48 saatten eski olduğu için sadece okul yöneticisi değiştirebilir. | US-20 |

## Ödemeler — `Billing`

| Kod | Tür | Türkçe mesaj | Hikaye |
| --- | --- | --- | --- |
| `Payment.NotFound` | NotFound | Ödeme bulunamadı. | US-23 |
| `Payment.InvalidAmount` | Validation | Tutar sıfırdan büyük olmalı. | US-23 |
| `Payment.EnrollmentNotActiveInPeriod` | BusinessRule | Öğrenci bu dönemde bu gruba kayıtlı değildi. | US-23 |
| `Payment.FeeNotDefined` | BusinessRule | Bu grup için bu dönemde tanımlı bir aidat yok. | US-23 |
| `Payment.AlreadyCancelled` | BusinessRule | Bu ödeme zaten iptal edilmiş. | US-23 |
| `Payment.CancelReasonRequired` | Validation | İptal gerekçesi zorunludur. | US-23 |
