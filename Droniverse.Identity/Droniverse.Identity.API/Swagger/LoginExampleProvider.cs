using Droniverse.Identity.Application.DTO.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Identity.API.Swagger
{
    public class LoginExampleProvider : IMultipleExamplesProvider<LoginEmailDto>
    {
        public IEnumerable<SwaggerExample<LoginEmailDto>> GetExamples()
        {
            yield return SwaggerExample.Create(
                "Admin Account",
                new LoginEmailDto(
                    Email: "admin@gmail.com",
                    Password: "Abc123"
                )
            );

            yield return SwaggerExample.Create(
                "System Manager Account",
                new LoginEmailDto(
                    Email: "sysmanager@gmail.com",
                    Password: "Abc123"
                )
            );

            yield return SwaggerExample.Create(
                "Club Manager Account",
                new LoginEmailDto(
                    Email: "clubmanager@gmail.com",
                    Password: "Abc123"
                )
            );

            yield return SwaggerExample.Create(
                "Club Member Account",
                new LoginEmailDto(
                    Email: "clubmember@gmail.com",
                    Password: "Abc123"
                )
            );
        }
    }
}
