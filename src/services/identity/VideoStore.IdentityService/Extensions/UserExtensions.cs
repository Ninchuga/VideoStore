using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VideoStore.IdentityService.DTOs;
using VideoStore.IdentityService.Model;
using VideoStore.IdentityService.Requests;

namespace VideoStore.IdentityService.Extensions
{
    public static class UserExtensions
    {
        private const string UserIdClaim = "userId";

        public static UserDTO ToDto(this User user) =>
            new() { Password = user.Password, UserName = user.UserName, Email = user.Email };

        public static IReadOnlyList<UserDTO> ToDtos(this IEnumerable<User> users) =>
            users.Select(ToDto).ToList();

        public static User ToEntity(this UserDTO user) =>
            new() { Password = user.Password, UserName = user.UserName, Email = user.Email };

        public static IReadOnlyList<User> ToDtos(this IEnumerable<UserDTO> users) =>
            users.Select(ToEntity).ToList();

        public static User ToEntity(this UserLoginRequest loginRequest) =>
            new() { Password = loginRequest.Password, UserName = loginRequest.UserName };

        public static List<Claim> BuildUserJwtClaims(this User user) =>
            new()
            {
                new Claim(UserIdClaim, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            };
    }
}
