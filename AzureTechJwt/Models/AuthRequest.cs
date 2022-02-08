using System.ComponentModel.DataAnnotations;

namespace AzureTechJwt.Entities
{
    public class AuthRequest
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}