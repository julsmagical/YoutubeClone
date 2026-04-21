using YoutubeClone.Application.Models.DTOS;
using YoutubeClone.Application.Models.Requests.User;
using YoutubeClone.Application.Models.Responses;

namespace YoutubeClone.Application.Interfaces.Services
{
    public interface IUserService
    {
        public GenericResponse<UserDTO> Create(CreateUserRequest model);
        public GenericResponse<List<UserDTO>> Get(int limit, int offset);
        public GenericResponse<UserDTO?> Get(Guid userId);
        public GenericResponse<bool> Delete(Guid userId);
    }
}
