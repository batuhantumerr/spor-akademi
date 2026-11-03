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

Repoyu klonladıktan sonra, commit mesajı hook'unu etkinleştir:

```bash
git config core.hooksPath .githooks
```

### Ön koşullar

- [Docker](https://docs.docker.com/get-docker/) (Docker Compose dahil)
- .NET 10 SDK: sürüm `global.json`'da sabit
- Node.js 26: sürüm `web/admin/.nvmrc`'de sabit; [fnm](https://github.com/Schniz/fnm) ile kurulması önerilir

### Yerel ortam: PostgreSQL ve Seq

İlk seferde `.env` dosyasını oluştur ve şifreleri değiştir. `.env` repoya girmez.

```bash
cp .env.example .env
```

Bilgisayarında 5432 portu başka bir PostgreSQL tarafından kullanılıyorsa, `.env`'de `POSTGRES_HOST_PORT`'u değiştir (ör. `5433`).

Başlatmak için tek komut yeterli. Servisler sağlıklı olana kadar bekler:

```bash
docker compose up -d --wait
```

| Servis | Adres | Giriş |
| --- | --- | --- |
| PostgreSQL | `127.0.0.1:<POSTGRES_HOST_PORT>`, veritabanı `spor_akademi` | Aşağıdaki kullanıcılar |
| Seq (loglar) | <http://localhost:8081> | `admin` / `.env`'deki `SEQ_ADMIN_PASSWORD` |
| Seq log alımı | `http://localhost:5341` | — |

Portlar sadece `127.0.0.1`'e açıktır; ağdaki başka cihazlar erişemez.

```bash
docker compose down        # durdur (veriler kalır)
docker compose down -v     # sıfırla: TÜM yerel veritabanı ve log verisi silinir
docker compose up -d --wait
```

`.env`'deki şifreler ve veritabanı kullanıcıları sadece ilk başlatmada uygulanır. Şifre değiştirdiysen ortamı sıfırla.

### Veritabanı kullanıcıları

| Kullanıcı | Ne için | Şemayı değiştirebilir mi? |
| --- | --- | --- |
| `postgres` | Sadece kurulum ve bakım; uygulama bu kullanıcıyla bağlanmaz | Evet |
| `spor_akademi_migrator` | Migration'lar; veritabanının sahibi | Evet |
| `spor_akademi_app` | API'nin çalışma anı bağlantısı; veri okur ve yazar | Hayır (NFR-SEC-10) |

> **Migration'lar her zaman `spor_akademi_migrator` ile çalıştırılır.** Uygulama kullanıcısının tablolara erişimi, sadece migrator'ın oluşturduğu nesneler için otomatik olarak verilir (varsayılan yetkiler). Migration `postgres` kullanıcısıyla çalıştırılırsa, uygulama oluşan tablolara erişemez.

Kullanıcılar ve yetkiler `docker/postgres/initdb/10-create-roles.sh` ile kurulur.

### Migration'lar

`scripts/ef.sh`, `dotnet ef`'i migrator kullanıcısıyla çalıştırır. Connection string'i `.env`'den üretir ve hiçbir yere yazdırmaz. Git Bash veya başka bir bash'te çalışır.

```bash
dotnet tool restore                           # ilk seferde: dotnet-ef'i dotnet-tools.json'daki sürümle kurar
scripts/ef.sh --check-env                     # .env'deki şifreler kurala uyuyor mu (sadece harf ve rakam)
scripts/ef.sh database update                 # bekleyen migration'ları uygula
scripts/ef.sh migrations add <Ad>             # yeni migration
scripts/ef.sh migrations script --idempotent  # PR'da incelenecek SQL (CONTRIBUTING §7)
```

Production'da migration'ların nasıl uygulandığı ADR-0011'de anlatılır.

### API

`scripts/run-api.sh`, API'yi `http://localhost:5073` adresinde `spor_akademi_app` kullanıcısıyla çalıştırır. Connection string'i `.env`'den üretir ve hiçbir yere yazdırmaz. Bağlantı bilgisi yoksa API başlamaz.

```bash
scripts/run-api.sh
curl http://localhost:5073/health/live    # süreç ayakta: 200 Healthy
curl http://localhost:5073/health/ready   # veritabanına erişilebiliyor: 200 Healthy, değilse 503 Unhealthy
```

## Lisans

Copyright © 2026. Tüm hakları saklıdır.

Bu repo, geliştirme süresince inceleme ve portföy amacıyla herkese açıktır. Herhangi bir açık kaynak lisansı verilmemiştir; kodun ve dokümanların izinsiz kopyalanması, değiştirilmesi, dağıtılması veya ticari amaçla kullanılması yasaktır.
