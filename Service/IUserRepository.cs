using AgentCreation.Models;
// namespace AgentCreation.IUserRepository
// {
//     public interface IUserRepository
// {
//     Task<List<UserDto>> GetUsernamesByAdminIdAsync();
// }
// }

namespace AgentCreation.Repositories
{
    public interface IUserRepository
    {
        Task<List<string>> GetUsernamesByAdminIdAsync(string adminId);
    }
}
