using Microsoft.EntityFrameworkCore;
using AdReport.Domain.Entities;
using AdReport.Domain.Enums;

namespace AdReport.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Agency> Agencies { get; set; }
    public DbSet<AgencyClient> AgencyClients { get; set; }
    public DbSet<MetaAccount> MetaAccounts { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportTemplate> ReportTemplates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Agency>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Plan).HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasMany(e => e.Clients)
                  .WithOne(e => e.Agency)
                  .HasForeignKey(e => e.AgencyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AgencyClient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Industry).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => new { e.AgencyId, e.Email }).IsUnique();

            entity.HasMany(e => e.MetaAccounts)
                  .WithOne(e => e.Client)
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MetaAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EncryptedAccessToken).IsRequired();
            entity.Property(e => e.AccountName).HasMaxLength(255);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => new { e.AgencyId, e.AccountId }).IsUnique();
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Agency)
                  .WithMany(e => e.Reports)
                  .HasForeignKey(e => e.AgencyId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Client)
                  .WithMany()
                  .HasForeignKey(e => e.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.MetaAccount)
                  .WithMany()
                  .HasForeignKey(e => e.MetaAccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReportTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AgencyId).IsUnique();
            entity.Property(e => e.PrimaryColor).HasMaxLength(20);
            entity.Property(e => e.SecondaryColor).HasMaxLength(20);
            entity.Property(e => e.AgencyDisplayName).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Agency)
                  .WithOne(e => e.Template)
                  .HasForeignKey<ReportTemplate>(e => e.AgencyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}