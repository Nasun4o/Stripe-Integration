using DataAccessLayer;
using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StripeIntegration.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    //WithOrigins("https/")
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });
        }

        public static void ConfigureMsSqlContext(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["DefaultConnection:ConnectionString"];
            services.AddDbContext<StripeIntegrationContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }

        public static void ConfigureRepositoryWrapper(this IServiceCollection services)
        {
            services.AddScoped<IData, Data>();
        }
    }
}
