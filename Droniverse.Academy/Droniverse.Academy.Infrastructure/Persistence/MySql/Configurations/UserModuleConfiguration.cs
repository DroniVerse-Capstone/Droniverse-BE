using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class UserModuleConfiguration : IEntityTypeConfiguration<UserModule>
{
    public void Configure(EntityTypeBuilder<UserModule> builder)
    {
        builder.ToTable("UserModule");

        builder.HasKey(um => new { um.ModuleID, um.UserID});

        builder.HasOne(um => um.Module)
            .WithMany(m => m.UserModules)
            .HasForeignKey(um => um.ModuleID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(um => um.UserID).HasColumnType("char(36)");
        builder.Property(um => um.ModuleID).HasColumnType("char(36)");
        builder.Property(um => um.EnrollDate).HasColumnType("datetime");
        builder.Property(um => um.CompleteDate).HasColumnType("datetime");
        builder.Property(um => um.Progress).HasColumnType("float");
        builder.Property(um => um.IsCompleted).HasColumnType("tinyint(1)").IsRequired();
        
        builder.ToTable(t => t.HasCheckConstraint("CK_UserModule_Progress", "`Progress` >= 0 AND `Progress` <= 100"));

    }
}
