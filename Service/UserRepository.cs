using AgentCreation.Data;
using AgentCreation.Models;
using Microsoft.EntityFrameworkCore;
using AgentCreation.Repositories;
// public class UserRepository : IUserRepository
// {
//     private readonly UserDbContext _context;

//     public UserRepository(UserDbContext context)
//     {
//         _context = context;
//     }

//     public async Task<List<UserDto>> GetAllUsersAsync(string adminId)
//     {
// return await _context.AdminUser
//             .Where(u => u.AdminId == adminId)
//             .Select(u => u.Username)
//             .ToListAsync();
//     }
// }
public class UserRepository : IUserRepository{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context;
    }
    public async Task<List<string>> GetUsernamesByAdminIdAsync(string adminId)
    {
        return await _context.AdminUser
            .Where(u => u.AdminId == adminId)
            .Select(u => u.Username)
            .ToListAsync();
    }
}
