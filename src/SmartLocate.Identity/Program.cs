using Microsoft.OpenApi.Models;
using SmartLocate.Identity.Entities;
using SmartLocate.Identity.Settings;
using SmartLocate.Infrastructure.Commons.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.AddMongoDBClient("MongoDbConnection");

builder.Services
    .AddMongoRepository<StudentActivationCode>()
    .AddMongoRepository<BusDriverActivationCode>()
    .AddMongoRepository<AdminUserRefreshToken>()
    .AddMongoRepository<BusDriverRefreshToken>()
    .AddMongoRepository<StudentRefreshToken>();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddDaprClient();

builder.Services.SetupDaprSidekick(builder.Configuration);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));

builder.Services.AddControllers().AddDapr();

builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartLocate.Identity", Version = "v1" });
    x.AddSecurityDefinitionAndRequirement();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCloudEvents();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartLocate.Identity.v1"));

await app.RunAsync();