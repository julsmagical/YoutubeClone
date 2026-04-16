using YoutubeClone.Domain.Database.SqlServer;
using YoutubeClone.Domain.Database.SqlServer.Context;
using YoutubeClone.Domain.Interfaces.Repositories;

namespace YoutubeClone.Infraestructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly YoutubeCloneContext context;
        public IUserRepository userRepository { get; set; }

        //constructor con inyección de dependencias
        public UnitOfWork(YoutubeCloneContext _context, IUserRepository _userRepository)
        {
            userRepository = _userRepository;
            context = _context;
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
