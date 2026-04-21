using YoutubeClone.Application.Models.DTOS;
using YoutubeClone.Application.Models.Requests.Auth;
using YoutubeClone.Application.Models.Requests.Auth.Register;
using YoutubeClone.Application.Models.Requests.User;
using YoutubeClone.Application.Models.Responses;
using YoutubeClone.Application.Models.Responses.Auth;

namespace YoutubeClone.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<GenericResponse<LoginAuthResponse>> Login(LoginAuthRequest model);
        Task<GenericResponse<LoginAuthResponse>> Renew(RenewAuthRequest model);
        Task<GenericResponse<string>> RegisterInit(RegisterInitAuthRequest model);
        Task<GenericResponse<string>> RegisterValidateToken(string token);
        Task<GenericResponse<UserDTO>> RegisterComplete(CreateUserRequest model, string token);
    }
}
