using SmartLocate.Commons.Constants;

var builder = DistributedApplication.CreateBuilder(args);

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var mongoDbConnection = Environment.GetEnvironmentVariable("ConnectionStrings__MongoDbConnection");

var daprPubSub = builder.AddDaprPubSub(SmartLocateComponents.PubSub);

var api = builder.AddProject<Projects.SmartLocate_API>(SmartLocateServices.Api);

var buses = builder.AddProject<Projects.SmartLocate_Buses>(SmartLocateServices.Buses);

var busRoutes = builder.AddProject<Projects.SmartLocate_BusRoutes>(SmartLocateServices.BusRoutes);

var students = builder.AddProject<Projects.SmartLocate_Students>(SmartLocateServices.Students);

var identity = builder.AddProject<Projects.SmartLocate_Identity>(SmartLocateServices.Identity);

var busDrivers = builder.AddProject<Projects.SmartLocate_BusDrivers>(SmartLocateServices.BusDrivers);

var adminUsers = builder.AddProject<Projects.SmartLocate_Admin>(SmartLocateServices.AdminUsers);

var infrastructure = builder.AddProject<Projects.SmartLocate_Infrastructure>(SmartLocateServices.Infrastructure);

var search = builder.AddProject<Projects.SmartLocate_Search>(SmartLocateServices.Search);

api.WithEnvironment("JWT_SECRET", jwtSecret);

buses
    .WithEnvironment("JWT_SECRET", jwtSecret)
    .WithEnvironment("ConnectionStrings__MongoDbConnection", mongoDbConnection)
    .WithDaprSidecar();

busRoutes
    .WithReference(students)
    .WithEnvironment("JWT_SECRET", jwtSecret)
    .WithEnvironment("ConnectionStrings__MongoDbConnection", mongoDbConnection)
    .WithDaprSidecar();

students
    .WithReference(busRoutes)
    .WithReference(daprPubSub)
    .WithEnvironment("JWT_SECRET", jwtSecret)
    .WithEnvironment("ConnectionStrings__MongoDbConnection", mongoDbConnection)
    .WithDaprSidecar();

busDrivers
    .WithReference(daprPubSub)
    .WithEnvironment("JWT_SECRET", jwtSecret)
    .WithEnvironment("ConnectionStrings__MongoDbConnection", mongoDbConnection)
    .WithDaprSidecar();

adminUsers
    .WithEnvironment("JWT_SECRET", jwtSecret)
    .WithEnvironment("ConnectionStrings__MongoDbConnection", mongoDbConnection)
    .WithDaprSidecar();

identity
    .WithReference(students)
    .WithReference(busDrivers)
    .WithReference(adminUsers)
    .WithReference(daprPubSub)
    .WithEnvironment("JWT_SECRET", jwtSecret)
    .WithEnvironment("ConnectionStrings__MongoDbConnection", mongoDbConnection)
    .WithDaprSidecar();

infrastructure
    .WithReference(daprPubSub)
    .WithEnvironment("JWT_SECRET", jwtSecret)
    .WithDaprSidecar();

search
    .WithEnvironment("JWT_SECRET", jwtSecret)
    .WithEnvironment("ConnectionStrings__MongoDbConnection", mongoDbConnection)
    .WithDaprSidecar();

await builder.Build().RunAsync();