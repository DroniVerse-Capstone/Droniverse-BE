using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class ClubConfiguration : IEntityTypeConfiguration<Club>
{
    public void Configure(EntityTypeBuilder<Club> builder)
    {
        builder.ToTable("Club");

        builder.HasKey(c => c.ClubID);
        builder.Property(c => c.ClubID).HasColumnType("char(36)");

        builder.HasOne(c => c.ClubPolicy)
            .WithOne(cp => cp.Club)
            .HasForeignKey<ClubPolicy>(cp => cp.ClubID)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.ClubCourses)
            .WithOne(cc => cc.Club);
        builder.HasMany(c => c.Participations)
            .WithOne(p => p.Club);
        builder.HasMany(c => c.Competitions)
            .WithOne(cp => cp.Club);
        builder.HasMany(c => c.ClubRequests)
            .WithOne(cr => cr.Club);
        //builder.HasMany(c => c.Enrollments)
        //    .WithOne(e => e.Club);

        builder.Property(c => c.CreatedBy).HasColumnType("char(36)").IsRequired();
        builder.Property(c => c.NameVN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.NameEN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.Description).HasColumnType("varchar(255)");
        builder.Property(c => c.ClubCode).HasColumnType("char(6)").IsRequired();
        builder.Property(c => c.ImageUrl).HasColumnType("varchar(255)");
        builder.Property(c => c.Status).HasColumnType("tinyint").HasConversion<byte>().IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("CK_Club_Status", "`Status` IN (0, 1, 2, 3)"));
        builder.Property(c => c.IsPublic).HasColumnType("tinyint(1)").IsRequired();
        builder.Property(c => c.LimitParticipation).HasColumnType("int");
        builder.Property(c => c.LimitClubManagers).HasColumnType("int");
        builder.Property(c => c.CreatedAt).HasColumnType("datetime").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnType("datetime");
    }
}

