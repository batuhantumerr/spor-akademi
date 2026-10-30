# Faz <n> Geçişi — <faz adı>

> Repodaki yeri: `docs/phases/phase-<n>-gate.md` (`docs/phases/phase-gate-template.md` şablonundan kopyalanır)
> Hazırlayan: Batuhan Tümer · Hazırlanma tarihi: <tarih> · Durum: `Taslak` | `Onaylandı` | `Değişiklik istendi`

Bölüm 7 sahip tarafından imzalanmadan bir sonraki faz başlamaz.

## 1. Kapsam incelemesi

`docs/roadmap.md`'deki fazın her maddesi:

| Madde | Başlık | Sonuç | Kanıt |
| --- | --- | --- | --- |
| P<n>-01 | | Tamamlandı / Taşındı / İptal edildi | PR #, test çalıştırması, bağlantı |

Tamamlanmayan maddeler ve nedenleri:

| Madde | Taşındığı yer | Gerekçe |
| --- | --- | --- |

## 2. Çıkış kriterleri

| Çıkış kriteri (roadmap'ten) | Karşılandı mı? | Kanıt |
| --- | --- | --- |

## 3. Kalite özeti

| Ölçüt | Değer |
| --- | --- |
| Testler (birim / entegrasyon / mimari / e2e) | |
| CI süresi | |
| Açık hatalar (önem derecesine göre) | |
| Bilinen teknik borç (bağlı issue'lar) | |
| Bağımlılık güvenlik açıkları | |
| Geri yükleme tatbikatı sonucu (RTO) | |

## 4. Karar incelemesi

`docs/decisions/decision-log.md`'deki her `Accepted` karar burada listelenir. Sahip her biri için seçim yapar.

| ID | Karar | Hâlâ doğru mu? Bu fazdan notlar | Sahibin seçimi: Koru / Değiştir / Kaldır |
| --- | --- | --- | --- |

Sonraki faz için önerilen yeni kararlar:

| Öneri | Neden | Değerlendirilen seçenekler | Sahibin seçimi: Kabul / Ret / Ertele |
| --- | --- | --- | --- |

## 5. Retrospektif

- **Ne iyi gitti:**
- **Ne zorladı:**
- **Sonraki fazda süreçte neyi değiştiriyoruz:**
- **Yapay zeka destekli geliştirme nasıl gitti:** hangi promptlar, kurallar veya iş akışları işe yaradı; ne ters gitti; `CLAUDE.md`'ye ne ekleniyor.

## 6. Öğrenme özeti

Bu fazda üretilen mülakat kartları (`docs/learning/journal.md`'den):

| Kavram | Projede nerede kullanıldı | Tek cümlede açıklaması |
| --- | --- | --- |

## 7. Sonraki faz planı ve onay

- [ ] Faz <n+1>'in detaylı görevleri `docs/roadmap.md`'ye yazıldı
- [ ] Faz <n+1> için GitHub issue'ları ve milestone oluşturuldu
- [ ] Bölüm 4'teki karar değişiklikleri uygulandı (yeni ADR'lar, karar günlüğü güncellendi)
- [ ] Sürüm etiketlendi: `v<x.y.z>`

**Onay**

Onaylayan: Batuhan Tümer · Tarih: ____________

Yorumlar:
