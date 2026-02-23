using WorldLeague.API.Abstractions;
using WorldLeague.API.Data;
using WorldLeague.API.Infrastructure;
using WorldLeague.API.Middleware;
using WorldLeague.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Native OpenAPI spec (used by Scalar)
builder.Services.AddOpenApi();

// Swashbuckle (used by Swagger UI)
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "World League API",
        Version = "v1",
        Description = "Randomized group draw API for the World League."
    });
});

// Localization — TR default (DrawMessages.resx), EN via DrawMessages.en.resx
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

// Run migrations and seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    // Scalar — modern API explorer
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "World League API";
        options.Theme = ScalarTheme.DeepSpace;
    });

    // Swagger UI — classic API explorer
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "World League API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseDefaultFiles();   // serves index.html at "/"
app.UseStaticFiles();    // serves wwwroot/
app.UseAuthorization();
app.MapControllers();

app.Run();
