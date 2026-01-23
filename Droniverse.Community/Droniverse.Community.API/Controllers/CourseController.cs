using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Community.API.Controllers
{
    [Route("api/courses")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCourses()
        {
            IEnumerable<CourseResponse> courses = await _courseService.GetAllCourses();
            return Ok(courses);
        }
    }
}
