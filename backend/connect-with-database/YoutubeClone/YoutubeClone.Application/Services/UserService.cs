using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Security.Claims;
using YoutubeClone.Application.Helpers;
using YoutubeClone.Application.Interfaces.Services;
using YoutubeClone.Application.Models.DTOs;
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
    public class UserService(IUnitOfWork uow, IConfiguration configuration, SMTP smtp, IEmailTemplateService emailTemplateService) : IUserService
    {
        public async Task<GenericResponse<UserDTO>> Create(CreateUserRequest model, Claim claim)
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

            var executor = await GetExecutor(claim.Value);

            if (model.RoleId == Guid.Empty)
            {
                throw new NotFoundException(ValidationConstants.IsEmpty("RoleId"));
            }

            await ValidateEmailIfExists(model.Email);

            var password = Generate.RandomText(32);

            var roleToAssign = await ValidateRole(executor, model.RoleId);

            var create = await uow.userRepository.Create(new UserAccount
            {
                UserId = Guid.NewGuid(),
                UserName = model.UserName.ToLower(),
                DisplayName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.DisplayName.ToLower()),
                Email = model.Email.ToLower(),
                Birthday = model.Birthday,
                Location = model.Location,
                Password = Hasher.HashPassword(password), //cambio a hash
                CreatedAt = DateTimeHelper.UtcNow(),
                DeletedAt = null,
                UserAccountRoles = [
                    new UserAccountRole {
                        RoleId = roleToAssign.RoleId,
                        AssignedBy = executor.UserId
                    }
                ]
            });

            var template = await emailTemplateService.Get(EmailTemplateNameConstants.USER_REGISTER, new Dictionary<string, string>
            {
                { "password", password }
            });
            await smtp.Send(model.Email, template.Subject, template.Body);

            await uow.SaveChangesAsync();

            return ResponseHelper.Create(Map(create), [], "Usuario creado correctamente.");
        }

        public async Task<GenericResponse<bool>> Delete(Guid id)
        {
            var user = await GetUser(id);

            user.DeletedAt = DateTimeHelper.UtcNow();

            await uow.userRepository.Update(user);

            await uow.SaveChangesAsync(); //

            return ResponseHelper.Create(true);
        }

        public GenericResponse<List<UserDTO>> GetAll(FilterUserRequest model)
        {
            var queryable = uow.userRepository.Queryable();



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

        public async Task<GenericResponse<UserDTO>> Update(Guid id, UpdateUserRequest model, Claim claim)
        {
            var executor = await GetExecutor(claim.Value);
            var user = await GetUser(id);

            user.UserName = model.UserName ?? user.UserName;
            user.DisplayName = model.DisplayName ?? user.DisplayName;
            user.Email = model.Email ?? user.Email;
            user.Location = model.Location ?? user.Location;

            if (!string.IsNullOrWhiteSpace(model.Email) && user.Email != model.Email)
            {
                await ValidateEmailIfExists(model.Email);
                user.Email = model.Email;
            }

            if (model.RoleId.HasValue)
            {
                var roleToAssign = await ValidateRole(executor, model.RoleId.Value);

                await uow.userRepository.ClearRoles([.. user.UserAccountRoles]);

                user.UserAccountRoles.Add(new UserAccountRole
                {
                    RoleId = roleToAssign.RoleId,
                    AssignedBy = executor.UserId
                });
            }

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
            var role = user.UserAccountRoles.FirstOrDefault()?.Role;

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
                Role = role != null ? new RoleDTO
                {
                    Id = role.RoleId,
                    Name = role.Name,
                    Description = role.Description
                } : null
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

            var adminRole = await uow.roleRepository.Get(x => x.Name == RoleConstants.Admin)
                ?? throw new Exception(ResponseConstants.RoleNotFound(RoleConstants.Admin));

            await uow.userRepository.Create(new UserAccount
            {
                UserName = userName,
                DisplayName = displayName,
                Email = email,
                Password = Hasher.HashPassword(password),
                UserAccountRoles = [
                    new UserAccountRole
                    {
                        RoleId = adminRole.RoleId,
                    }
                ]
            });

            await uow.SaveChangesAsync();
        }

        private async Task<UserAccount> GetExecutor(string value)
        {
            var uuid = Guid.Parse(value);
            return await uow.userRepository.GetById(uuid)
                ?? throw new NotFoundException(ResponseConstants.USER_NOT_EXIST);
        }

        private async Task ValidateEmailIfExists(string email)
        {
            if (await uow.userRepository.IfExists(x => x.Email == email))
            {
                throw new BadRequestException(ResponseConstants.USER_EMAIL_TAKED);
            }
        }

        private async Task<Role> ValidateRole(UserAccount executor, Guid roleId)
        {
            var roleToAssign = await uow.roleRepository.Get(roleId)
                ?? throw new NotFoundException(ResponseConstants.RoleNotFound(roleId));

            if (executor.UserAccountRoles.First().Role.Name == RoleConstants.CreadorContenido && roleToAssign.Name == RoleConstants.Admin)
            {
                throw new BadRequestException(ResponseConstants.CANNOT_ASSIGN_THE_ROLE);
            }

            return roleToAssign;
        }

        public async Task<GenericResponse<UserDTO>> Me(Claim claim)
        {
            var executor = await GetExecutor(claim.Value);
            return ResponseHelper.Create(Map(executor));
        }


    }
}