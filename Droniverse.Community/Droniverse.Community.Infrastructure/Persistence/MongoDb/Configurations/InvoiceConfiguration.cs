using Droniverse.Community.Domain.Entities.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MongoDb.Configurations;
public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i._id);

        builder.Property(i => i._id)
            .IsRequired();

        builder.Property(i => i.OrderID)
            .IsRequired();
        builder.Property(i => i.ClubID).IsRequired();
        builder.Property(i => i.TotalAmount)
            .IsRequired();

        builder.Property(i => i.ContentVN)
            .IsRequired();

        builder.Property(i => i.ContentEN)
            .IsRequired();

        builder.Property(i => i.IssueAt)
            .IsRequired();


        builder.OwnsOne(i => i.CustomerInfo, customer =>
        {
            customer.Property(c => c.UserID).IsRequired();
            customer.Property(c => c.FullName).IsRequired();
            customer.Property(c => c.Email).IsRequired();
            customer.Property(c => c.TaxCode); // nullable
        });

        builder.OwnsOne(i => i.Item, item =>
        {
            item.Property(i => i.ProductID).IsRequired();
            item.Property(i => i.ProductNameVN).IsRequired();
            item.Property(i => i.ProductNameEN).IsRequired();
            item.Property(i => i.UnitPrice).IsRequired();
            item.Property(i => i.Quantity).IsRequired();
            item.Property(i => i.Total).IsRequired();
        });
    }
}


