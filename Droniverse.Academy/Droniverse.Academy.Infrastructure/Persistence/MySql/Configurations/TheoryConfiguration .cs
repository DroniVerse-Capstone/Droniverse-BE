using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class TheoryConfiguration : IEntityTypeConfiguration<Theory>
{
    public void Configure(EntityTypeBuilder<Theory> builder)
    {
        builder.ToTable("Theory");
        builder.HasKey(e => e.TheoryID);

        builder.Property(e => e.TheoryID).HasColumnType("char(36)");
        builder.Property(e => e.ContentVN).HasColumnType("varchar(255)");
        builder.Property(e => e.ContentEN).HasColumnType("varchar(255)");
        builder.Property(e => e.CreateBy).HasColumnType("char(36)");
        builder.Property(e => e.UpdateBy).HasColumnType("char(36)");
        builder.Property(e => e.CreateAt).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(e => e.UpdateAt).HasColumnType("datetime").ValueGeneratedOnUpdate();
        builder.Property(e => e.EstimatedTime).HasColumnType("int");


    }
}
