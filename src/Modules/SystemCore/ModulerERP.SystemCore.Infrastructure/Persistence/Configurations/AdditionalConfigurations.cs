using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.SystemCore.Domain.Entities;
using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.SystemCore.Infrastructure.Persistence.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.Code).IsUnique();
    }
}

public class TranslationConfiguration : IEntityTypeConfiguration<Translation>
{
    public void Configure(EntityTypeBuilder<Translation> builder)
    {
        builder.ToTable("Translations");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.LanguageCode).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Key).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Value).IsRequired();
        builder.HasIndex(e => new { e.LanguageCode, e.Key }).IsUnique();
    }
}

public class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ToTable("MediaFiles");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.StoragePath).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.Extension).HasMaxLength(10);
        builder.Property(e => e.MimeType).HasMaxLength(100);
        builder.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
    }
}

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Street).HasMaxLength(500);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.State).HasMaxLength(100);
        builder.Property(e => e.Country).HasMaxLength(100);
        builder.Property(e => e.PostalCode).HasMaxLength(20);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
    }
}

public class QueuedJobConfiguration : IEntityTypeConfiguration<QueuedJob>
{
    public void Configure(EntityTypeBuilder<QueuedJob> builder)
    {
        builder.ToTable("QueuedJobs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.JobType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Payload).HasColumnType("jsonb");
        builder.HasIndex(e => new { e.Status, e.NextTryAt });
    }
}

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("ApiKeys");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.KeyHash).IsRequired().HasMaxLength(256);
        builder.Property(e => e.KeyPrefix).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Permissions).HasColumnType("jsonb");
        builder.HasIndex(e => e.KeyPrefix);
    }
}

public class WebhookConfiguration : IEntityTypeConfiguration<Webhook>
{
    public void Configure(EntityTypeBuilder<Webhook> builder)
    {
        builder.ToTable("Webhooks");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Url).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.Secret).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Events).HasColumnType("jsonb");
    }
}

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Subject).HasMaxLength(200);
        builder.Property(e => e.BodyHtml).IsRequired();
        builder.Property(e => e.LanguageCode).IsRequired().HasMaxLength(10);
        builder.HasIndex(e => new { e.TenantId, e.Code, e.LanguageCode }).IsUnique();
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Content).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.Link).HasMaxLength(500);
        builder.HasIndex(e => new { e.UserId, e.IsRead });
        
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.OldValues).HasColumnType("jsonb");
        builder.Property(e => e.NewValues).HasColumnType("jsonb");
        builder.HasIndex(e => new { e.EntityName, e.EntityId });
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.TraceId);
    }
}

public class UserLoginHistoryConfiguration : IEntityTypeConfiguration<UserLoginHistory>
{
    public void Configure(EntityTypeBuilder<UserLoginHistory> builder)
    {
        builder.ToTable("UserLoginHistory");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.IpAddress).HasMaxLength(45);
        builder.Property(e => e.UserAgent).HasMaxLength(500);
        builder.Property(e => e.FailureReason).HasMaxLength(200);
        builder.HasIndex(e => new { e.UserId, e.LoginTime });
        
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
