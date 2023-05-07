using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using VideoStore.IdentityService.Constants;
using VideoStore.IdentityService.Extensions;
using VideoStore.IdentityService.Model;

namespace VideoStore.IdentityService.Services
{
    public class TokenService
    {
        private readonly IOptionsSnapshot<JwtConfig> _jwtConfiguration;
        private readonly SecretClient _secretClient;

        public TokenService(IOptionsSnapshot<JwtConfig> jwtConfiguration, SecretClient secretClient)
        {
            _jwtConfiguration = jwtConfiguration;
            _secretClient = secretClient;
        }

        public async Task<string> GenerateTokenFor(User user)
        {
            var jwtKeyVaultSecretResponse = await _secretClient.GetSecretAsync(IdentityConstants.JwtSecretKeyName);
            var jwtKeyValueSecret = jwtKeyVaultSecretResponse.Value;
            
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeyValueSecret.Value));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var tokeOptions = new JwtSecurityToken(
                issuer: _jwtConfiguration.Value.Issuer,
                audience: _jwtConfiguration.Value.Audience,
                claims: user.BuildUserJwtClaims(),
                expires: DateTime.UtcNow.AddHours(12), // only in dev environment
                signingCredentials: signinCredentials);
            return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        }
    }
}
