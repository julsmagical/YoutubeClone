using Microsoft.AspNetCore.Mvc;
using YoutubeClone.Application.Interfaces.Services;
using YoutubeClone.Application.Models.Requests.Auth;
using YoutubeClone.Application.Models.Responses;
using YoutubeClone.Application.Models.Responses.Auth;

namespace YoutubeClone.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService service) : ControllerBase
    {
        [HttpPost("login")]
        [EndpointSummary("Iniciar sesion como usuario")]
        [EndpointDescription("Esto le permite iniciar sesion en el aplicativo. Genera dos tokens, uno que es el JWT para la autenticacion y el otro para la renovacion")]
        [ProducesResponseType<GenericResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<GenericResponse<LoginAuthResponse>>(StatusCodes.Status200OK)]
        [Tags("auth", "collaborators", "JWT", "refresh_token")]
        public async Task<IActionResult> Login([FromBody] LoginAuthRequest model)
        {
            var srv = await service.Login(model);
            return Ok(srv);
        }

        [HttpPost("renew")]
        [EndpointSummary("Renovar sesion como usuario")]
        [EndpointDescription("Esto le permite renovar sesion en el aplicativo. Genera dos tokens, uno que es el JWT para la autenticacion y el otro para la renovacion")]
        [ProducesResponseType<GenericResponse<string>>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<GenericResponse<LoginAuthResponse>>(StatusCodes.Status200OK)]
        [Tags("auth", "collaborators", "JWT", "refresh_token")]
        public async Task<IActionResult> Renew([FromBody] RenewAuthRequest model)
        {
            var srv = await service.Renew(model);
            return Ok(srv);
        }
    }
}
