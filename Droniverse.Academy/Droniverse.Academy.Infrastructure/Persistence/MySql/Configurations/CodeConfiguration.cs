using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class CodeConfiguration : IEntityTypeConfiguration<Code>
{
    public void Configure(EntityTypeBuilder<Code> builder)
    {
        builder.ToTable("Code");

        builder.HasKey(c => c.CodeID);
        builder.Property(c => c.CodeID).HasColumnType("char(36)");
        builder.HasOne(c => c.CourseVersion)
            .WithMany(cv => cv.Codes)
            .HasForeignKey(c => c.CourseVersionID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(c => c.CodeUsages)
            .WithOne(cu => cu.Code);

        builder.Property(c => c.CourseVersionID).HasColumnType("char(36)").IsRequired();
        builder.Property(c => c.ExpireDate).HasColumnType("datetime").IsRequired();
        builder.Property(c => c.Status).HasColumnType("tinyint").HasConversion<int>().IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("CK_Code_Status", "`Status` IN (0, 1)"));
    }
}

