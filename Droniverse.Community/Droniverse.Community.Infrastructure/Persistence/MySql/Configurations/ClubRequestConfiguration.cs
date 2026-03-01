using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class ClubRequestConfiguration : IEntityTypeConfiguration<ClubRequest>
{
    public void Configure(EntityTypeBuilder<ClubRequest> builder)
    {
        builder.ToTable("ClubRequest");

        builder.HasKey(cr => cr.ClubRequestID);
        builder.HasOne(cr => cr.Club)
            .WithMany(c => c.ClubRequests)
            .HasForeignKey(cr => cr.ClubID)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(c => c.ClubRequestID).HasColumnType("char(36)");
        builder.Property(c => c.ClubID).HasColumnType("char(36)").IsRequired();
        builder.Property(c => c.RequesterID).HasColumnType("char(36)").IsRequired();
        builder.Property(c => c.ApproverID).HasColumnType("char(36)");
        
    }
}

