using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations
{
    public class ClubCreationRequestConfiguration : IEntityTypeConfiguration<ClubCreationRequest>
    {
        public void Configure(EntityTypeBuilder<ClubCreationRequest> builder)
        {
            builder.ToTable("ClubCreationRequest");

            builder.HasKey(x => x.ClubCreationRequestID);

            builder.Property(x => x.ClubCreationRequestID)
                .HasColumnType("char(36)");

            builder.Property(x => x.NameVN)
                .HasMaxLength(255);

            builder.Property(x => x.NameEN)
                .HasMaxLength(255);

            builder.Property(x => x.ClubCode)
                .HasColumnType("char(6)");

            builder.Property(x => x.ImageUrl)
                .HasColumnType("text");

            builder.Property(x => x.IsPublic)
                .HasColumnType("boolean");

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            builder.Property(x => x.ApprovedAt);

            builder.Property(x => x.RejectReason)
                .HasMaxLength(255);

            builder.Property(x => x.ClubID)
                .HasColumnType("char(36)")
                .IsRequired(false);

            builder.Property(x => x.RequesterID)
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(x => x.ApproverID)
                .HasColumnType("char(36)")
                .IsRequired(false);

            builder.Property(x => x.Status)
                .HasConversion<byte>()
                .IsRequired();

            builder.HasOne(x => x.Club)
                .WithMany()
                .HasForeignKey(x => x.ClubID)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }

}
