using WorldLeague.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace WorldLeague.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DrawSession> DrawSessions => Set<DrawSession>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Team> Teams => Set<Team>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DrawSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DrawerFirstName).IsRequired().HasMaxLength(100);
            e.Property(x => x.DrawerLastName).IsRequired().HasMaxLength(100);
            e.Property(x => x.DrawnAt).IsRequired();
            e.Property(x => x.GroupCount).IsRequired();

            e.HasMany(x => x.Groups)
             .WithOne(x => x.DrawSession)
             .HasForeignKey(x => x.DrawSessionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.ToTable("draw_sessions");
        });

        modelBuilder.Entity<Group>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(1);

            e.HasMany(x => x.Teams)
             .WithOne(x => x.Group)
             .HasForeignKey(x => x.GroupId)
             .OnDelete(DeleteBehavior.Cascade);

            e.ToTable("groups");
        });

        modelBuilder.Entity<Team>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Country).IsRequired().HasMaxLength(50);

            e.ToTable("teams");
        });
    }
}
