using YoutubeClone.Application.Interfaces.Services;
using YoutubeClone.Application.Models.DTOs;
using YoutubeClone.Application.Models.Requests.Channel;
using YoutubeClone.Application.Models.Responses;

namespace YoutubeClone.Application.Services
{
    public class ChannelService : IChannelService
    {
        public Task<GenericResponse<ChannelDTO>> Create(CreateChannelRequest model)
        {
            throw new NotImplementedException();
        }

        public Task CreateFirstChannel()
        {
            throw new NotImplementedException();
        }
    }
}
