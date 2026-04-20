using YoutubeClone.Application.Models.DTOs;

namespace YoutubeClone.Application.Interfaces.Services
{
    public interface IEmailTemplateService
    {
        Task<EmailTemplateDTO> Get(string name, Dictionary<string, string> variables);
        Task Init();
    }
}
