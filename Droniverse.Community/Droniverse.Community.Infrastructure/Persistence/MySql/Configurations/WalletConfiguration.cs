using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallet");
        builder.HasKey(w => w.WalletID);
        builder.Property(w => w.WalletID).HasColumnType("char(36)");

        builder.Property(w => w.OwnerID).HasColumnType("char(36)");
        builder.Property(w => w.BankNumber).HasMaxLength(50).IsRequired();
        builder.Property(w => w.Bank).HasMaxLength(100).IsRequired();
        builder.Property(w => w.Balance).HasColumnType("decimal(15, 2)");
        builder.Property(w => w.CreatedAt).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(w => w.UpdatedAt).HasColumnType("datetime").ValueGeneratedOnUpdate();

        builder.HasMany<WithdrawRequest>()
            .WithOne(wr => wr.Wallet)
            .HasForeignKey(wr => wr.WalletID)
            .OnDelete(DeleteBehavior.Restrict);

    }
}

