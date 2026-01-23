using Droniverse.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Droniverse.Identity.Infrastructure.Persistence.Configurations;
public class UserInfoConfiguration : IEntityTypeConfiguration<UserInfo>
{
    public void Configure(EntityTypeBuilder<UserInfo> builder)
    {
        builder.ToTable("UserInfo");
        builder.HasKey(u => u.UserID);

        builder.HasOne(u => u.Account)
            .WithOne(a => a.UserInfo)
            .HasForeignKey<UserInfo>(u => u.UserID)
            .OnDelete(DeleteBehavior.Cascade); // xóa account thì xóa userinfo

        builder.Property(u => u.UserID)
            .HasColumnType("char(36)");
        builder.Property(u => u.FirstName)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(u => u.LastName)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(u => u.ImageUrl)
            .HasColumnType("text")
            .IsRequired(false);
        builder.Property(u => u.Phone)
            .HasMaxLength(12)
            .IsRequired(false);
        builder.Property(u => u.Gender)
            .HasColumnType("tinyint")
            .IsRequired();
        builder.Property(u => u.DateOfBirth)
            .HasColumnType("date")
            .IsRequired(false);
        builder.Property(u => u.Bio)
            .HasColumnType("text")
            .IsRequired(false);
    }
}
