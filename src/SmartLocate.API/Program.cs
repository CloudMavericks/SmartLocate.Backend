using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using SmartLocate.Infrastructure.Commons.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(action => action.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddOcelot();

builder.Services.AddControllers();

builder.Services.AddSwaggerForOcelot(builder.Configuration, 
    x => x.AddAuthenticationProviderKeyMapping(JwtBearerDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme));

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerForOcelotUI();

await app.UseOcelot();

app.Run();
