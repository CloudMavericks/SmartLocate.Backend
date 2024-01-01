using Microsoft.OpenApi.Models;
using SmartLocate.BusRoutes.Entities;
using SmartLocate.Infrastructure.Commons.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.AddMongoDBClient("MongoDbConnection");

builder.Services.AddMongoRepository<BusRoute>();

builder.Services.AddJwtAuthentication();

builder.Services.AddDaprClient();

builder.Services.AddControllers().AddDapr();

builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartLocate.BusRoutes", Version = "v1" });
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

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartLocate.BusRoutes.v1"));

await app.RunAsync();