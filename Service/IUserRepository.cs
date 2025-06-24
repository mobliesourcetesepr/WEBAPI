using AgentCreation.Models;

public interface IUserRepository
{
    Task<List<UserDto>> GetAllUsersAsync();
}
