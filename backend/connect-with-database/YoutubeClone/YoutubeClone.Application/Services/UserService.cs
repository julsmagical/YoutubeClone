using Microsoft.Extensions.Configuration;
using System.Globalization;
using YoutubeClone.Application.Helpers;
using YoutubeClone.Application.Interfaces.Services;
using YoutubeClone.Application.Models.DTOS;
using YoutubeClone.Application.Models.Requests.User;
using YoutubeClone.Application.Models.Responses;
using YoutubeClone.Domain.Database.SqlServer;
using YoutubeClone.Domain.Database.SqlServer.Entities;
using YoutubeClone.Domain.Exceptions;
using YoutubeClone.Shared;
using YoutubeClone.Shared.Constants;
using YoutubeClone.Shared.Helpers;

namespace YoutubeClone.Application.Services
{
    public class UserService(IUnitOfWork uow, IConfiguration configuration) : IUserService
    {
        public async Task<GenericResponse<UserDTO>> Create(CreateUserRequest model)
        {
            // VALIDACIONES
            /*var queryable = repository.Queryable();

            bool userNameExists = queryable.Any(u => u.UserName == model.UserName.ToLower());
            if (userNameExists) //username unico
            {
                return ResponseHelper.Create<UserDTO>(null, "Este username ya existe");
            }

            bool emailExists = queryable.Any(u => u.Email == model.Email.ToLower());
            if (emailExists) //email unico
            {
                return ResponseHelper.Create<UserDTO>(null, "Este email ya fue registrado");
            }

            var today = DateTime.Today; //edad minima
            var age = today.Year - model.Birthday.Year;
            if (model.Birthday > today.AddYears(-age))
            {
                age--;
            }
            if (age < 13)
            {
                return ResponseHelper.Create<UserDTO>(null, "La edad mínima es 13 años");
            }*/

            //throw new Exception("La base de datos no se pudo conectar con el servicio");

            var create = await uow.userRepository.Create(new UserAccount
            {
                UserId = Guid.NewGuid(),
                UserName = model.UserName.ToLower(),
                DisplayName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.DisplayName.ToLower()),
                Email = model.Email.ToLower(),
                Birthday = model.Birthday,
                Location = model.Location,
                Password = model.Password,
                CreatedAt = DateTimeHelper.UtcNow(),
                DeletedAt = null,
            });

            await uow.SaveChangesAsync();

            return ResponseHelper.Create(Map(create), [], "Usuario creado correctamente.");
        }

        public async Task<GenericResponse<bool>> Delete(Guid id)
        {
            var user = await GetUser(id);

            user.DeletedAt = DateTimeHelper.UtcNow();

            await uow.userRepository.Update(user);

            return ResponseHelper.Create(true);
        }

        public GenericResponse<List<UserDTO>> GetAll(FilterUserRequest model)
        {
            var queryable = uow.userRepository.Queryable();

            if (!string.IsNullOrWhiteSpace(model.UserName))
            {
                queryable = queryable.Where(x => x.UserName.Contains(model.UserName ?? ""));
            }
            if (!string.IsNullOrWhiteSpace(model.DisplayName))
            {
                queryable = queryable.Where(x => x.DisplayName.Contains(model.DisplayName ?? ""));
            }
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                queryable = queryable.Where(x => x.Email.Contains(model.Email ?? ""));
            }
            if (!string.IsNullOrWhiteSpace(model.Location))
            {
                queryable = queryable.Where(x => x.Location.Contains(model.Location ?? ""));
            }

            // paginacion y consultas
            var users = queryable.Skip(model.Offset).Take(model.Limit).ToList();

            List<UserDTO> mapped = []; //mapear resultado
            foreach (var user in users)
            {
                mapped.Add(Map(user));
            }
            return ResponseHelper.Create(mapped);
        }

        public async Task<GenericResponse<UserDTO>> GetById(Guid id)
        {
            var user = await GetUser(id);
            return ResponseHelper.Create(Map(user));
        }

        public async Task<GenericResponse<UserDTO>> Update(Guid id, UpdateUserRequest model)
        {
            var user = await GetUser(id);

            user.UserName = model.UserName ?? user.UserName;
            user.DisplayName = model.DisplayName ?? user.DisplayName;
            user.Email = model.Email ?? user.Email;
            user.Location = model.Location ?? user.Location;
            //actualizar updatedAt cuando el campo este disponible en la entidad
            user.UpdatedAt = DateTimeHelper.UtcNow();

            var update = await uow.userRepository.Update(user);

            await uow.SaveChangesAsync();

            return ResponseHelper.Create(Map(user));
        }

        private async Task<UserAccount> GetUser(Guid id)
        {
            return await uow.userRepository.GetById(id)
                ?? throw new NotFoundException(ResponseConstants.USER_NOT_EXIST);
        }

        private static UserDTO Map(UserAccount user)
        {
            return new UserDTO
            {
                UserId = user.UserId,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Birthday = user.Birthday,
                Location = user.Location,
                Password = user.Password,
                CreatedAt = user.CreatedAt,
            };
        }

        public async Task CreateFirstUser()
        {
            var hasCreated = await uow.userRepository.HasCreated();
            if (hasCreated) return;

            var userName = configuration[ConfigurationConstants.FIRST_APP_TIME_USER_USERNAME]
                ?? throw new Exception(ResponseConstants.ConfigurationPropertyNotFound(ConfigurationConstants.FIRST_APP_TIME_USER_USERNAME));

            var displayName = configuration[ConfigurationConstants.FIRST_APP_TIME_USER_DISPLAYNAME]
                ?? throw new Exception(ResponseConstants.ConfigurationPropertyNotFound(ConfigurationConstants.FIRST_APP_TIME_USER_DISPLAYNAME));

            var email = configuration[ConfigurationConstants.FIRST_APP_TIME_USER_EMAIL]
                ?? throw new Exception(ResponseConstants.ConfigurationPropertyNotFound(ConfigurationConstants.FIRST_APP_TIME_USER_EMAIL));

            var password = configuration[ConfigurationConstants.FIRST_APP_TIME_USER_PASSWORD]
                ?? throw new Exception(ResponseConstants.ConfigurationPropertyNotFound(ConfigurationConstants.FIRST_APP_TIME_USER_PASSWORD));

            await uow.userRepository.Create(new UserAccount
            {
                UserName = userName,
                DisplayName = displayName,
                Email = email,
                Password = Hasher.HashPassword(password)
            });

            await uow.SaveChangesAsync();
        }

        Task<GenericResponse<List<UserDTO>>> IUserService.GetAll(FilterUserRequest model)
        {
            throw new NotImplementedException();
        }
    }
}