using YoutubeClone.Domain.Database.SqlServer.Context;
using YoutubeClone.Domain.Database.SqlServer.Entities;
using YoutubeClone.Domain.Interfaces.Repositories;

namespace YoutubeClone.Infraestructure.Persistence.SqlServer.Repositories
{
    public class ChannelRepository(YoutubeCloneContext context) : IChannelRepository
    {
        public Task<Channel> Create(Channel channel)
        {
            throw new NotImplementedException();
        }

        public Task<Channel?> GetAll(string email)
        {
            throw new NotImplementedException();
        }

        public Task<Channel?> GetById(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasCreated()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IfExist(Guid channelId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IfExist(string handle)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Channel> Queryable()
        {
            throw new NotImplementedException();
        }

        public Task<Channel> Update(Channel channel)
        {
            throw new NotImplementedException();
        }
    }
}
