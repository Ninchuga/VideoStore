using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoStore.IdentityService.DTOs;
using VideoStore.IdentityService.Extensions;
using VideoStore.IdentityService.Infrastrucutre.Repositories;
using VideoStore.IdentityService.Requests;
using VideoStore.IdentityService.Responses;
using VideoStore.IdentityService.Services;

namespace VideoStore.IdentityService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class UserIdentityController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly TokenService _tokenService;

        public UserIdentityController(IUserRepository userRepository, TokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        [HttpGet]
        [Route("getUsers")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _userRepository.GetAllUsers();

            return users.Any() ? Ok(users.ToDtos()) : NotFound();
        }

        [HttpPost]
        [Route("createUser")]
        public async Task<ActionResult> CreateUser([FromBody] UserDTO user)
        {
            var existingUser = await _userRepository.FindUser(user.ToEntity());
            if (existingUser is not null)
                return BadRequest($"User with username {user.UserName} already exists.");

            _userRepository.AddUser(user.ToEntity());
            await _userRepository.SaveChanges();

            return Ok(user);
        }

        [HttpPost]
        [Route("updateUser")]
        public async Task<ActionResult> UpdateUser([FromBody] UserDTO user)
        {
            var existingUser = await _userRepository.FindUser(user.ToEntity());
            if (existingUser is not null)
                return BadRequest($"User with username {user.UserName} already exists.");

            _userRepository.AddUser(user.ToEntity());
            await _userRepository.SaveChanges();

            return Ok(user);
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<UserLoginResponse>> Login([FromBody] UserLoginRequest user)
        {
            if (user is null || string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Invalid user request.");

            var existingUser = await _userRepository.FindUser(user.ToEntity());
            if (existingUser is null)
                return Unauthorized("Invalid credentials.");

            return Ok(new UserLoginResponse(await _tokenService.GenerateTokenFor(existingUser)));
        }
    }
}
