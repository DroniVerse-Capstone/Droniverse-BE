using Droniverse.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Identity.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.NotificationID);

        builder.Property(n => n.NotificationID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(n => n.UserID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(n => n.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(n => n.Type)
            .HasColumnType("varchar(50)")
            .IsRequired()
            .HasConversion<string>();

        builder.Property(n => n.Status)
            .HasColumnType("varchar(50)")
            .IsRequired()
            .HasConversion<string>();

        builder.Property(n => n.CreatedAt)
            .HasColumnType("datetime(6)")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(n => n.SentAt)
            .HasColumnType("datetime(6)")
            .IsRequired(false);

        builder.Property(n => n.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(n => n.RelatedEntityID)
            .HasMaxLength(36)
            .IsRequired(false);

        builder.Property(n => n.ErrorMessage)
            .HasMaxLength(500)
            .IsRequired(false);

        // Foreign Key
        builder.HasOne(n => n.Account)
            .WithMany()
            .HasForeignKey(n => n.UserID)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(n => n.UserID)
            .HasDatabaseName("idx_user");

        builder.HasIndex(n => n.Status)
            .HasDatabaseName("idx_status");

        builder.HasIndex(n => new { n.Status, n.CreatedAt })
            .HasDatabaseName("idx_status_created");

        builder.HasIndex(n => n.CreatedAt)
            .HasDatabaseName("idx_created");
    }
}
