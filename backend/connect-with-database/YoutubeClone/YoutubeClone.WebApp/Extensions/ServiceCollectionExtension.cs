using YoutubeClone.Application.Interfaces.Services;
using YoutubeClone.Application.Services;
using YoutubeClone.Domain.Database.SqlServer.Context;
using YoutubeClone.Domain.Interfaces.Repositories;
using YoutubeClone.Infraestructure.Persistence.SqlServer;
using YoutubeClone.WebApp.Middlewares;

namespace YoutubeClone.WebApp.Extensions
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Método para añadir todos los servicios de la aplicación
        /// </summary>
        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
        }

        /// <summary>
        /// Metodo para añadir los repositorios de la aplicación
        /// </summary>
        /// <param name="services"></param>
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
        }

        /// <summary>
        /// Metodo para añadir middlewares
        /// </summary>
        /// <param name="services"></param>
        public static void AddMiddlewares(this IServiceCollection services)
        {
            services.AddScoped<ErrorHandleMiddleware>();
        }

        /// <summary>
        /// Agrega lo que la api necesita para funcionar
        /// </summary>
        /// <param name="services"></param>
        public static void AddCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddOpenApi();
            services.AddSqlServer<YoutubeCloneContext>(configuration.GetConnectionString("Database"));

            services.AddRepositories();
            services.AddServices();
            services.AddMiddlewares();
        }
    }
}
