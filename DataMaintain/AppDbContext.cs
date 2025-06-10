using Microsoft.EntityFrameworkCore;
using WEBAPI.Models;

namespace WEBAPI.DataMaintain
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }
    }
}