using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Community.API.Controllers
{
    [ApiController]
    [Route("community/dashboards")]
    [Authorize]
    public class DashboardController : ControllerBase
    {

    }
}
