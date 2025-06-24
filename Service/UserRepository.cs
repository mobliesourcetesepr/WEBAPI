using AgentCreation.Data;
using AgentCreation.Models;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _context.AdminUser
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Username,
                Email = u.AdminId,
            })
            .ToListAsync();
    }
}
