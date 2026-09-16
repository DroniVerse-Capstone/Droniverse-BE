using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class UserAssignmentConfiguration : IEntityTypeConfiguration<UserAssignment>
{
    public void Configure(EntityTypeBuilder<UserAssignment> builder)
    {
        builder.ToTable("UserAssignment");
        builder.HasKey(e => e.UserAssignmentID);

        builder.Property(e => e.UserAssignmentID).HasColumnType("char(36)");
        builder.Property(e => e.AssignmentID).HasColumnType("char(36)");
        builder.Property(e => e.EnrollmentID).HasColumnType("char(36)");
        builder.Property(e => e.AttemptNumber).HasColumnType("int");
        builder.Property(e => e.MediaID).HasColumnType("char(36)");
        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.Status)
            .HasColumnType("tinyint")
            .HasConversion<byte>()
            .HasDefaultValue(UserAssignmentStatus.SUBMITTED)
            .IsRequired();
        builder.Property(e => e.Score).HasColumnType("int");
        builder.Property(e => e.ReviewComment).HasColumnType("text");
        builder.Property(e => e.ReviewedBy).HasColumnType("char(36)");
        builder.Property(e => e.ReviewedAt).HasColumnType("datetime");
        builder.Property(e => e.SubmittedAt).HasColumnType("datetime");

        builder.HasOne(e => e.Assignment)
            .WithMany(e => e.UserAssignments)
            .HasForeignKey(e => e.AssignmentID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Enrollment)
            .WithMany()
            .HasForeignKey(e => e.EnrollmentID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.AssignmentID, e.EnrollmentID, e.AttemptNumber })
            .IsUnique();

        builder.HasIndex(e => new { e.EnrollmentID, e.Status, e.SubmittedAt });

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_UserAssignment_AttemptNumber", "`AttemptNumber` >= 1");
            t.HasCheckConstraint("CK_UserAssignment_Status", "`Status` IN (0,1,2,3)");
            t.HasCheckConstraint("CK_UserAssignment_Score", "`Score` IS NULL OR (`Score` BETWEEN 0 AND 100)");
            t.HasCheckConstraint(
                "CK_UserAssignment_ReviewRequiredOnFinalStatus",
                "(`Status` IN (2,3) AND `ReviewedBy` IS NOT NULL AND `ReviewedAt` IS NOT NULL AND `Score` IS NOT NULL) OR (`Status` IN (0,1))");
        });
    }
}
