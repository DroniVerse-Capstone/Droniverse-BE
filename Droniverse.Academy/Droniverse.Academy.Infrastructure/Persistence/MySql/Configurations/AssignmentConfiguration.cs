using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignment");
        builder.HasKey(e => e.AssignmentID);

        builder.Property(e => e.AssignmentID).HasColumnType("char(36)");
        builder.Property(e => e.TitleEN).HasColumnType("text");
        builder.Property(e => e.TitleVN).HasColumnType("text");
        builder.Property(e => e.DescriptionEN).HasColumnType("text");
        builder.Property(e => e.DescriptionVN).HasColumnType("text");
        builder.Property(e => e.Requirement).HasColumnType("text");
        builder.Property(e => e.CreateBy).HasColumnType("char(36)");
        builder.Property(e => e.UpdateBy).HasColumnType("char(36)");
        builder.Property(e => e.CreateAt).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(e => e.UpdateAt).HasColumnType("datetime").ValueGeneratedOnUpdate();
        builder.Property(e => e.EstimatedTime).HasColumnType("int");

        builder.HasMany(e => e.UserAssignments)
            .WithOne(e => e.Assignment)
            .HasForeignKey(e => e.AssignmentID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Assignment_EstimatedTime", "`EstimatedTime` >= 0");
        });
    }
}
