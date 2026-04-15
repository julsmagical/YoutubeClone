using YoutubeClone.Domain.Database.SqlServer.Entities;

namespace YoutubeClone.Domain.Interfaces.Repositories
{
    public interface IChannelRepository
    {
        Task<Channel> Create(Channel channel);
        Task<Channel?> GetById(Guid userId);
        Task<Channel?> GetAll(string email);
        Task<Channel> Update(Channel channel);
        Task<bool> IfExist(Guid channelId);
        Task<bool> IfExist(string handle);
        IQueryable<Channel> Queryable();
        Task<bool> HasCreated();
    }
}
