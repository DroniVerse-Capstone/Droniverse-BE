using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class ClubAttemptRequestConfiguration : IEntityTypeConfiguration<ClubAttemptRequest>
{
    public void Configure(EntityTypeBuilder<ClubAttemptRequest> builder)
    {
        builder.ToTable("ClubAttemptRequest");

        builder.HasKey(cr => cr.ClubRequestID);
        builder.HasOne(cr => cr.Club)
            .WithMany(c => c.ClubRequests)
            .HasForeignKey(cr => cr.ClubID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cr => cr.Media)
            .WithMany(m => m.ClubAttemptRequests)
            .HasForeignKey(cr => cr.MediaID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.ClubRequestID).HasColumnType("char(36)");
        builder.Property(c => c.MediaID).HasColumnType("char(36)");
        builder.Property(c => c.ClubID).HasColumnType("char(36)").IsRequired();
        builder.Property(c => c.RequesterID).HasColumnType("char(36)").IsRequired();
        builder.Property(c => c.ApproverID).HasColumnType("char(36)");
        builder.Property(c => c.Status).HasConversion<int>().IsRequired();

        builder.Property(c => c.CreatedAt)
           .HasColumnType("datetime(6)")
           .IsRequired();

        builder.Property(c => c.ProcessedAt)
            .HasColumnType("datetime(6)")
            .IsRequired(false);

        builder.HasIndex(c => c.ClubID);
        builder.HasIndex(c => c.RequesterID);
        builder.HasIndex(c => c.Status);
    }
}

