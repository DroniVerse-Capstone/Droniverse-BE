using Droniverse.Community.Domain.Entities.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MongoDb.Configurations;
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o._id);

        builder.Property(o => o._id).IsRequired();
        builder.Property(o => o.UserID).IsRequired();
        builder.Property(o => o.TotalAmount).IsRequired();
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .IsRequired();
        builder.Property(o => o.CreateAt)
            .IsRequired();
        builder.Property(o => o.InvoiceID);

        builder.OwnsOne(o => o.Item, items =>
        {
            items.Property(i => i.ProductID).IsRequired();
            items.Property(i => i.ProductName).IsRequired();
            items.Property(i => i.Type)
                .HasConversion<string>()
                .IsRequired();
            items.Property(i => i.UnitOfPrice).IsRequired();
            items.Property(i => i.Quantity).IsRequired();
            items.Property(i => i.Total).IsRequired();
        });

        builder.OwnsOne(o => o.Payment, payment =>
        {
            payment.Property(p => p.TransactionID).IsRequired();
            payment.Property(p => p.PaymentMethod)
                .HasConversion<string>()
                .IsRequired();
            payment.Property(p => p.PaymentStatus)
                .HasConversion<string>()
                .IsRequired();
            payment.Property(p => p.TransactionDate).IsRequired();
        });
    }
}

