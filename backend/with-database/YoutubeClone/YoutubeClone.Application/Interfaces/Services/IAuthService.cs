using System.Security.Claims;
using YoutubeClone.Application.Models.DTOS;
using YoutubeClone.Application.Models.Requests.Auth;
using YoutubeClone.Application.Models.Requests.Auth.RecoverPassword;
using YoutubeClone.Application.Models.Requests.Auth.Register;
using YoutubeClone.Application.Models.Requests.User;
using YoutubeClone.Application.Models.Responses;
using YoutubeClone.Application.Models.Responses.Auth;

namespace YoutubeClone.Application.Interfaces.Services
{
    public interface IAuthService
    {
        // Iniciar sesión
        Task<GenericResponse<LoginAuthResponse>> Login(LoginAuthRequest model);
        Task<GenericResponse<LoginAuthResponse>> Renew(RenewAuthRequest model);

        // Registrarse
        Task<GenericResponse<string>> RegisterInit(RegisterInitAuthRequest model);
        Task<GenericResponse<RegisterInitAuthResponse>> RegisterValidateToken(string token);
        Task<GenericResponse<UserDTO>> RegisterComplete(CreateUserRequest model, string token);

        // Recuperar contraseña
        Task<GenericResponse<string>> RecoverPasswordSendOTP(RecoverPasswordSendOTPAuthRequest model);
        Task<GenericResponse<string>> RecoverPassword(RecoverPasswordAuthRequest model, string code);
        Task<GenericResponse<string>> ChangePassword(ChangePasswordAuthRequest model, Claim claim);
    }
}
