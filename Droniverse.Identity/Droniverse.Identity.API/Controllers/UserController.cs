using Droniverse.Identity.Application.DTO.Extension;
using Droniverse.Identity.Application.DTO.Extension;
using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.HttpClients;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Extensions;
using Droniverse.Shared.Services.IServices;
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
        private readonly IRoleService _roleService;
        private readonly AcademyMicroserviceClient _academyMicroserviceClient;
        private readonly ICloudinaryService _cloudinaryService;
        public UserController(IUserService userService, IRoleService roleService, AcademyMicroserviceClient academyMicroserviceClient, ICloudinaryService cloudinaryService)
        {
            _userService = userService;
            _roleService = roleService;
            _academyMicroserviceClient = academyMicroserviceClient;
            _cloudinaryService = cloudinaryService;
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
        /// <summary>
        /// Lấy ra danh sách người dùng của hệ thống với phân trang và lọc theo username, email, role
        /// </summary>
        /// <param name="userSearchRequest"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] UserSearchRequest userSearchRequest)
        {
            userSearchRequest ??= new UserSearchRequest();
            var users = await _userService.GetAllUsers(
                userSearchRequest,
                userSearchRequest.CurrentPage,
                userSearchRequest.PageSize);
            return Ok(users);
        }

        /// <summary>
        /// Lấy ra thông tin chi tiết của người dùng theo userID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            UserResponse account = await _userService.GetUserById(id);
            if (account == null)
                return NotFound($"Not found user with id: #{id}");
            return Ok(account);
        }

        /// <summary>
        /// Tạo người dùng mới trong hệ thống, trả về thông tin người dùng vừa được tạo ra
        /// </summary>
        /// <param name="userCreateDto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userCreateDto)
        {
            UserResponse userResponse = await _userService.AddUser(userCreateDto);
            return CreatedAtAction(nameof(GetUserById), new { id = userResponse.UserId }, userResponse);
        }

        /// <summary>
        /// Cập nhật người dùng theo userID, trả về thông tin người dùng sau khi đã được cập nhật
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userUpdateDto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto userUpdateDto)
        {
            UserResponse updatedUser = await _userService.UpdateUser(id, userUpdateDto);
            return Ok(updatedUser);
        }


        //[HttpDelete]
        //public async Task<IActionResult> DeleteUser(Guid id)
        //{
        //    bool isDeleted = await _userService.DeleteUser(id);
        //    if (!isDeleted)
        //    {
        //        return NotFound($"Account id not found #{id}");
        //    }
        //    return NoContent();
        //}

        /// <summary>
        /// Cập nhật avatar cho user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{userId}/upload-avatar")]
        public async Task<IActionResult> UploadUserAvatar(Guid userId, [FromForm] FileUploadDto file)
        {
            UserResponse userResponse = await _userService.UploadUserAvatar(userId, file.File);
            return Ok(userResponse);
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

        /// <summary>
        /// API call cross service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("search-pagination")]
        public async Task<IActionResult> SearchUsersWithPagination([FromBody] SearchUsersWithPaginationRequest request)
        {
            var result = await _userService.SearchUsersWithPagination(request);
            return Ok(result);
        }
    }
}
