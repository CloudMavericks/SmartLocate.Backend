# SmartLocate.Infrastructure.Commons

This project is a shared library that contains common code that is used by other microservices in the SmartLocate application.

It primarily contains the following:

- `CurrentUserService` - A service that provides the current user's details to the application.
- `MongoDbRepository<TEntity>` - A generic repository implementation of the `IRepository<TEntity>` interface that provides basic CRUD operations for a MongoDB database.
- `IServiceCollection` extension methods - Extensions for JwtBearer authentication, MongoDB, and other services.
- `SwaggerGenOptions` extension methods - Extensions for SwaggerGen options to add JWT authentication and other options to the Swagger UI.

