using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;
using FluentValidation.AspNetCore;
using Serilog;
using Hangfire;
using Hangfire.PostgreSql;
using AdReport.Infrastructure.Data;
using AdReport.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 9.3 — Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AdReport API",
        Version = "v1",
        Description = "White-label Meta Ads reporting SaaS"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("AdReport.API")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddAuthorization();

// 9.1 — Rate limiting
builder.Services.AddRateLimiter(options =>
{
    // Auth endpoints: 10 requests / minute per IP
    options.AddFixedWindowLimiter("auth", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 10;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // General API: 120 requests / minute per IP
    options.AddFixedWindowLimiter("api", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 120;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.RejectionStatusCode = 429;
});

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

// Register services
builder.Services.AddScoped<AdReport.Application.Interfaces.IJwtService, AdReport.Infrastructure.Services.JwtService>();
builder.Services.AddScoped<AdReport.Application.Interfaces.IAuthService, AdReport.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<AdReport.Application.Interfaces.IAgencyClientService, AdReport.Infrastructure.Services.AgencyClientService>();
builder.Services.AddSingleton<AdReport.Application.Interfaces.IEncryptionService, AdReport.Infrastructure.Services.EncryptionService>();
builder.Services.AddScoped<AdReport.Application.Interfaces.IMetaAccountService, AdReport.Infrastructure.Services.MetaAccountService>();
builder.Services.AddHttpClient<AdReport.Application.Interfaces.IMetaApiClient, AdReport.Infrastructure.Services.MetaApiClient>();
builder.Services.AddScoped<AdReport.Application.Interfaces.IPdfGeneratorService, AdReport.Infrastructure.Services.PdfGeneratorService>();
builder.Services.AddScoped<AdReport.Application.Interfaces.IReportService, AdReport.Infrastructure.Services.ReportService>();
builder.Services.AddScoped<AdReport.Application.Interfaces.IReportTemplateService, AdReport.Infrastructure.Services.ReportTemplateService>();
builder.Services.AddScoped<AdReport.Application.Interfaces.IEmailService, AdReport.Infrastructure.Services.EmailService>();
builder.Services.AddScoped<AdReport.Application.Interfaces.IReportEmailService, AdReport.Infrastructure.Services.ReportEmailService>();

// 9.2 — CORS: dev allows all, prod restricts to frontend origin
var frontendUrl = builder.Configuration["App:BaseUrl"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dev", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    options.AddPolicy("Prod", policy => policy
        .WithOrigins(frontendUrl)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

var app = builder.Build();

// Auto-apply pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AdReport API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors(app.Environment.IsDevelopment() ? "Dev" : "Prod");

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStaticFiles();

app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new Hangfire.DashboardOptions
{
    Authorization = [new AdReport.API.Middleware.HangfireDashboardAuthFilter()]
});

// Register recurring job: 1st of every month at 08:00 UTC
RecurringJob.AddOrUpdate<AdReport.Application.Interfaces.IReportEmailService>(
    "monthly-reports",
    svc => svc.RunMonthlyReportsAsync(),
    "0 8 1 * *");

app.MapControllers().RequireRateLimiting("api");

app.Run();
