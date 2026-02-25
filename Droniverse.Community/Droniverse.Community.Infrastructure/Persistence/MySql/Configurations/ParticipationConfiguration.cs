using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class ParticipationConfiguration : IEntityTypeConfiguration<Participation>
{
    public void Configure(EntityTypeBuilder<Participation> builder)
    {
        builder.ToTable("Participation");

        builder.HasKey(p => p.ParticipationID);
        builder.Property(p => p.ParticipationID).HasColumnType("char(36)");
        builder.HasOne(p => p.Club)
            .WithMany(c => c.Participations)
            .HasForeignKey(p => p.ClubID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.UserID).HasColumnType("char(36)").IsRequired();
        builder.Property(p => p.ApproverID).HasColumnType("char(36)").IsRequired();
        builder.Property(p => p.ClubID).HasColumnType("char(36)").IsRequired();
        builder.Property(p => p.Status).HasColumnType("bit").HasConversion<byte>().IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("CK_Participation_Status", "`Status` IN (0, 1, 2)"));
        builder.Property(p => p.JoinDate).HasColumnType("datetime").ValueGeneratedOnAdd();
    }
}

