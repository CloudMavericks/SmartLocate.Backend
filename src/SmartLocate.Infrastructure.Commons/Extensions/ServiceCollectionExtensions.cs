using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using SmartLocate.Commons.Constants;
using SmartLocate.Commons.Extensions;
using SmartLocate.Infrastructure.Commons.Contracts;
using SmartLocate.Infrastructure.Commons.Repositories;

namespace SmartLocate.Infrastructure.Commons.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services) where T : class, IEntity
    {
        return services.AddSingleton<IMongoRepository<T>>(provider =>
        {
            var mongoDatabase = provider.GetRequiredService<IMongoDatabase>();
            return new MongoRepository<T>(mongoDatabase, typeof(T).GetCollectionName());
        });
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if(string.IsNullOrEmpty(jwtSecret))
            throw new Exception("JWT_SECRET environment variable is not set");
        services.AddAuthentication()
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
                };
            });
        services.AddAuthorizationBuilder()
            .AddPolicy(SmartLocateRoles.Admin, policy => policy.RequireClaim(SmartLocateClaimTypes.Type, SmartLocateRoles.Admin))
            .AddPolicy(SmartLocateRoles.Student, policy => policy.RequireClaim(SmartLocateClaimTypes.Type, SmartLocateRoles.Student))
            .AddPolicy(SmartLocateRoles.BusDriver, policy => policy.RequireClaim(SmartLocateClaimTypes.Type, SmartLocateRoles.BusDriver))
            .SetDefaultPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        return services;
    }
}