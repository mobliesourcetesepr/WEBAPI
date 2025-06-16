using Microsoft.EntityFrameworkCore;
using MultiTenantAPI.Models;

namespace MultiTenantAPI.Data
{
    public class MyLogDbContext : DbContext
    {
        public MyLogDbContext(DbContextOptions<MyLogDbContext> options) : base(options) { }
        public DbSet<AdminUser> AdminUser { get; set; }
        public DbSet<LogDetail> LogDetails { get; set; }
    }
}
