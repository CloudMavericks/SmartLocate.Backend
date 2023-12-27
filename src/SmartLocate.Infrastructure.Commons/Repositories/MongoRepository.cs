using System.Linq.Expressions;
using MongoDB.Driver;
using SmartLocate.Infrastructure.Commons.Contracts;

namespace SmartLocate.Infrastructure.Commons.Repositories;

public class MongoRepository<T>(IMongoDatabase mongoDatabase, string collectionName) : IMongoRepository<T>
    where T : class, IEntity
{
    private readonly IMongoCollection<T> _collection = mongoDatabase.GetCollection<T>(collectionName);

    public Task CreateAsync(T entity)
    {
        entity.Id = Guid.NewGuid();
        return _collection.InsertOneAsync(entity);
    }

    public Task<T> GetAsync(Guid id)
    {
        return _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public Task<List<T>> GetAllAsync()
    {
        return _collection.Find(x => true).ToListAsync();
    }

    public Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter)
    {
        return _collection.Find(filter).ToListAsync();
    }

    public Task<List<T>> GetAllAsync(int skip, int take, Expression<Func<T, object>> orderBy, bool orderByDescending = false)
    {
        return orderByDescending
            ? _collection.Find(x => true)
                .SortByDescending(orderBy)
                .Skip(skip)
                .Limit(take)
                .ToListAsync()
            : _collection.Find(x => true)
                .SortBy(orderBy)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();
    }

    public Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter, int skip, int take, Expression<Func<T, object>> orderBy, bool orderByDescending = false)
    {
        return orderByDescending
            ? _collection.Find(filter)
                .SortByDescending(orderBy)
                .Skip(skip)
                .Limit(take)
                .ToListAsync()
            : _collection.Find(filter)
                .SortBy(orderBy)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();
    }

    public Task UpdateAsync(T entity)
    {
        return _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
    }

    public Task RemoveAsync(Guid id)
    {
        return _collection.DeleteOneAsync(x => x.Id == id);
    }
}