using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SmartLocate.Commons.Extensions;
using SmartLocate.Infrastructure.Commons.Contracts;
using SmartLocate.Infrastructure.Commons.Repositories;

namespace SmartLocate.Infrastructure.Commons.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddSingleton(_ =>
        {
            var connectionString = configuration.GetConnectionString("MongoDbConnection");
            var mongoUrl = new MongoUrl(connectionString);
            var client = new MongoClient(mongoUrl);
            return client.GetDatabase(mongoUrl.DatabaseName);
        });
    }
    
    public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services) where T : class, IEntity
    {
        return services.AddSingleton<IMongoRepository<T>>(provider =>
        {
            var mongoDatabase = provider.GetRequiredService<IMongoDatabase>();
            return new MongoRepository<T>(mongoDatabase, typeof(T).GetCollectionName());
        });
    } 
}