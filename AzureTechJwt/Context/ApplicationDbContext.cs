using AzureTechJwt.Entities;
using Microsoft.EntityFrameworkCore;

namespace AzureTechJwt.Context
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;
        public ApplicationDbContext(DbContextOptions options) : base(options) {}
    }
}