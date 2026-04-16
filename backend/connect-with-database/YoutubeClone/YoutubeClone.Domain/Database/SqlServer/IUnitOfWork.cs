using YoutubeClone.Domain.Interfaces.Repositories;

namespace YoutubeClone.Domain.Database.SqlServer
{
    public interface IUnitOfWork
    {
        IUserRepository userRepository { get; set; }
        Task SaveChangesAsync();
    }
}
