using AdessoWorldLeague.API.Abstractions;
using AdessoWorldLeague.API.Data;
using AdessoWorldLeague.API.Infrastructure;
using AdessoWorldLeague.API.Middleware;
using AdessoWorldLeague.API.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Localization — TR is the default culture (DrawMessages.resx), EN via DrawMessages.en.resx
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Domain services
builder.Services.AddSingleton<ITeamDataProvider, StaticTeamDataProvider>();
builder.Services.AddScoped<IDrawService, DrawService>();

// Centralized exception handling — keeps controllers free of try-catch
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Adesso World League API";
        options.Theme = ScalarTheme.DeepSpace;
    });
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
