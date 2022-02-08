using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureTechJwt.Entities
{
    [Table(("UserRefreshToken"))]
    public class UserRefreshToken
    {
        [Key]
        public int UserRefreshTokenId { get; set; }

        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpirationDate { get; set; }

        [NotMapped]
        public bool IsActive => ExpirationDate > DateTime.UtcNow;
        
        public string IpAddress { get; set; }
        public bool IsInvalidated { get; set; }
        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        
    }
}