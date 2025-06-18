using Microsoft.EntityFrameworkCore;
using MultiTenantAPI.Models;
namespace MultiTenantAPI.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<LogDetail> LogDetails { get; set; }
        public DbSet<AdminAudit> AdminAudits { get; set; }
        public DbSet<AdminUser> AdminUser { get; set; }
        public DbSet<SubAgent> SubAgents { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }  
        public DbSet<LogEntry> Logs { get; set; }
        public DbSet<SessionStore> SessionStores { get; set; }

    }
}
 
