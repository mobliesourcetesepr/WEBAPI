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

    }
}
