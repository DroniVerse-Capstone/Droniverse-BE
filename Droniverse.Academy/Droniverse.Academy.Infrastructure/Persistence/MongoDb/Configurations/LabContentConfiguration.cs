//using Droniverse.Academy.Domain.Entities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

//namespace Droniverse.Community.Infrastructure.Persistence.MongoDb.Configurations;
//public class LabContentConfiguration : IEntityTypeConfiguration<LabContent>
//{
//    public void Configure(EntityTypeBuilder<LabContent> builder)
//    {
//        builder.HasKey(i => i._id);

//        builder.Property(i => i._id)
//            .IsRequired();

//        builder.Property(i => i.OrderID)
//            .IsRequired();

//        builder.Property(i => i.TotalAmount)
//            .IsRequired();

//        builder.Property(i => i.ContentVN)
//            .IsRequired();

//        builder.Property(i => i.ContentEN)
//            .IsRequired();

//        builder.Property(i => i.IssueAt)
//            .IsRequired();


//        builder.OwnsOne(i => i.CustomerInfo, customer =>
//        {
//            customer.Property(c => c.UserID).IsRequired();
//            customer.Property(c => c.Name).IsRequired();
//            customer.Property(c => c.TaxCode); // Nullable
//        });
//    }
//}


