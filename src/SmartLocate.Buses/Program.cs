using Microsoft.OpenApi.Models;
using SmartLocate.Buses.Entities;
using SmartLocate.Infrastructure.Commons.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMongo(builder.Configuration)
    .AddMongoRepository<Bus>();

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(x => x.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartLocate.Buses", Version = "v1" }));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseRouting();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartLocate.Buses.v1"));

await app.RunAsync();