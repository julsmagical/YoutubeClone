using YoutubeClone.Application.Helpers;
using YoutubeClone.Application.Interfaces.Services;
using YoutubeClone.Application.Models.DTOS;
using YoutubeClone.Application.Models.Requests.User;
using YoutubeClone.Application.Models.Responses;
using YoutubeClone.Shared;
using YoutubeClone.Shared.Helpers;

namespace YoutubeClone.Application.Services
{
    public class UserService(Cache<UserDTO> cache) : IUserService
    {
        public GenericResponse<UserDTO> Create(CreateUserRequest model)
        {
            var user = new UserDTO
            {
                UserId = Guid.NewGuid(),
                UserName = model.UserName,
                DisplayName = model.DisplayName,
                Email = model.Email,
                Birthday = model.Birthday,
                Country = model.Country,
                Password = model.Password,
                CreatedAt = DateTimeHelper.UtcNow(),
                DeletedAt = DateTimeHelper.UtcNow(),
            };

            cache.Add(user.UserId.ToString(), user);
            return ResponseHelper.Create(user);
        }

        public GenericResponse<List<UserDTO>> Get(int limit, int offset)
        {
            var users = cache.Get();
            return ResponseHelper.Create(users);
        }

        public GenericResponse<UserDTO> Get(Guid userId)
        {
            var user = cache.Get(userId.ToString());
            return ResponseHelper.Create(user);
        }

        public GenericResponse<bool> Delete(Guid userId)
        {
            var siExiste = cache.Get(userId.ToString());

            if (siExiste is null)
            {
                return ResponseHelper.Create(false);
            }

            cache.Delete(userId.ToString());
            return ResponseHelper.Create(true);
        }
    }
}
