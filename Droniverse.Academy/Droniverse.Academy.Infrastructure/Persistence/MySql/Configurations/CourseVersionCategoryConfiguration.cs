using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class CourseVersionCategoryConfiguration : IEntityTypeConfiguration<CourseVersionCategory>
{
    public void Configure(EntityTypeBuilder<CourseVersionCategory> builder)
    {
        builder.ToTable("CourseVersionCategory");
        builder.HasKey(cvc => new {cvc.CourseVersionID, cvc.CategoryID});

        builder.HasOne(cvc => cvc.CourseVersion)
            .WithMany(cv => cv.CourseVersionCategories)
            .HasForeignKey(cvc => cvc.CourseVersionID)
            .OnDelete(DeleteBehavior.Restrict);
        //builder.HasOne(cvc => cvc.Category)
        //    .WithMany(cv => cv.CourseVersionCategories)
        //    .HasForeignKey(cvc => cvc.CategoryID)
        //    .OnDelete(DeleteBehavior.Restrict); 


    }
}

