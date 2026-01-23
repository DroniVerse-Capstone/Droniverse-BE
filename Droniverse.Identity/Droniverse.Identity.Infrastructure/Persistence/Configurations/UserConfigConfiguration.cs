using Droniverse.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Identity.Infrastructure.Persistence.Configurations;
public class UserConfigConfiguration : IEntityTypeConfiguration<UserConfig>
{
    public void Configure(EntityTypeBuilder<UserConfig> builder)
    {
        builder.ToTable("UserConfig");
        builder.HasKey(uc => uc.UserID);
        builder.HasOne(uc => uc.UserInfo)
            .WithOne(ui => ui.UserConfig)
            .HasForeignKey<UserConfig>(uc => uc.UserID)
            .OnDelete(DeleteBehavior.Cascade); // xóa userinfo thì xóa userconfig

        builder.Property(uc => uc.UserID)
            .HasColumnType("char(36)");

        builder.Property(uc => uc.Language)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<byte>();
        builder.ToTable(t =>
            t.HasCheckConstraint("CK_UserConfig_Language", "`Language` IN(0, 1, 2)")
        );
    }

}
