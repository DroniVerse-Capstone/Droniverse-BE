using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples;

public class CreateModuleRequestExample : IMultipleExamplesProvider<CreateModuleRequestDTO>
{
    public IEnumerable<SwaggerExample<CreateModuleRequestDTO>> GetExamples()
    {
        // 1. Nhập môn
        yield return SwaggerExample.Create(
            "Nhập môn",
            new CreateModuleRequestDTO
            {
                TitleVN = "Mô-đun nhập môn điều khiển drone",
                TitleEN = "Drone Control Fundamentals",
                ModuleNumber = 1
            }
        );

        // 2. An toàn bay
        yield return SwaggerExample.Create(
            "An toàn bay",
            new CreateModuleRequestDTO
            {
                TitleVN = "Quy tắc an toàn khi bay drone",
                TitleEN = "Drone Flight Safety Rules",
                ModuleNumber = 2
            }
        );

        // 3. Lập trình drone cơ bản
        yield return SwaggerExample.Create(
            "Lập trình cơ bản",
            new CreateModuleRequestDTO
            {
                TitleVN = "Lập trình điều khiển drone cơ bản",
                TitleEN = "Basic Drone Programming",
                ModuleNumber = 3
            }
        );

        // 4. Drone SDK
        yield return SwaggerExample.Create(
            "Drone SDK",
            new CreateModuleRequestDTO
            {
                TitleVN = "Sử dụng SDK để điều khiển drone",
                TitleEN = "Using Drone SDK for Control",
                ModuleNumber = 4
            }
        );

        // 5. Tự động hóa chuyến bay
        yield return SwaggerExample.Create(
            "Autonomous Flight",
            new CreateModuleRequestDTO
            {
                TitleVN = "Lập trình bay tự động theo waypoint",
                TitleEN = "Autonomous Flight with Waypoints",
                ModuleNumber = 5
            }
        );

        // 6. Xử lý dữ liệu & telemetry
        yield return SwaggerExample.Create(
            "Telemetry",
            new CreateModuleRequestDTO
            {
                TitleVN = "Thu thập và xử lý dữ liệu bay (Telemetry)",
                TitleEN = "Drone Telemetry Data Processing",
                ModuleNumber = 6
            }
        );

        // 7. Computer Vision
        yield return SwaggerExample.Create(
            "Computer Vision",
            new CreateModuleRequestDTO
            {
                TitleVN = "Nhận diện vật thể bằng drone",
                TitleEN = "Object Detection with Drone Vision",
                ModuleNumber = 7
            }
        );

        // 8. Backend điều khiển drone
        yield return SwaggerExample.Create(
            "Backend System",
            new CreateModuleRequestDTO
            {
                TitleVN = "Xây dựng API điều khiển drone bằng .NET",
                TitleEN = "Building Drone Control APIs with .NET",
                ModuleNumber = 8
            }
        );

        // 9. Realtime communication
        yield return SwaggerExample.Create(
            "Realtime",
            new CreateModuleRequestDTO
            {
                TitleVN = "Giao tiếp realtime với drone (WebSocket/MQTT)",
                TitleEN = "Realtime Communication with Drone",
                ModuleNumber = 9
            }
        );

        // 10. Streaming video
        yield return SwaggerExample.Create(
            "Streaming",
            new CreateModuleRequestDTO
            {
                TitleVN = "Streaming video trực tiếp từ drone",
                TitleEN = "Live Video Streaming from Drone",
                ModuleNumber = 10
            }
        );

        // 11. AI & tránh vật cản
        yield return SwaggerExample.Create(
            "AI Navigation",
            new CreateModuleRequestDTO
            {
                TitleVN = "Tránh vật cản bằng AI",
                TitleEN = "Obstacle Avoidance using AI",
                ModuleNumber = 11
            }
        );

        // 12. Capstone project
        yield return SwaggerExample.Create(
            "Capstone",
            new CreateModuleRequestDTO
            {
                TitleVN = "Xây dựng hệ thống drone hoàn chỉnh",
                TitleEN = "Build a Complete Drone System",
                ModuleNumber = 12
            }
        );
    }
}