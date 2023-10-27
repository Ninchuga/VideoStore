using System.ComponentModel.DataAnnotations;

namespace VideoStore.IdentityService.Requests
{
    public class UserLoginRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
