using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations
{
    public class ClubCreationRequestCategoryConfiguration : IEntityTypeConfiguration<ClubCreationRequestCategory>
    {
        public void Configure(EntityTypeBuilder<ClubCreationRequestCategory> builder)
        {
            builder.ToTable("ClubCreationRequestCategory");

            builder.HasKey(x => new { x.ClubCreationRequestID, x.CategoryID });

            builder.HasOne(x => x.ClubCreationRequest)
                .WithMany(x => x.Categories)
                .HasForeignKey(x => x.ClubCreationRequestID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.ClubCreationRequests)
                .HasForeignKey(x => x.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
