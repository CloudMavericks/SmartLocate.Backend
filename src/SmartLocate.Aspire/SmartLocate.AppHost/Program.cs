using SmartLocate.Commons.Constants;

var builder = DistributedApplication.CreateBuilder(args);

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");

var daprPubSub = builder.AddDaprPubSub(SmartLocateComponents.PubSub);

var api = builder.AddProject<Projects.SmartLocate_API>(SmartLocateServices.Api);

var buses = builder.AddProject<Projects.SmartLocate_Buses>(SmartLocateServices.Buses);

var busRoutes = builder.AddProject<Projects.SmartLocate_BusRoutes>(SmartLocateServices.BusRoutes);

var students = builder.AddProject<Projects.SmartLocate_Students>(SmartLocateServices.Students);

var identity = builder.AddProject<Projects.SmartLocate_Identity>(SmartLocateServices.Identity);

var infrastructure = builder.AddProject<Projects.SmartLocate_Infrastructure>(SmartLocateServices.Infrastructure);

api.WithEnvironment("JWT_SECRET", jwtSecret);

buses.WithDaprSidecar()
    .WithEnvironment("JWT_SECRET", jwtSecret);

busRoutes
    .WithReference(students)
    .WithDaprSidecar()
    .WithEnvironment("JWT_SECRET", jwtSecret);

students
    .WithReference(busRoutes)
    .WithReference(daprPubSub)
    .WithDaprSidecar()
    .WithEnvironment("JWT_SECRET", jwtSecret);

identity
    .WithReference(students)
    .WithReference(daprPubSub)
    .WithDaprSidecar()
    .WithEnvironment("JWT_SECRET", jwtSecret);

infrastructure
    .WithReference(daprPubSub)
    .WithDaprSidecar()
    .WithEnvironment("JWT_SECRET", jwtSecret);

await builder.Build().RunAsync();