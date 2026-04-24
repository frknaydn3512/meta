# AdReport SaaS

White-label Meta Ads raporlama platformu. Ajanslar kendi logoları ve renkleriyle müşterilerine otomatik aylık rapor gönderir.

## Tech Stack

| Katman | Teknoloji |
|--------|-----------|
| Backend | ASP.NET Core 10, C# |
| Frontend | React 18 + Vite + Tailwind CSS |
| Veritabanı | PostgreSQL 15 |
| ORM | Entity Framework Core |
| Auth | JWT (access + refresh token) |
| PDF | QuestPDF |
| Meta API | Meta Marketing API v19 |
| E-posta | MailKit (SMTP) |
| Background jobs | Hangfire + PostgreSQL storage |
| Container | Docker + docker-compose |

## Hızlı Başlangıç (Geliştirme)

### 1. PostgreSQL başlat

```bash
docker-compose up -d
```

### 2. Backend konfigürasyonu

`backend/AdReport.API/appsettings.json` içindeki şu alanları doldur:

```json
{
  "Jwt": { "Secret": "min-32-karakter-rastgele-key" },
  "Encryption": { "Key": "min-32-karakter-rastgele-key" },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "you@gmail.com",
    "SmtpPassword": "app-password"
  }
}
```

### 3. Backend çalıştır

```bash
cd backend/AdReport.API
dotnet run
# → http://localhost:5000
# → Swagger: http://localhost:5000/swagger
# → Hangfire: http://localhost:5000/hangfire
```

Migration'lar uygulama başlarken otomatik çalışır.

### 4. Frontend çalıştır

```bash
cd frontend
npm install
npm run dev
# → http://localhost:5173
```

Vite proxy ayarı sayesinde `/api`, `/r`, `/logos` istekleri backend'e iletilir.

## Production Deploy (Docker)

### 1. Ortam değişkenlerini ayarla

```bash
cp .env.example .env
# .env dosyasını düzenle
```

### 2. Derle ve başlat

```bash
docker-compose -f docker-compose.prod.yml --env-file .env up -d --build
```

Uygulama `http://sunucu-ip` adresinde çalışır.

## API Endpoint Özeti

### Auth
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| POST | /api/auth/register | Ajans kaydı |
| POST | /api/auth/login | Giriş |
| POST | /api/auth/refresh | Token yenile |

### Clients
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | /api/agencyclients | Müşteri listesi |
| POST | /api/agencyclients | Müşteri ekle |
| PUT | /api/agencyclients/{id} | Güncelle |
| DELETE | /api/agencyclients/{id} | Sil |

### Meta Accounts
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | /api/meta-accounts | Bağlı hesaplar |
| POST | /api/meta-accounts | Hesap bağla |
| DELETE | /api/meta-accounts/{id} | Bağlantıyı kes |

### Reports
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | /api/reports | Rapor listesi |
| POST | /api/reports | Rapor oluştur |
| GET | /api/reports/{id} | Rapor detayı |
| GET | /api/reports/{id}/download | PDF indir |
| POST | /api/reports/{id}/send-email | E-posta gönder |
| GET | /r/{slug} | Public rapor (auth gerekmez) |

### White-label
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | /api/template | Şablon getir |
| PUT | /api/template | Güncelle |
| POST | /api/template/logo | Logo yükle |

## Fiyat Planları

| Plan | Fiyat | Max Müşteri |
|------|-------|-------------|
| Starter | $49/ay | 3 |
| Agency | $129/ay | 15 |
| Scale | $299+/ay | Sınırsız |

## Otomatik Raporlama

Hangfire her ayın 1'i saat 08:00 UTC'de `RunMonthlyReportsAsync` job'ını çalıştırır:
1. Tüm bağlı Meta hesapları için önceki ayın raporu oluşturulur
2. PDF arka planda Hangfire worker'ı tarafından üretilir
3. Tamamlanınca müşteriye ajans markalı e-posta gönderilir
