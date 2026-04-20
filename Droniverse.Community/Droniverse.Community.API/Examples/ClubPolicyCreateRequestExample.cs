using Droniverse.Community.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples
{
    public class ClubPolicyCreateRequestExample : IExamplesProvider<ClubPolicyCreateDto>
    {
        public ClubPolicyCreateDto GetExamples()
        {
            return new ClubPolicyCreateDto  
            {
                ClubId = Guid.Empty,
                Title = "Nội quy của câu lạc bộ",
                Content = "Đây là nội dung của nội quy câu lạc bộ. Nó nêu rõ các quy tắc và hướng dẫn cho các thành viên câu lạc bộ."
            };
        }
    }
}
