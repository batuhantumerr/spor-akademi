# Spor Akademi

Spor okulları için çok kiracılı (multi-tenant) bir yönetim platformu: okullar, branşlar, tesisler, gruplar, öğrenciler, kayıtlar, yoklama ve aidat takibi.

Bu proje, uçtan uca bir full stack geliştirme pratiği olarak geliştirilmektedir: gereksinim analizi, domain modelleme, mimari kararlar, test, CI/CD ve yapay zeka destekli geliştirme.

> **Durum:** Faz 0 — Temel. Planlama dokümanları tamamlandı; uygulama iskeleti hazırlanıyor. İlerleme için [yol haritasına](docs/roadmap.md) bakın.

## Teknolojiler

- **Backend:** .NET 10, ASP.NET Core, EF Core, PostgreSQL
- **Frontend:** Angular (admin ve antrenör uygulaması); faz 3'te React (veli uygulaması)
- **Mimari:** Clean Architecture katmanlarıyla modüler monolit
- **Altyapı:** Docker, GitHub Actions

## Dokümantasyon

| Doküman | İçerik |
| --- | --- |
| [Ürün vizyonu](docs/product/vision.md) | Problem, roller, MVP kapsamı |
| [Kullanıcı hikayeleri](docs/product/user-stories.md) | Kabul kriterleriyle MVP hikayeleri |
| [Fonksiyonel olmayan gereksinimler](docs/product/nfr.md) | Güvenlik, KVKK, performans, güvenilirlik hedefleri |
| [Domain modeli](docs/domain/domain-model.md) | Ortak dil, aggregate'lar, kurallar |
| [Mimariye genel bakış](docs/architecture/overview.md) | C4 diyagramları, modüller, istek akışları |
| [API](docs/api/overview.md) | API kuralları ve endpoint'ler |
| [Mimari kararlar (ADR)](docs/adr/) | Her önemli kararın gerekçesi |
| [Karar günlüğü](docs/decisions/decision-log.md) | Tüm kararların dizini |
| [Yol haritası](docs/roadmap.md) | Fazlar ve ilerleme |
| [Katkı kuralları](CONTRIBUTING.md) | Branch, commit, PR ve inceleme kuralları |

## Yerelde çalıştırma

> P0-23 – P0-25 tamamlandığında eklenecek.
