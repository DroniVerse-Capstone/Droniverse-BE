using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;

public class WithdrawRequestConfiguration : IEntityTypeConfiguration<WithdrawRequest>
{
    public void Configure(EntityTypeBuilder<WithdrawRequest> builder)
    {
        builder.ToTable("WithdrawRequest");

        builder.HasKey(w => w.WithdrawRequestID);

        builder.Property(w => w.WalletID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(w => w.RequesterID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(w => w.ApproverID)
            .HasColumnType("char(36)")
            .IsRequired(false);

        builder.Property(w => w.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .HasColumnType("datetime")
            .IsRequired(false);

        builder.Property(w => w.ApprovedAt)
            .HasColumnType("datetime")
            .IsRequired(false);

        builder.Property(w => w.Note)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(w => w.RejectReason)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(w => w.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnType("varchar(20)")
            .HasConversion<string>()
            .IsRequired();

        builder.HasOne(w => w.Wallet)
            .WithMany()
            .HasForeignKey(w => w.WalletID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(t => t.RejectReason).HasColumnType("text").IsRequired(false);

        builder.ToTable(t =>
        t.HasCheckConstraint("CK_WithdrawRequest_Status", "Status IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELLED')"));
    }

}

