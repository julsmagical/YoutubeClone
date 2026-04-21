using System.Security.Claims;
using YoutubeClone.Application.Models.DTOS;
using YoutubeClone.Application.Models.Requests.User;
using YoutubeClone.Application.Models.Responses;
using YoutubeClone.Domain.Database.SqlServer.Entities;

namespace YoutubeClone.Application.Interfaces.Services
{
    public interface IUserService
    {
        public Task<GenericResponse<UserDTO>> Create(CreateUserRequest model, Claim? claim);
        public GenericResponse<List<UserDTO>> GetAll(FilterUserRequest model);
        public Task<GenericResponse<UserDTO>> GetById(Guid userId);
        public Task<GenericResponse<bool>> Delete(Guid userId);
        public Task<GenericResponse<UserDTO>> Update(Guid id, UpdateUserRequest model, Claim claim);
        public Task<GenericResponse<UserDTO>> Me(Claim claim);
        Task<UserAccount> GetExecutor(string value);
        public Task CreateFirstUser();
    }
}
