using Microsoft.OpenApi.Models;
using SmartLocate.Admin.Entities;
using SmartLocate.Admin.Services;
using SmartLocate.Infrastructure.Commons.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.AddMongoDBClient("MongoDbConnection");

builder.Services.AddMongoRepository<AdminUser>();

builder.Services.AddJwtAuthentication();

builder.Services.AddCurrentUserService();

builder.Services.AddScoped<DataSeeder>();

builder.Services.AddDaprClient();

builder.Services.AddControllers().AddDapr();

builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartLocate.Admin", Version = "v1" });
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
app.MapSubscribeHandler();

app.UseSwagger();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartLocate.Admin.v1"));

app.Services.CreateScope().ServiceProvider.GetRequiredService<DataSeeder>().SeedFirstAdminUser();

app.Run();