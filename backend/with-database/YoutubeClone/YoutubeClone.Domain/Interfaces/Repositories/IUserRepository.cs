using YoutubeClone.Domain.Database.SqlServer.Entities;

namespace YoutubeClone.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<UserAccount>
    {
        Task<UserAccount?> GetById(Guid userId);
        Task<UserAccount?> GetAll(string email);
        Task<bool> HasCreated();
        Task<bool> ClearRoles(List<UserAccountRole> roles);
    }
}
