using Droniverse.Identity.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Identity.API.Swagger
{
    public class RegisterExampleProvider : IMultipleExamplesProvider<RegisterDto>
    {
        public IEnumerable<SwaggerExample<RegisterDto>> GetExamples()
        {
            yield return SwaggerExample.Create(
                "User 1",
                new RegisterDto(
                    Password: "Abc123",
                    Email: "david@gmail.com",
                    FirstName: "David",
                    LastName: "Backham",
                    RoleName: "CLUB_MEMBER"
                )
            );

            yield return SwaggerExample.Create(
                "User 2",
                new RegisterDto(
                    Password: "Abc123",
                    Email: "quanlyclb@gmail.com",
                    FirstName: "Quản lý",
                    LastName: "Clb",
                    RoleName: "CLUB_MANAGER"
                )
            );
        }
    }
}
