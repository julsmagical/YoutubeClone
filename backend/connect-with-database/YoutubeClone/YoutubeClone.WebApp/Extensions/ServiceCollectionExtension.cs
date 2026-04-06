using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using YoutubeClone.Application.Helpers;
using YoutubeClone.Application.Interfaces.Services;
using YoutubeClone.Application.Services;
using YoutubeClone.Domain.Database.SqlServer.Context;
using YoutubeClone.Domain.Interfaces.Repositories;
using YoutubeClone.Infraestructure.Persistence.SqlServer.Repositories;
using YoutubeClone.Shared.Constants;
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

        public static void AddLogging(this IServiceCollection services)
        {
            services.AddSerilog();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(connectionString: "Server=localhost,1433;User=sa;Password=Admin1234@;Database=YoutubeClone;TrustServerCertificate=True;",
                sinkOptions: new MSSqlServerSinkOptions { TableName = "LogEvents", AutoCreateSqlTable = true })
                //File
                .WriteTo.Console()
                //Archivos
                .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "logs", "log.txt"), rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        /// <summary>
        /// Agrega lo que la api necesita para funcionar
        /// </summary>
        /// <param name="services"></param>
        public static void AddCore(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = (errorContext) =>
                {
                    var errors = errorContext.ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage).ToList()).ToList();
                    var response = ResponseHelper.Create(
                        data: ValidationConstants.VALIDATION_MESSAGE,
                        errors: errors,
                        message: ValidationConstants.VALIDATION_MESSAGE);
                    return new BadRequestObjectResult(response);
                };
            });
            services.AddOpenApi();
            services.AddSqlServer<YoutubeCloneContext>(configuration.GetConnectionString("Database"));

            services.AddRepositories();
            services.AddServices();
            services.AddMiddlewares();

            AddLogging(services);
        }
    }
}
