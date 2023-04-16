using System.ComponentModel.DataAnnotations;

namespace VideoStore.IdentityService.DTOs
{
    public class UserDTO
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }
    }
}
