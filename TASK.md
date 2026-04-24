# TASK.md — AdReport SaaS MVP

Görevleri sırayla yap. Bir görev bitmeden bir sonrakine geçme.
Her görev sonunda çalışır durumda olmalı.

---

## Faz 1 — Proje iskeleti

- [x] 1.1 Solution oluştur: `AdReport.API`, `AdReport.Application`, `AdReport.Domain`, `AdReport.Infrastructure` projeleri
- [x] 1.2 NuGet paketlerini ekle:
  - EF Core + Npgsql
  - AutoMapper
  - FluentValidation
  - JwtBearer
  - QuestPDF
  - Hangfire + Hangfire.PostgreSql
  - MailKit
  - Serilog
- [x] 1.3 `docker-compose.yml` oluştur (PostgreSQL + pgAdmin servisleri)
- [x] 1.4 `Program.cs` temel yapısını kur (middleware sırası, DI registrations)
- [x] 1.5 Global exception handler middleware yaz (`ApiResponse<T>` wrapper ile)
- [x] 1.6 `appsettings.json` ve `.env` yapısını kur

---

## Faz 2 — Auth sistemi

- [x] 2.1 `Agency` entity'si: Id, Email, PasswordHash, Name, Plan, CreatedAt
- [x] 2.2 EF migration: Agencies tablosu
- [x] 2.3 JWT service: token üret, doğrula (access + refresh token)
- [x] 2.4 `AuthController`: POST /api/auth/register, POST /api/auth/login, POST /api/auth/refresh
- [x] 2.5 Register endpoint'i: email unique kontrolü, BCrypt hash, token dön
- [x] 2.6 Login endpoint'i: credential doğrula, token çifti dön
- [x] 2.7 `[Authorize]` middleware'i test et — korumalı endpoint ekle

---

## Faz 3 — Ajans müşteri yönetimi

- [x] 3.1 `AgencyClient` entity'si: Id, AgencyId, Name, Email, Industry, CreatedAt
- [x] 3.2 EF migration: AgencyClients tablosu
- [x] 3.3 `AgencyClientService`: CRUD işlemleri
- [x] 3.4 `AgencyClientsController`:
  - GET /api/agencyclients (ajansın kendi müşterileri)
  - POST /api/agencyclients
  - PUT /api/agencyclients/{id}
  - DELETE /api/agencyclients/{id}
- [x] 3.5 Plan limiti kontrolü: Starter max 3, Agency max 15 müşteri

---

## Faz 4 — Meta API entegrasyonu

- [x] 4.1 `MetaAccount` entity'si: Id, AgencyId, ClientId, AccountId, AccessToken, AccountName
- [x] 4.2 EF migration: MetaAccounts tablosu
- [x] 4.3 `IMetaApiClient` interface'i (Infrastructure'da implement edilecek)
- [x] 4.4 `MetaApiClient` implement et:
  - `GetAccountInsights(accountId, dateRange)` — harcama, impression, click, ROAS
  - `GetCampaigns(accountId)` — kampanya listesi
  - `ValidateToken(accessToken)` — token geçerliliği
- [x] 4.5 `MetaAccountsController`:
  - POST /api/meta-accounts (hesap bağla — accessToken + accountId al)
  - GET /api/meta-accounts (bağlı hesaplar)
  - DELETE /api/meta-accounts/{id}
- [x] 4.6 Token şifreli sakla (AES encryption)

---

## Faz 5 — Rapor oluşturma

- [x] 5.1 `Report` entity'si: Id, AgencyId, ClientId, MetaAccountId, Month, Year, Status, PdfPath, Slug, CreatedAt
- [x] 5.2 `ReportTemplate` entity'si: Id, AgencyId, LogoUrl, PrimaryColor, SecondaryColor, AgencyName, AgencyEmail, AgencyPhone
- [x] 5.3 EF migrations
- [x] 5.4 `ReportDataDto`: tüm rapor verisi (metrikler + kampanyalar)
- [x] 5.5 `ReportService.GenerateReportData()` — Meta API'den veri çek, DTO'ya dönüştür
- [x] 5.6 `PdfGeneratorService` (QuestPDF ile):
  - Header: ajans logosu + rapor dönemi
  - Metrik kartları: harcama, ROAS, tıklama, dönüşüm
  - Kampanya tablosu
  - Footer: ajans iletişim bilgisi
- [x] 5.7 Hangfire job: `GenerateReportJob(reportId)`
- [x] 5.8 `ReportsController`:
  - POST /api/reports (rapor oluştur — arka planda job tetikle)
  - GET /api/reports (ajansın raporları)
  - GET /api/reports/{id} (rapor detayı)
  - GET /api/reports/{id}/download (PDF indir)
- [x] 5.9 Public rapor link: GET /r/{slug} — şifresiz erişilebilir HTML rapor

---

## Faz 6 — White-label ayarları

- [x] 6.1 `ReportTemplateController`:
  - GET /api/template
  - PUT /api/template (logo, renk, iletişim güncelle)
- [x] 6.2 Logo upload: POST /api/template/logo (dosya kaydet, URL dön)
- [x] 6.3 PDF generator white-label'ı okusun: her rapor kendi ajansının şablonunu kullansın

---

## Faz 7 — Otomatik e-posta

- [x] 7.1 `EmailService` (MailKit): HTML e-posta gönder
- [x] 7.2 E-posta şablonu: "Raporu görüntüle" butonu olan ajans markalı HTML
- [x] 7.3 Hangfire recurring job: her ayın 1'i tüm ajanslar için rapor oluştur + e-posta gönder
- [x] 7.4 Manuel gönderim: POST /api/reports/{id}/send-email

---

## Faz 8 — Frontend (React)

- [x] 8.1 Vite + React + Tailwind kurulumu
- [x] 8.2 Axios instance (JWT interceptor ile)
- [x] 8.3 Zustand store: auth state
- [x] 8.4 Sayfalar:
  - `/login` — giriş
  - `/register` — kayıt
  - `/dashboard` — özet (müşteri sayısı, son raporlar)
  - `/clients` — müşteri listesi + ekle/sil
  - `/clients/{id}` — müşteri detayı + Meta hesap bağla
  - `/reports` — rapor listesi
  - `/reports/{id}` — rapor önizleme
  - `/settings/template` — white-label ayarları (logo, renk)
  - `/r/{slug}` — public rapor sayfası (auth gerektirmez)
- [x] 8.5 Protected route wrapper
- [x] 8.6 Toast notifications (react-hot-toast)

---

## Faz 9 — Polish & deploy

- [x] 9.1 Rate limiting (API endpoint'leri için)
- [x] 9.2 CORS ayarları
- [x] 9.3 Swagger / OpenAPI dökümanı
- [x] 9.4 Dockerfile (backend + frontend)
- [x] 9.5 docker-compose production config
- [x] 9.6 README.md — kurulum adımları

---

## MVP tamamlandı sayılır:
- Ajans kayıt olup giriş yapabiliyorsa
- Meta hesabı bağlayabiliyorsa
- Rapor oluşturup PDF indirebiliyorsa
- Müşterisine e-posta gönderebiliyorsa
- Kendi logosu/rengiyle rapor görünüyorsa
