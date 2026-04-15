using YoutubeClone.Domain.Database.SqlServer.Entities;

namespace YoutubeClone.Domain.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<UserAccount> Create(UserAccount userAccount);
        Task<UserAccount?> GetById(Guid userId);
        Task<UserAccount?> GetAll(string email);
        Task<UserAccount> Update(UserAccount userAccount);
        Task<bool> IfExist(Guid userId);
        Task<bool> IfExist(string userName);
        IQueryable<UserAccount> Queryable();
        Task<bool> HasCreated();
    }
}
