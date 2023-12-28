using Microsoft.OpenApi.Models;
using SmartLocate.BusRoutes.Entities;
using SmartLocate.Infrastructure.Commons.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMongo(builder.Configuration)
    .AddMongoRepository<BusRoute>();

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(x => x.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartLocate.BusRoutes", Version = "v1" }));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartLocate.BusRoutes.v1"));

await app.RunAsync();