using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AzureTechJwt.Entities
{
    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        [Key]
        public int UserId { get; set; }
        public List<UserRefreshToken> UserRefreshTokens { get; set; }

    }
}