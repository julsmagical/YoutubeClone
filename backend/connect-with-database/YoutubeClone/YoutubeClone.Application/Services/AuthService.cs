using Microsoft.Extensions.Configuration;
using YoutubeClone.Application.Helpers;
using YoutubeClone.Application.Interfaces.Services;
using YoutubeClone.Application.Models.DTOS;
using YoutubeClone.Application.Models.Helpers;
using YoutubeClone.Application.Models.Requests.Auth;
using YoutubeClone.Application.Models.Requests.Auth.Register;
using YoutubeClone.Application.Models.Requests.User;
using YoutubeClone.Application.Models.Responses;
using YoutubeClone.Application.Models.Responses.Auth;
using YoutubeClone.Domain.Database.SqlServer;
using YoutubeClone.Domain.Exceptions;
using YoutubeClone.Domain.Interfaces.Repositories;
using YoutubeClone.Shared;
using YoutubeClone.Shared.Constants;

namespace YoutubeClone.Application.Services
{
    public class AuthService(IUnitOfWork uow, IUserRepository userRepository, IConfiguration configuration, ICacheService cacheService) : IAuthService
    {
        // CODIGO TEMPORAL DE PRUEBAS
        /*public async Task<GenericResponse<LoginAuthResponse>> Login(LoginAuthRequest model)
        {
            var userAccount = await uow.userRepository.GetAll(model.Email);

            Console.WriteLine($"=== DEBUG LOGIN ===");
            Console.WriteLine($"Email buscado: '{model.Email}'");
            Console.WriteLine($"Usuario encontrado: {userAccount != null}");

            if (userAccount == null)
                throw new BadRequestException(ResponseConstants.AUTH_USER_OR_PASSWORD_NOT_FOUND);

            Console.WriteLine($"Hash en DB: '{userAccount.Password}'");
            var validatePassword = Hasher.ComparePassword(model.Password, userAccount.Password);
            Console.WriteLine($"Password válida: {validatePassword}");

            if (!validatePassword)
                throw new BadRequestException(ResponseConstants.AUTH_USER_OR_PASSWORD_NOT_FOUND);

            var token = TokenHelper.Create(userAccount.UserId, [.. userAccount.UserAccountRoles.Select(x => x.Role.Name)], configuration, cacheService);
            var refreshToken = TokenHelper.CreateRefresh(userAccount.UserId, configuration, cacheService);

            return ResponseHelper.Create(new LoginAuthResponse
            {
                Token = token,
                RefreshToken = refreshToken
            });
        }*/
        public async Task<GenericResponse<LoginAuthResponse>> Login(LoginAuthRequest model)
        {
            var userAccount = await uow.userRepository.GetAll(model.Email)
                ?? throw new BadRequestException(ResponseConstants.AUTH_USER_OR_PASSWORD_NOT_FOUND);

            var validatePassword = Hasher.ComparePassword(model.Password, userAccount.Password);
            if (!validatePassword)
            {
                throw new BadRequestException(ResponseConstants.AUTH_USER_OR_PASSWORD_NOT_FOUND);
            }

            var token = TokenHelper.Create(userAccount.UserId, [.. userAccount.UserAccountRoles.Select(x => x.Role.Name)], configuration, cacheService);
            var refreshToken = TokenHelper.CreateRefresh(userAccount.UserId, configuration, cacheService);

            return ResponseHelper.Create(new LoginAuthResponse
            {
                Token = token,
                RefreshToken = refreshToken
            });
        }

        public async Task<GenericResponse<LoginAuthResponse>> Renew(RenewAuthRequest model)
        {
            var findRefreshToken = cacheService.Get<RefreshToken>(CacheHelper.AuthRefreshTokenKey(model.RefreshToken))
                ?? throw new NotFoundException(ResponseConstants.AUTH_REFRESH_TOKEN_NOT_FOUND);

            var user = await uow.userRepository.GetById(findRefreshToken.UserId)
                ?? throw new NotFoundException(ResponseConstants.USER_NOT_EXIST);

            var token = TokenHelper.Create(findRefreshToken.UserId, [.. user.UserAccountRoles.Select(x => x.Role.Name)], configuration, cacheService);
            var refreshToken = TokenHelper.CreateRefresh(findRefreshToken.UserId, configuration, cacheService);

            cacheService.Delete(CacheHelper.AuthRefreshTokenKey(model.RefreshToken));

            return ResponseHelper.Create(new LoginAuthResponse
            {
                Token = token,
                RefreshToken = refreshToken
            });
        }

        public Task<GenericResponse<UserDTO>> RegisterComplete(CreateUserRequest model, string token)
        {
            throw new NotImplementedException();
        }

        //Temporal
        public Task<GenericResponse<string>> RegisterInit(RegisterInitAuthRequest model)
        {
            throw new NotImplementedException();
        }

        /*public Task<GenericResponse<string>> RegisterInit(RegisterInitAuthRequest model)
        {
            var token = Generate.RandomText();
            var cacheKey = CacheHelper.AuthRegisterTokenCreation(token, TimeSpan.FromMinutes(5));
            cacheService.Create(cacheKey.Key, cacheKey.Expiration, model);
        }*/

        public Task<GenericResponse<string>> RegisterValidateToken(string token)
        {
            throw new NotImplementedException();
        }


    }
}
