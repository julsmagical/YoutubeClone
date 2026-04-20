using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YoutubeClone.Application.Interfaces.Services;
using YoutubeClone.Application.Models.DTOS;
using YoutubeClone.Application.Models.Requests.User;
using YoutubeClone.Application.Models.Responses;
using YoutubeClone.Domain.Exceptions;
using YoutubeClone.Shared.Constants;
using YoutubeClone.WebApp.Helpers;

namespace YoutubeClone.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Sistema")]
        [EndpointSummary("Crear un usuario")]
        [EndpointDescription("Realiza la creación de un usuario")]
        [ProducesResponseType<GenericResponse<UserDTO>>(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest model)
        {
            var rsp = await userService.Create(model);
            return Ok(rsp);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Sistema, Admin")]
        [EndpointSummary("Elimina un usuario")]
        [EndpointDescription("Elimina un usuario")]
        [ProducesResponseType<GenericResponse<bool>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var rsp = await userService.Delete(id);
            return Ok(rsp);
        }

        [HttpGet]
        [Authorize]
        [EndpointSummary("Obtiene uno o más usuarios")]
        [EndpointDescription("Realiza la petición para obtener uno o más usuarios")]
        [ProducesResponseType<GenericResponse<List<UserDTO>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] FilterUserRequest model)
        {
            var rsp = userService.GetAll(model);
            return Ok(rsp);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        [EndpointSummary("Obtener un usuario")]
        [EndpointDescription("Obtiene la información de un usuario")]
        [ProducesResponseType<GenericResponse<UserDTO>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var rsp = await userService.GetById(id);
            return Ok(rsp);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "CreadorContenido, Usuario")]
        [EndpointSummary("Actualizar un usuario")]
        [EndpointDescription("Actualiza la información de un usuario")]
        [ProducesResponseType<GenericResponse<UserDTO>>(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update([FromBody] UpdateUserRequest model, Guid id)
        {
            var srv = await userService.Update(id, model, UserClaim());
            return ResponseStatus.Updated(HttpContext, srv);
        }

        private Claim UserClaim()
        {
            return User.FindFirst(ClaimsConstants.USERACCOUNT_ID)
                ?? throw new BadRequestException(ResponseConstants.AUTH_CLAIM_USER_NOT_FOUND);
        }
    }
}
