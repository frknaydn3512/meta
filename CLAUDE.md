# AdReport SaaS — Claude Code Context

## Proje nedir?
Reklam ajanslarına white-label Meta Ads rapor aracı satan bir SaaS.
Ajans kendi müşterisine kendi logosuyla otomatik rapor gönderir.
Müşteri bu aracın varlığından haberdar olmaz.

## Tech stack
- **Backend:** ASP.NET Core 8 Web API (C#)
- **Frontend:** React 18 + Vite + Tailwind CSS
- **Veritabanı:** PostgreSQL
- **ORM:** Entity Framework Core
- **Auth:** JWT (access + refresh token)
- **PDF:** QuestPDF
- **Meta API:** Meta Marketing API v19 (HTTP client)
- **E-posta:** MailKit (SMTP)
- **Container:** Docker + docker-compose

## Klasör yapısı
```
/
├── backend/
│   ├── AdReport.API/          # Controllers, Program.cs
│   ├── AdReport.Application/  # Services, DTOs, Interfaces
│   ├── AdReport.Domain/       # Entities, Enums
│   ├── AdReport.Infrastructure/ # EF, Repositories, Meta API client
│   └── AdReport.sln
├── frontend/
│   ├── src/
│   │   ├── pages/
│   │   ├── components/
│   │   ├── api/               # axios hooks
│   │   └── store/             # zustand
│   └── package.json
├── docker-compose.yml
└── TASK.md
```

## Domain modeli (özet)
```
Agency          → ajans (bizim müşterimiz)
AgencyClient    → ajansın müşterisi (ayakkabı markası vs.)
MetaAccount     → bağlı Meta reklam hesabı
Report          → oluşturulmuş rapor
ReportTemplate  → ajansa ait rapor şablonu (logo, renk, layout)
```

## Planlama kuralları
- Her entity için önce migration, sonra repository, sonra service, sonra controller
- Controller'lar sadece HTTP katmanı — iş mantığı Application'da
- Meta API çağrıları Infrastructure'da, interface Application'da
- Tüm servisler interface üzerinden inject edilir
- Exception handling: global middleware ile, controller'da try-catch yok
- Tüm response'lar ApiResponse<T> wrapper ile döner

## Kodlama kuralları
- Türkçe yorum yazma, kod İngilizce
- Her public method XML doc comment alır
- Async/await everywhere — senkron IO yasak
- EF query'leri AsNoTracking() ile (read), tracking sadece write işlemlerinde
- DTO'lar için AutoMapper kullan
- Validation: FluentValidation

## Önemli notlar
- Meta API için long-lived access token gerekir (ajans bağlama akışında OAuth)
- PDF oluşturma CPU-intensive — background job kuyruğuna al (Hangfire)
- White-label: her rapor linki /r/{reportSlug} formatında, slug ajansa özel subdomain'e yönlendirilebilir
- Fiyat planları: Starter ($49, 3 hesap), Agency ($129, 15 hesap), Scale ($299+)

## Çevre değişkenleri (.env.example)
```
DATABASE_URL=postgresql://postgres:pass@localhost:5432/adreport
JWT_SECRET=
JWT_EXPIRY_MINUTES=60
REFRESH_TOKEN_EXPIRY_DAYS=30
META_APP_ID=
META_APP_SECRET=
SMTP_HOST=
SMTP_PORT=587
SMTP_USER=
SMTP_PASS=
QUESTPDF_LICENSE=Community
```
