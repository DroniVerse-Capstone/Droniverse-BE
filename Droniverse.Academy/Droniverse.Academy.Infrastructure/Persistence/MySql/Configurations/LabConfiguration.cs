using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class LabConfiguration : IEntityTypeConfiguration<Lab>
{
    public void Configure(EntityTypeBuilder<Lab> builder)
    {
        builder.ToTable("Lab");
        builder.HasKey(e => e.LabID);

        builder.HasOne(e => e.Lesson)
            .WithOne(c => c.Lab)
            .HasForeignKey<Lab>(e => e.LessonID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.UserLabs)
            .WithOne(c => c.Lab);
        builder.HasMany(e => e.Reports)
            .WithOne(c => c.Lab);

        builder.Property(e => e.NameVN).HasColumnType("varchar(255)");
        builder.Property(e => e.NameEN).HasColumnType("varchar(255)");
        builder.Property(e => e.DescriptionVN).HasColumnType("text");
        builder.Property(e => e.DescriptionEN).HasColumnType("text");
        builder.Property(e => e.Level).HasColumnType("tinyint").HasConversion<byte>();
        builder.Property(e => e.Status).HasColumnType("tinyint").HasConversion<byte>();
        builder.Property(e => e.CreateBy).HasColumnType("char(36)");
        builder.Property(e => e.UpdateBy).HasColumnType("char(36)");
        builder.Property(e => e.CreateAt).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(e => e.UpdateAt).HasColumnType("datetime").ValueGeneratedOnUpdate();
        builder.Property(e => e.Type).HasColumnType("varchar(50)").HasConversion<string>();
        builder.ToTable(t => t.HasCheckConstraint("CK_Lab_Type", "`Type` IN ('LEARNING', 'COMPETITION')"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Lab_Level", "`Level` IN (0, 1, 2)"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Lab_Status", "`Status` IN (0, 1, 2, 3, 4)"));


    }
}
