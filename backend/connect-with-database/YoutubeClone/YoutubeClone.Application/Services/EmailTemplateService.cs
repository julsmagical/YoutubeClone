using YoutubeClone.Application.Interfaces.Services;
using YoutubeClone.Application.Models.DTOs;
using YoutubeClone.Application.Models.Services;
using YoutubeClone.Domain.Database.SqlServer;

namespace YoutubeClone.Application.Services
{
    public class EmailTemplateService(IUnitOfWork uow, EmailTemplateData data) : IEmailTemplateService
    {
        public async Task<EmailTemplateDTO> Get(string name, Dictionary<string, string> variables)
        {
            var template = data.Data.First(x => x.Name == name);

            foreach (var variable in variables)
            {
                template.Body = template.Body.Replace("{{" + variable.Key + "}}", variable.Value);
            }

            return new EmailTemplateDTO
            {
                Body = template.Body,
                Subject = template.Subject,
            };
        }

        public async Task Init()
        {
            var templates = await uow.emailTemplateRepository.Get();
            data.Data = templates;
        }
    }
}
