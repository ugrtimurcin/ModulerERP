using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModulerERP.SystemCore.Domain.Entities;

namespace ModulerERP.SystemCore.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Phone)
            .HasMaxLength(20);
        
        builder.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
        
        builder.HasMany(e => e.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(e => e.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.RefreshToken)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(e => e.IpAddress)
            .HasMaxLength(45);
        
        builder.Property(e => e.DeviceInfo)
            .HasMaxLength(500);
        
        builder.HasIndex(e => e.RefreshToken);
        builder.HasIndex(e => new { e.UserId, e.IsRevoked });
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.Description)
            .HasMaxLength(500);

        // Map Permissions list to JSONB column
        builder.Property(e => e.Permissions)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => DeserializePermissions(v),
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (c1, c2) => System.Text.Json.JsonSerializer.Serialize(c1, (System.Text.Json.JsonSerializerOptions?)null) == System.Text.Json.JsonSerializer.Serialize(c2, (System.Text.Json.JsonSerializerOptions?)null),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                )
            );
        
        builder.HasIndex(e => new { e.TenantId, e.Name }).IsUnique();
        
        builder.HasOne(e => e.ParentRole)
            .WithMany(r => r.ChildRoles)
            .HasForeignKey(e => e.ParentRoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static List<string> DeserializePermissions(string v)
    {
        if (string.IsNullOrEmpty(v)) return new List<string>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
        
        builder.HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
