using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("Module");

        builder.HasKey(m => m.ModuleID);
        builder.HasOne(m => m.CourseVersion)
            .WithMany(c => c.Modules)
            .HasForeignKey(m => m.CourseID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(m => m.UserModules)
            .WithOne(um => um.Module);
        builder.HasMany(m => m.Lessons)
            .WithOne(l => l.Module);

        builder.Property(m => m.ModuleID).HasColumnType("char(36)");
        builder.Property(m => m.CourseID).HasColumnType("char(36)");
        builder.Property(m => m.TitleVN).HasMaxLength(255);
        builder.Property(m => m.TitleEN).HasMaxLength(255);
        builder.Property(m => m.ModuleNumber).HasColumnType("int");
        builder.Property(m => m.CreateAt).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(m => m.UpdateAt).HasColumnType("datetime").ValueGeneratedOnUpdate();

    }
}
