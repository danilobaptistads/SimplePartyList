using Microsoft.EntityFrameworkCore;
using SimplePartyList.Core.Entities;
using SimplePartyList.Infrastructure.Data;
using SimplePartyList.Infrastructure.Services;

namespace SimplePartyList.Tests.Services;

public class EventServiceTests
{
    private static SimplePartyListContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SimplePartyListContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new SimplePartyListContext(options);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateEventAndChosenList()
    {
        using var context = CreateContext();
        var eventService = new EventService(context);
        var eventDate = new DateTime(2026, 12, 31, 20, 0, 0);

        var result = await eventService.CreateAsync("admin-1", "Réveillon", eventDate);

        Assert.NotNull(result);
        Assert.Equal("Réveillon", result.Name);
        Assert.Equal("admin-1", result.AdminId);
        Assert.Equal(eventDate, result.Date);
        Assert.NotEqual(Guid.Empty, result.ChosenListId);

        var chosenList = await context.ChosenLists.FindAsync(result.ChosenListId);
        Assert.NotNull(chosenList);
        Assert.Equal(eventDate.AddDays(1), chosenList!.Expire);
        Assert.NotEqual(Guid.Empty, chosenList.ListUrl);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEvent()
    {
        using var context = CreateContext();
        var eventId = Guid.NewGuid();
        context.Events.Add(new Event { EventId = eventId, AdminId = "admin-1", Name = "Festa", Date = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var eventService = new EventService(context);
        var result = await eventService.GetByIdAsync(eventId);

        Assert.NotNull(result);
        Assert.Equal(eventId, result!.EventId);
        Assert.Equal("Festa", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        using var context = CreateContext();
        var eventService = new EventService(context);

        var result = await eventService.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByAdminIdAsync_ShouldReturnEvents()
    {
        using var context = CreateContext();
        var adminId = "admin-1";
        context.Events.AddRange(
            new Event { AdminId = adminId, Name = "Festa 1", Date = DateTime.UtcNow },
            new Event { AdminId = adminId, Name = "Festa 2", Date = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var eventService = new EventService(context);
        var result = await eventService.GetByAdminIdAsync(adminId);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        using var context = CreateContext();
        var eventDate = new DateTime(2026, 6, 15, 20, 0, 0);
        var anEvent = new Event
        {
            AdminId = "admin-1",
            Name = "Festa",
            Date = eventDate
        };
        context.Events.Add(anEvent);
        await context.SaveChangesAsync();

        anEvent.Name = "Festa Atualizada";
        anEvent.Date = new DateTime(2026, 7, 20, 18, 0, 0);

        var eventService = new EventService(context);
        var result = await eventService.UpdateAsync(anEvent);

        Assert.NotNull(result);
        Assert.Equal("Festa Atualizada", result.Name);
        Assert.Equal(new DateTime(2026, 7, 20, 18, 0, 0), result.Date);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRecalculateExpire_WhenDateChanges()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = new DateTime(2026, 6, 16, 20, 0, 0) };
        context.ChosenLists.Add(chosenList);
        var anEvent = new Event
        {
            AdminId = "admin-1",
            Name = "Festa",
            Date = new DateTime(2026, 6, 15, 20, 0, 0),
            ChosenListId = chosenList.ChosenListId
        };
        context.Events.Add(anEvent);
        await context.SaveChangesAsync();

        anEvent.Date = new DateTime(2026, 7, 20, 18, 0, 0);

        var eventService = new EventService(context);
        await eventService.UpdateAsync(anEvent);

        var updatedList = await context.ChosenLists.FindAsync(chosenList.ChosenListId);
        Assert.Equal(new DateTime(2026, 7, 21, 18, 0, 0), updatedList!.Expire);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        using var context = CreateContext();
        var eventService = new EventService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => eventService.UpdateAsync(new Event { EventId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEvent()
    {
        using var context = CreateContext();
        var anEvent = new Event
        {
            AdminId = "admin-1",
            Name = "Festa",
            Date = DateTime.UtcNow
        };
        context.Events.Add(anEvent);
        await context.SaveChangesAsync();

        var eventService = new EventService(context);
        await eventService.DeleteAsync(anEvent);

        Assert.Null(await context.Events.FindAsync(anEvent.EventId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        using var context = CreateContext();
        var eventService = new EventService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => eventService.DeleteAsync(new Event { EventId = Guid.NewGuid() }));
    }
}
