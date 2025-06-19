
using Microsoft.EntityFrameworkCore;
namespace AgentCreation.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
        public DbSet<AdminUser> AdminUser { get; set; }
        // public DbSet<SessionStore> SessionStores { get; set; }

    }
}
 
