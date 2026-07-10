using ECommerce.API.Extensions;
using ECommerce.API.Filters;
using ECommerce.Application.DependencyInjection;
using ECommerce.Infrastructure.DependencyInjection;
using ECommerce.Infrastructure.Persistence.Seeders;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration).AddApplication().AddSwaggerGenExt();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});


builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

await RoleSeeder.SeedAsync(app.Services);

app.UseGlobalExceptionMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExt();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
