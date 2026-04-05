using Droniverse.Identity.Application.DTO.Extension;
using Droniverse.Identity.Application.DTO.Extension;
using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Identity.API.Controllers
{
    [Route("identity/users")]
    [Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            var vietnamTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                DateTime.UtcNow,
                "SE Asia Standard Time"
            );
            Console.WriteLine(vietnamTime.ToString("dddd:MM:yyyy:ss"));
            return Ok("Test successful");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            UserResponse account = await _userService.GetUserById(id);
            if (account == null)
                return NotFound($"Not found user with id: #{id}");
            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userCreateDto)
        {
            UserResponse userResponse = await _userService.AddUser(userCreateDto);
            return CreatedAtAction(nameof(GetUserById), new { id = userResponse.UserId }, userResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto userUpdateDto)
        {
            UserResponse updatedUser = await _userService.UpdateUser(id, userUpdateDto);
            return Ok(updatedUser);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            bool isDeleted = await _userService.DeleteUser(id);
            if (!isDeleted)
            {
                return NotFound($"Account id not found #{id}");
            }
            return NoContent();
        }

        [HttpPost("{userId}/image-url")]
        public async Task<IActionResult> UploadUserAvatar(Guid userId, IFormFile image)
        {
            string imageUrl = await _userService.UploadUserAvatar(userId, image);
            return Ok(SuccessResponse<string>.Create(imageUrl, "Image uploaded successfully."));
        }

        /// <summary>
        /// Dùng cho giao tiếp giữa các service
        /// </summary>
        /// <param name="userIds">List of user IDs to retrieve</param>
        /// <returns>List of UserResponse objects for the requested user IDs</returns>
        [HttpPost("bulk")]
        public async Task<IActionResult> GetUsersByIds(
            [FromBody] IEnumerable<Guid> userIds)
        {
            var vietnamTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                DateTime.UtcNow,
                "SE Asia Standard Time"
            );

            Console.WriteLine(vietnamTime.ToString("dddd:MM:yyyy:ss"));
            var users = await _userService.GetUsersByIds(userIds);
            return Ok(users);
        }

        /// <summary>
        /// Tìm kiếm người dùng theo thông tin hồ sơ (tên)
        /// </summary>
        /// <param name="request">Thông tin tìm kiếm</param>
        /// <returns>Danh sách UserID theo điều kiện tìm kiếm</returns>
        [HttpGet("search")]
        public async Task<IActionResult> GetUsersByUserInfo(
            [FromQuery] UserInfoSearchRequest request)
        {
            var userIds = await _userService.GetUsersByUserInfo(request);
            return Ok(userIds);
        }
    }
}
