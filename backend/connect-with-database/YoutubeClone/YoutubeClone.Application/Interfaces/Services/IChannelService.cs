using YoutubeClone.Application.Models.DTOs;
using YoutubeClone.Application.Models.Requests.Channel;
using YoutubeClone.Application.Models.Responses;

namespace YoutubeClone.Application.Interfaces.Services
{
    public interface IChannelService
    {
        public Task<GenericResponse<ChannelDTO>> Create(CreateChannelRequest model);
        //public Task<GenericResponse<List<ChannelDTO>>> GetAll(FilterChannelRequest model);
        //public Task<GenericResponse<ChannelDTO>> GetById(Guid channelId);
        //public Task<GenericResponse<bool>> Delete(Guid channelId);
        //public Task<GenericResponse<ChannelDTO>> Update(Guid id, UpdateUserRequest model);
        public Task CreateFirstChannel();
    }
}
