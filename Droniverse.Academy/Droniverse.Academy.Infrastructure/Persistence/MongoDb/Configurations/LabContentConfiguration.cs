using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Droniverse.Community.Infrastructure.Persistence.MongoDb.Configurations;

public class LabContentConfiguration : IEntityTypeConfiguration<LabContent>
{
    public void Configure(EntityTypeBuilder<LabContent> builder)
    {
        // 1. Cấu hình Collection
        builder.ToCollection("lab_contents");

        // 2. Cấu hình Primary Key
        builder.HasKey(i => i._id);

        // 3. Cấu hình Environment (Nested Object 1:1)
        builder.OwnsOne(i => i.Environment, env =>
        {
            env.HasElementName("environment");

            // 3.1 Các mảng tọa độ [x,y,z]
            // EF Core Mongo xử lý mảng primitive (double[]) như một Property bình thường
            env.Property(e => e.Start).HasElementName("start");
            env.Property(e => e.Goal).HasElementName("goal");

            // 3.2 List các mảng tọa độ (Mảng 2 chiều)
            env.Property(e => e.Checkpoints).HasElementName("checkpoints");

            // 3.3 List Obstacles (Nested List Objects 1:N)
            // QUAN TRỌNG: Dùng OwnsMany cho danh sách object
            env.OwnsMany(e => e.Obstacles, obs =>
            {
                obs.HasElementName("obstacles");

                // Mapping các field bên trong Obstacle
                obs.Property(o => o.Id).HasElementName("id"); // Đây là ID logic, không phải _id của Mongo
                obs.Property(o => o.Type).HasElementName("type");

                // Các mảng tọa độ của Obstacle
                obs.Property(o => o.Position).HasElementName("position");
                obs.Property(o => o.Rotation).HasElementName("rotation");
                obs.Property(o => o.Size).HasElementName("size");
                obs.Property(o => o.Center).HasElementName("center");

                // Các field Optional (Nullable)
                obs.Property(o => o.Radius)
                   .HasElementName("radius")
                   .IsRequired(false); // Cho phép null

                obs.Property(o => o.Height)
                   .HasElementName("height")
                   .IsRequired(false); // Cho phép null
            });
        });
    }
}