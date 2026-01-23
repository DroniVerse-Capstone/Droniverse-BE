using Droniverse.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Identity.Infrastructure.Persistence.Configurations;
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Account");

        builder.HasKey(a => a.UserID);

        builder.HasOne(a => a.UserInfo)
            .WithOne(u => u.Account); // xóa account thì xóa userinfo
        
        builder.HasOne(a => a.Role)
            .WithMany(r => r.Accounts)
            .HasForeignKey(a => a.RoleID)
            .OnDelete(DeleteBehavior.Restrict); //xóa role thì không xóa account

        builder.Property(a => a.UserID)
            .HasColumnType("char(36)");

        builder.Property(a => a.RoleID)
            .HasColumnType("char(36)");

        builder.Property(a => a.Username)
            .HasMaxLength(50) //varchar(50)
            .IsRequired();
        builder.HasIndex(a => a.Username)
            .IsUnique();

        builder.Property(a => a.PasswordHash)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.Email)
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(a => a.Email)
            .IsUnique();

        builder.Property(a => a.LastLogin)
            .HasColumnType("datetime")
            .IsRequired(false);

        builder.Property(a => a.IsEmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.CreateAt)
            .HasColumnType("datetime")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(a => a.UpdatedAt)
            .HasColumnType("datetime")
            .IsRequired(false)
            .ValueGeneratedOnUpdate();

        builder.Property(a => a.Status)
            .HasColumnType("tinyint")
            .IsRequired()
            .HasConversion<byte>();
        builder.ToTable(t =>
            t.HasCheckConstraint("CK_Account_Status", "`Status` IN (0, 1, 2, 3)")
        );
    }
}
