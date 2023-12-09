using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<DataContext>(o => {
                o.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });
            services.AddCors();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<LogUserActivity>();
            services.AddScoped<IMessageRepository, MessageRepository>();

            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

            services.AddSignalR();

            services.AddSingleton<PresenceTracker>();

            return services;
        }
    }
}
