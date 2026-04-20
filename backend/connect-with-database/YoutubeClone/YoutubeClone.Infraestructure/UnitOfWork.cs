using YoutubeClone.Domain.Database.SqlServer;
using YoutubeClone.Domain.Database.SqlServer.Context;
using YoutubeClone.Domain.Interfaces.Repositories;

namespace YoutubeClone.Infraestructure
{
    public class UnitOfWork(YoutubeCloneContext context, IUserRepository userRepository, IEmailTemplateRepository emailTemplateRepository, IRoleRepository roleRepository) : IUnitOfWork
    {
        private readonly YoutubeCloneContext _context = context;
        public IUserRepository userRepository { get; set; } = userRepository;
        public IEmailTemplateRepository emailTemplateRepository { get; set; }
        public IRoleRepository roleRepository { get; set; } = roleRepository;

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
