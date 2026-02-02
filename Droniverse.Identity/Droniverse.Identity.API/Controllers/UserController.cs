using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Identity.API.Controllers
{
    [Route("identity/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
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
            if(!isDeleted)
            {
                return NotFound($"Account id not found #{id}");
            }
            return NoContent();
        }
    }
}
