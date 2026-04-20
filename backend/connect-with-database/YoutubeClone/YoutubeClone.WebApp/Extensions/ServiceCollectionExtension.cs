using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using YoutubeClone.Application.Helpers;
using YoutubeClone.Application.Interfaces.Services;
using YoutubeClone.Application.Models.Services;
using YoutubeClone.Application.Services;
using YoutubeClone.Domain.Database.SqlServer;
using YoutubeClone.Domain.Database.SqlServer.Context;
using YoutubeClone.Domain.Exceptions;
using YoutubeClone.Domain.Interfaces.Repositories;
using YoutubeClone.Infraestructure;
using YoutubeClone.Infraestructure.Persistence.SqlServer.Repositories;
using YoutubeClone.Shared;
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
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICacheService, CacheService>();
        }

        /// <summary>
        /// Metodo para añadir los repositorios de la aplicación
        /// </summary>
        /// <param name="services"></param>
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddTransient<IUserRepository, UserRepository>();

            services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();

            services.AddScoped<IRoleRepository, RoleRepository>();
        }

        /// <summary>
        /// Metodo para añadir middlewares
        /// </summary>
        /// <param name="services"></param>
        public static void AddMiddlewares(this IServiceCollection services)
        {
            services.AddScoped<ErrorHandleMiddleware>();
        }

        public async static Task AddSMTP(this IServiceCollection services, IConfiguration configuration)
        {
            var host = Environment.GetEnvironmentVariable(EnvironmentConstants.SMTP_HOST)
                ?? throw new Exception(ResponseConstants.ConfigurationPropertyNotFound(ConfigurationConstants.SMTP_HOST));

            var from = Environment.GetEnvironmentVariable(EnvironmentConstants.SMTP_FROM)
                ?? throw new Exception(ResponseConstants.ConfigurationPropertyNotFound(ConfigurationConstants.SMTP_FROM));

            var portValue = Environment.GetEnvironmentVariable(EnvironmentConstants.SMTP_PORT) ??
                configuration[ConfigurationConstants.SMTP_PORT];

            var port = Convert.ToInt32(portValue ?? "587");

            var user = Environment.GetEnvironmentVariable(EnvironmentConstants.SMTP_USER)
                ?? configuration[ConfigurationConstants.SMTP_USER]
                ?? throw new Exception(ResponseConstants.ConfigurationPropertyNotFound(ConfigurationConstants.SMTP_USER));

            var password = Environment.GetEnvironmentVariable(EnvironmentConstants.SMTP_PASSWORD)
                ?? configuration[ConfigurationConstants.SMTP_PASSWORD]
                ?? throw new Exception(ResponseConstants.ConfigurationPropertyNotFound(ConfigurationConstants.SMTP_PASSWORD));

            var smtp = new SMTP(host, from, port, user, password);
            services.AddSingleton(smtp);
        }

        public static void AddLogging(this IServiceCollection services)
        {
            services.AddSerilog();

            Log.Logger = new LoggerConfiguration()
                // File
                .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "logs", "log.txt"), rollingInterval: RollingInterval.Day)
                // Console
                .WriteTo.Console()
                .CreateLogger();
        }

        /// <summary>
        /// Agrega lo que la api necesita para funcionar
        /// </summary>
        /// <param name="services"></param>
        public async static Task AddCore(this IServiceCollection services, IConfiguration configuration)
        {
            await services.AddSMTP(configuration);

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

            var databaseConnectionString = Environment.GetEnvironmentVariable(EnvironmentConstants.CONNECTION_STRING_DATABASE)
                    ?? configuration[ConfigurationConstants.CONNECTION_STRING_DATABASE];

            services.AddSqlServer<YoutubeCloneContext>(databaseConnectionString);

            services.AddRepositories(); //database

            services.AddServices();

            services.AddMiddlewares();

            services.AddLogging(); //serilog

            services.AddAuth(configuration);

            services.AddCache();

            //primer usuario
            await Initialize(services);
        }

        public async static Task Initialize(this IServiceCollection services)
        {
            var templatesData = new EmailTemplateData();
            services.AddSingleton(templatesData);

            var provider = services.BuildServiceProvider();
            var scope = provider.CreateAsyncScope();

            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            await userService.CreateFirstUser();

            var emailTemplateService = scope.ServiceProvider.GetRequiredService<IEmailTemplateService>();
            await emailTemplateService.Init();

        }

        public static void AddAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(builder =>
            {
                builder.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                builder.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(builder =>
            {
                var tokenConfiguration = TokenHelper.Configuration(configuration);

                builder.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = tokenConfiguration.Issuer,
                    ValidateAudience = true,
                    ValidAudience = tokenConfiguration.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = tokenConfiguration.SecurityKey,
                    ClockSkew = TimeSpan.Zero
                };

                builder.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        throw new UnauthorizedException(ResponseConstants.AUTH_TOKEN_NOT_FOUND);
                    }
                };
            });

            services.AddAuthorization();
        }

        public static void AddCache(this IServiceCollection services)
        {
            services.AddMemoryCache();
        }
    }
}
