using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class UserLabConfiguration : IEntityTypeConfiguration<UserLab>
{
    public void Configure(EntityTypeBuilder<UserLab> builder)
    {
        builder.ToTable("UserLab");

        builder.HasKey(ul => ul.UserLabID);

        builder.HasOne(um => um.Lab)
            .WithMany(m => m.UserLabs)
            .HasForeignKey(um => um.LabID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(um => um.UserLabID).HasColumnType("char(36)");
        builder.Property(um => um.UserID).HasColumnType("char(36)");
        builder.Property(um => um.LabID).HasColumnType("char(36)");
        builder.Property(um => um.Solution).HasColumnType("text");
        builder.Property(um => um.Time).HasColumnType("float").IsRequired();
        builder.Property(um => um.IsCompleted).HasColumnType("tinyint(1)").IsRequired();
        builder.Property(um => um.NumberOfStep).HasColumnType("int").IsRequired();
        builder.Property(um => um.Length).HasColumnType("float").IsRequired();
        builder.Property(um => um.FeedbackVN).HasColumnType("text");
        builder.Property(um => um.FeedbackEN).HasColumnType("text");
        builder.Property(um => um.Point).HasColumnType("decimal(10,2)").IsRequired();

    }
}
