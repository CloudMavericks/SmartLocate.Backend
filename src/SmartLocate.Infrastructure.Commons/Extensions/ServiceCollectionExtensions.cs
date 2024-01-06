using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using SmartLocate.Commons.Constants;
using SmartLocate.Commons.Extensions;
using SmartLocate.Infrastructure.Commons.Contracts;
using SmartLocate.Infrastructure.Commons.Repositories;
using SmartLocate.Infrastructure.Commons.Services;

namespace SmartLocate.Infrastructure.Commons.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds an implementation of <see cref="IMongoRepository{T}"/> to the service collection where T is the type of the entity that inherits from <see cref="IEntity"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <typeparam name="TEntity">The type of the entity that inherits from <see cref="IEntity"/>.</typeparam>
    public static IServiceCollection AddMongoRepository<TEntity>(this IServiceCollection services) where TEntity : class, IEntity
    {
        return services.AddSingleton<IMongoRepository<TEntity>>(provider =>
        {
            var mongoDatabase = provider.GetRequiredService<IMongoDatabase>();
            return new MongoRepository<TEntity>(mongoDatabase, typeof(TEntity).GetCollectionName());
        });
    }

    /// <summary>
    /// Adds Jwt Authentication services and roles based authorization policies to the service collection. 
    /// </summary>
    /// <param name="services">The service collection.</param>
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
                    RoleClaimType = SmartLocateClaimTypes.Type
                };
            });
        services.AddAuthorizationBuilder()
            .AddPolicy(SmartLocateRoles.SuperAdmin, policy => policy.RequireRole(SmartLocateRoles.SuperAdmin))
            .AddPolicy(SmartLocateRoles.Admin, policy => policy.RequireRole(SmartLocateRoles.Admin, SmartLocateRoles.SuperAdmin))
            .AddPolicy(SmartLocateRoles.Student, policy => policy.RequireRole(SmartLocateRoles.Student))
            .AddPolicy(SmartLocateRoles.BusDriver, policy => policy.RequireRole(SmartLocateRoles.BusDriver))
            .SetDefaultPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        return services;
    }
    
    /// <summary>
    /// Adds an implementation of <see cref="ICurrentUserService"/> along with <see cref="Microsoft.AspNetCore.Http.HttpContextAccessor"/> to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection AddCurrentUserService(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
}