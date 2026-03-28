using Droniverse.Academy.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Examples
{
    public class GetCoursesByIdsExample : IMultipleExamplesProvider<GetCoursesByIdsRequestDTO>
    {
        public IEnumerable<SwaggerExample<GetCoursesByIdsRequestDTO>> GetExamples()
        {
            yield return SwaggerExample.Create(
                "Example 1: Lấy nhiều khóa học theo danh sách ID",
                new GetCoursesByIdsRequestDTO
                {
                    CourseIds = new List<Guid>
                    {
                        Guid.Parse("11f05847-4461-40c4-901f-fa8d46c0a481"),
                        Guid.Parse("22667d21-b3af-438d-833d-2f2967b8268b"),
                        Guid.Parse("3ba568bb-7657-41db-87ad-e867f7dca183")
                    }
                }
            );

            yield return SwaggerExample.Create(
                "Example 2: Lấy 1 khóa học duy nhất",
                new GetCoursesByIdsRequestDTO
                {
                    CourseIds = new List<Guid>
                    {
                        Guid.Parse("11f05847-4461-40c4-901f-fa8d46c0a481")
                    }
                }
            );
        }
    }
}
