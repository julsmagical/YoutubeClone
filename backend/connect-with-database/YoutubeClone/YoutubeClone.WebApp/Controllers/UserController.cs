using Microsoft.AspNetCore.Mvc;
using YoutubeClone.Application.Interfaces.Services;
using YoutubeClone.Application.Models.Requests.User;

namespace YoutubeClone.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest model)
        {
            var rsp = await userService.Create(model);
            return Ok(rsp);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var rsp = await userService.Delete(id);
            return Ok(rsp);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] FilterUserRequest model)
        {
            var rsp = await userService.GetAll(model);
            return Ok(rsp);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var rsp = await userService.GetById(id);
            return Ok(rsp);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateUserRequest model)
        {
            var rsp = await userService.Update(id, model);
            return Ok(rsp);
        }
    }
}
