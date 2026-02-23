using WorldLeague.API.Data;
using WorldLeague.API.DTOs;
using WorldLeague.API.Services;
using Microsoft.EntityFrameworkCore;

namespace WorldLeague.API.Infrastructure;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();

        if (await db.DrawSessions.AnyAsync())
            return;

        var drawService = services.GetRequiredService<IDrawService>();

        await drawService.PerformDrawAsync(new DrawRequest
        {
            GroupCount = 8,
            DrawerFirstName = "World",
            DrawerLastName = "Admin"
        });

        await drawService.PerformDrawAsync(new DrawRequest
        {
            GroupCount = 4,
            DrawerFirstName = "League",
            DrawerLastName = "Manager"
        });
    }
}
