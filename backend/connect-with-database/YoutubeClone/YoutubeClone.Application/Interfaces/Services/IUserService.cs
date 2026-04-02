using YoutubeClone.Application.Models.DTOS;
using YoutubeClone.Application.Models.Requests.User;
using YoutubeClone.Application.Models.Responses;

namespace YoutubeClone.Application.Interfaces.Services
{
    public interface IUserService
    {
        public Task<GenericResponse<UserDTO>> Create(CreateUserRequest model);
        public Task<GenericResponse<List<UserDTO>>> GetAll(FilterUserRequest model);
        public Task<GenericResponse<UserDTO>> GetById(Guid userId);
        public Task<GenericResponse<bool>> Delete(Guid userId);
        public Task<GenericResponse<UserDTO>> Update(Guid id, UpdateUserRequest model);
    }
}
