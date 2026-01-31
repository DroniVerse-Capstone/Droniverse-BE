using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions; // Cần thư viện này để dùng HasElementName

namespace Droniverse.Community.Infrastructure.Persistence.MongoDb.Configurations;

public class SimulateConfigConfiguration : IEntityTypeConfiguration<SimulateConfig>
{
    public void Configure(EntityTypeBuilder<SimulateConfig> builder)
    {
        // 1. Cấu hình Collection (Tên bảng trong Mongo)
        builder.ToCollection("simulate_configs");

        // 2. Cấu hình Primary Key
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasElementName("_id");

        // 3. UserID
        builder.Property(i => i.UserId)
            .HasElementName("userID") // Map đúng tên field trong ảnh (camelCase)
            .IsRequired();

        // 4. Nested Object: SandboxColorConfig
        // Sử dụng OwnsOne để nhúng document con vào document cha
        builder.OwnsOne(i => i.SandboxColorConfig, sandbox =>
        {
            sandbox.HasElementName("sandboxColorConfig");

            // 4.1 Nested sâu hơn: Drone Config
            sandbox.OwnsOne(s => s.Drone, drone =>
            {
                drone.HasElementName("drone");

                // Các field bắt buộc (NOT NULL)
                drone.Property(d => d.Fuselage).HasElementName("fuselage").IsRequired();
                drone.Property(d => d.Nose).HasElementName("nose").IsRequired();
                drone.Property(d => d.Canopy).HasElementName("canopy").IsRequired();
                drone.Property(d => d.Wings).HasElementName("wings").IsRequired();
                drone.Property(d => d.Rotor).HasElementName("rotor").IsRequired();

                // Các field tùy chọn (OPTIONAL)
                drone.Property(d => d.FuselageEmi).HasElementName("fuselageEmi").IsRequired(false);
                drone.Property(d => d.NoseEmi).HasElementName("noseEmi").IsRequired(false);
                drone.Property(d => d.RotorEmi).HasElementName("rotorEmi").IsRequired(false);
            });

            // 4.2 Nested sâu hơn: Map Config
            sandbox.OwnsOne(s => s.Map, map =>
            {
                map.HasElementName("map");

                // Tất cả đều bắt buộc (NOT NULL)
                map.Property(m => m.Ground).HasElementName("ground").IsRequired();
                map.Property(m => m.Grid).HasElementName("grid").IsRequired();
                map.Property(m => m.Border).HasElementName("border").IsRequired();
                map.Property(m => m.Ambient).HasElementName("ambient").IsRequired();
            });
        });

        // 5. Nested Object: DisplayConfig
        builder.OwnsOne(i => i.DisplayConfig, display =>
        {
            display.HasElementName("displayConfig");

            // Toàn bộ là OPTIONAL
            display.Property(d => d.TrailEnabled).HasElementName("trailEnabled");
            display.Property(d => d.TrailColor).HasElementName("trailColor");
            display.Property(d => d.TrailMaxLength).HasElementName("trailMaxLength");
            display.Property(d => d.Smoothing).HasElementName("smoothing");
            display.Property(d => d.Fade).HasElementName("fade");
            display.Property(d => d.SampleDistance).HasElementName("sampleDistance");
            display.Property(d => d.LineWidth).HasElementName("lineWidth");
        });
    }
}