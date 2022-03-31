using Entities;
using Entities.EntityModels;
using Entities.Utils.Models;
using Interfaces.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Services;
using Stripe;
using StripeIntegration.Extensions;
using System.Text;

namespace StripeIntegration
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureCors();
            services.ConfigureMsSqlContext(Configuration);
            services.ConfigureRepositoryWrapper();
            services.AddControllers();
            services.AddAutoMapper(typeof(Startup));

            // Enable Swagger   
            services.AddSwaggerGen(swagger =>
            {
                //This is to generate the Default UI of Swagger Documentation  
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "JWT Token Stripe Integration API",
                    Description = "Stripe Integration"
                });
                // To Enable authorization using Swagger (JWT)  
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer ' and then your token in the input below.",
                });
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}

                    }
                });
            });

            services.AddScoped<IPaymentIntentServiceWrapper, PaymentIntentServiceWrapper>(r =>
            {
                var service = new PaymentIntentService();
                return new PaymentIntentServiceWrapper(service);
            });

            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IStripePaymentService, StripePaymentService>();


            services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
               {
                   options.Password.RequireDigit = false;
                   options.Password.RequiredLength = 3;
                   options.Password.RequiredUniqueChars = 0;
                   options.Password.RequireLowercase = false;
                   options.Password.RequireNonAlphanumeric = false;
                   options.Password.RequireUppercase = false;
                   options.SignIn.RequireConfirmedEmail = false;
                   options.User.RequireUniqueEmail = true;
               })
               .AddEntityFrameworkStores<StripeIntegrationContext>()
               .AddDefaultTokenProviders();

            services.Configure<JwtSettings>(this.Configuration.GetSection("JwtSettings"));
            services.Configure<StripeSettings>(this.Configuration.GetSection("StripeSettings"));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration.GetSection("JwtSettings")["Secret"])),
                    ValidateAudience = false,
                    ValidAudience = this.Configuration.GetSection("JwtSettings")["Audience"],
                    ValidateIssuer = true,
                    ValidIssuer = this.Configuration.GetSection("JwtSettings")["Issuer"],
                    ValidateLifetime = true
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BasicAuth v1"));
            }

            app.UseHttpsRedirection();

            app.UseCors("CorsPolicy");

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
