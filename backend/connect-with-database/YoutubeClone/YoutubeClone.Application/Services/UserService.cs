using System.Globalization;
using YoutubeClone.Application.Helpers;
using YoutubeClone.Application.Interfaces.Services;
using YoutubeClone.Application.Models.DTOS;
using YoutubeClone.Application.Models.Requests.User;
using YoutubeClone.Application.Models.Responses;
using YoutubeClone.Domain.Database.SqlServer.Entities;
using YoutubeClone.Domain.Exceptions;
using YoutubeClone.Domain.Interfaces.Repositories;
using YoutubeClone.Shared.Constants;
using YoutubeClone.Shared.Helpers;

namespace YoutubeClone.Application.Services
{
    public class UserService(IUserRepository repository) : IUserService
    {
        public async Task<GenericResponse<UserDTO>> Create(CreateUserRequest model)
        {
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

            var create = await repository.Create(new UserAccount
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

            return ResponseHelper.Create(Map(create), "Usuario creado correctamente.");
        }

        public async Task<GenericResponse<bool>> Delete(Guid id)
        {
            var user = await GetUser(id);

            user.DeletedAt = DateTimeHelper.UtcNow();

            await repository.Update(user);

            return ResponseHelper.Create(true);
        }

        public async Task<GenericResponse<List<UserDTO>>> GetAll(FilterUserRequest model)
        {
            var queryable = repository.Queryable();

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
            //actualizar updatedAt cuando el campo este disponible en la entidad

            await repository.Update(user);

            return ResponseHelper.Create(Map(user));
        }

        private async Task<UserAccount> GetUser(Guid id)
        {
            return await repository.Get(id)
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
    }
}