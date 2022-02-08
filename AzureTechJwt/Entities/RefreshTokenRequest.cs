using System.ComponentModel.DataAnnotations;

namespace AzureTechJwt.Entities
{
    public class RefreshTokenRequest
    {
        [Required]
        public string ExpiredToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}