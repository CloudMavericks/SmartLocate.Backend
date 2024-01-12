using Microsoft.OpenApi.Models;
using SmartLocate.Infrastructure.Commons.Extensions;
using SmartLocate.Infrastructure.Services;
using SmartLocate.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(nameof(EmailSettings)));

builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddDaprClient();

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartLocate.Infrastructure", Version = "v1" });
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
app.UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartLocate.Infrastructure.v1"));

await app.RunAsync();