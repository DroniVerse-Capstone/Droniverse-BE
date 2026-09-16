using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Droniverse.Community.Infrastructure.Persistence.MongoDb.Configurations;

public class LabContentConfiguration : IEntityTypeConfiguration<LabContent>
{
    public void Configure(EntityTypeBuilder<LabContent> builder)
    {
        builder.ToCollection("lab_contents");

        builder.HasKey(x => x._id);

        builder.Property(x => x.Environment)
               .HasElementName("environment");
    }
}
