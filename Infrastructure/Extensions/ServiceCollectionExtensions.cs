using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Infrastructure.Configurations;
using Infrastructure.Data;
using Infrastructure.Gateways;
using Infrastructure.Services;
using Application.Interfaces.Gateways;
using Application.Interfaces.Services;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Stripe settings
        services.Configure<StripeSettings>(configuration.GetSection("PaymentGateways:Stripe"));

        // Configure JWT Authentication
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        if (jwtSettings != null)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var claimsPrincipal = context.Principal;
                        var subClaim = claimsPrincipal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                                       ?? claimsPrincipal?.FindFirst("sub");

                        if (subClaim == null || !Guid.TryParse(subClaim.Value, out _))
                        {
                            context.Fail("Token must contain a valid 'sub' claim with a GUID value");
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        }

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<MockGateway>();
        services.AddScoped<StripeGateway>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IWebHookHelper, WebHookStripeHelper>();
        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddPaymentDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL database
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}
