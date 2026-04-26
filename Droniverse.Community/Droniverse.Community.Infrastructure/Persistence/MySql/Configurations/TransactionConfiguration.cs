using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transaction");
        builder.HasKey(t => t.TransactionID);
        builder.Property(t => t.TransactionID).HasColumnType("char(36)");
        
        builder.Property(t => t.WalletID).HasColumnType("char(36)");
        builder.HasOne(t => t.Wallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(t => t.WalletID)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(t => t.Amount).HasColumnType("int").IsRequired();
        builder.Property(t => t.Type)
            .HasColumnType("varchar(20)")
            .HasConversion<string>()
            .IsRequired();
        builder.Property(t => t.OrderID).HasColumnType("char(36)").IsRequired(false);
        builder.Property(t => t.WithdrawRequestID).HasColumnType("char(36)").IsRequired(false);

        builder.HasOne(t => t.WithdrawRequest)
            .WithOne(wr => wr.Transaction)
            .HasForeignKey<Transaction>(t => t.WithdrawRequestID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(t => t.ClubID).HasColumnType("char(36)").IsRequired(false);
        builder.HasOne(t => t.Club)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.ClubID)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(t => t.ReferenceID).HasColumnType("char(36)");
        builder.Property(t => t.CreatedAt).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.ToTable(t =>
        t.HasCheckConstraint("CK_Transaction_Type", "Type IN ('COMMISSION', 'WITHDRAWAL', 'REFUND')"));
        builder.ToTable(t =>
        t.HasCheckConstraint(
            "CK_Transaction_Type_Club_WithdrawRequest",
            "((Type = 'COMMISSION' AND ClubID IS NOT NULL AND WithdrawRequestID IS NULL) OR (Type IN ('WITHDRAWAL', 'REFUND') AND WithdrawRequestID IS NOT NULL AND ClubID IS NULL))"));

    }
}

