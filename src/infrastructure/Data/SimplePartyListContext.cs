using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimplePartyList.Core.Entities;

namespace SimplePartyList.Infrastructure.Data;

public class SimplePartyListContext : IdentityDbContext<Admin>
{
    public SimplePartyListContext(DbContextOptions<SimplePartyListContext> options)
        : base(options) { }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<ChosenList> ChosenLists => Set<ChosenList>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Chosen> Chosens => Set<Chosen>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Admin>(entity =>
        {
            entity.Property(a => a.Name).IsRequired();
        });

        builder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId);

            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Date).HasColumnType("TEXT");

            entity.HasOne<Admin>()
                  .WithMany(a => a.Events)
                  .HasForeignKey(e => e.AdminId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<ChosenList>()
                  .WithOne()
                  .HasForeignKey<Event>(e => e.ChosenListId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ChosenList>(entity =>
        {
            entity.HasKey(cl => cl.ChosenListId);

            entity.Property(cl => cl.ListUrl).IsRequired();
            entity.Property(cl => cl.Expire).HasColumnType("TEXT");

            entity.HasIndex(cl => cl.ListUrl).IsUnique();
        });

        builder.Entity<Item>(entity =>
        {
            entity.HasKey(i => i.ItemId);

            entity.Property(i => i.Name).IsRequired();

            entity.HasOne<ChosenList>()
                  .WithMany(cl => cl.Items)
                  .HasForeignKey(i => i.ChosenListId)
                  .OnDelete(DeleteBehavior.Cascade);

        });

        builder.Entity<Chosen>(entity =>
        {
            entity.HasKey(c => c.ChosenId);

            entity.Property(c => c.GuestName).IsRequired();
            entity.Property(c => c.ItemName).IsRequired();

            entity.HasOne<ChosenList>()
                  .WithMany(cl => cl.Chosens)
                  .HasForeignKey(c => c.ChosenListId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
