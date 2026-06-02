using Microsoft.EntityFrameworkCore;
using SimplePartyList.Core.Entities;
using SimplePartyList.Infrastructure.Data;
using SimplePartyList.Infrastructure.Services;

namespace SimplePartyList.Tests.Integration;

public class PersistenceTests
{
    private static SimplePartyListContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SimplePartyListContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new SimplePartyListContext(options);
    }

    [Fact]
    public async Task CreateAndRetrieve_ChosenList()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        await context.SaveChangesAsync();

        var retrieved = await context.ChosenLists
            .FirstOrDefaultAsync(cl => cl.ListUrl == chosenList.ListUrl);

        Assert.NotNull(retrieved);
        Assert.Equal(chosenList.ChosenListId, retrieved!.ChosenListId);
        Assert.NotEqual(Guid.Empty, retrieved.ListUrl);
    }

    [Fact]
    public async Task Update_Item_PersistsChanges()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        var item = new Item { Name = "Cerveja", MaxQuantity = 50, ChosenListId = chosenList.ChosenListId };
        context.Items.Add(item);
        await context.SaveChangesAsync();

        item.Name = "Cerveja Skol";
        item.MaxQuantity = 100;
        await context.SaveChangesAsync();

        var updated = await context.Items.FindAsync(item.ItemId);
        Assert.Equal("Cerveja Skol", updated!.Name);
        Assert.Equal(100, updated.MaxQuantity);
    }

    [Fact]
    public async Task Delete_Item_RemovesFromDatabase()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        var item = new Item { Name = "Cerveja", MaxQuantity = 50, ChosenListId = chosenList.ChosenListId };
        context.Items.Add(item);
        await context.SaveChangesAsync();

        context.Items.Remove(item);
        await context.SaveChangesAsync();

        var deleted = await context.Items.FindAsync(item.ItemId);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task CascadeDelete_ChosenList_RemovesItemsAndChosens()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        context.Items.Add(new Item { Name = "Cerveja", MaxQuantity = 50, ChosenListId = chosenList.ChosenListId });
        context.Chosens.Add(new Chosen { GuestName = "João", ItemName = "Cerveja", ChosenListId = chosenList.ChosenListId });
        await context.SaveChangesAsync();

        context.ChosenLists.Remove(chosenList);
        await context.SaveChangesAsync();

        Assert.Empty(await context.Items.Where(i => i.ChosenListId == chosenList.ChosenListId).ToListAsync());
        Assert.Empty(await context.Chosens.Where(c => c.ChosenListId == chosenList.ChosenListId).ToListAsync());
    }

    [Fact]
    public async Task SubmitAndCount_Quota()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        var item = new Item { Name = "Cerveja", MaxQuantity = 2, ChosenListId = chosenList.ChosenListId };
        context.Items.Add(item);
        await context.SaveChangesAsync();

        context.Chosens.Add(new Chosen { GuestName = "João", ItemName = "Cerveja", ChosenListId = chosenList.ChosenListId });
        context.Chosens.Add(new Chosen { GuestName = "Maria", ItemName = "Cerveja", ChosenListId = chosenList.ChosenListId });
        await context.SaveChangesAsync();

        var currentCount = await context.Chosens
            .CountAsync(c => c.ItemName == "Cerveja" && c.ChosenListId == chosenList.ChosenListId);

        Assert.Equal(2, currentCount);
        Assert.True(currentCount >= item.MaxQuantity);
    }

    [Fact]
    public async Task GetByChosenListId_ReturnsChosens()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        context.Chosens.AddRange(
            new Chosen { GuestName = "João", ItemName = "Cerveja", ChosenListId = chosenList.ChosenListId },
            new Chosen { GuestName = "Maria", ItemName = "Refrigerante", ChosenListId = chosenList.ChosenListId }
        );
        await context.SaveChangesAsync();

        var chosens = await context.Chosens
            .Where(c => c.ChosenListId == chosenList.ChosenListId)
            .ToListAsync();

        Assert.Equal(2, chosens.Count);
    }

    [Fact]
    public async Task CreateAndRetrieve_Event_CreatesChosenList()
    {
        using var context = CreateContext();
        var eventDate = new DateTime(2026, 12, 31, 20, 0, 0);
        var chosenList = new ChosenList { Expire = eventDate.AddDays(1) };
        context.ChosenLists.Add(chosenList);
        var anEvent = new Event
        {
            AdminId = "admin-1",
            Name = "Réveillon",
            Date = eventDate,
            ChosenListId = chosenList.ChosenListId
        };
        context.Events.Add(anEvent);
        await context.SaveChangesAsync();

        var retrieved = await context.Events
            .FirstOrDefaultAsync(e => e.EventId == anEvent.EventId);

        Assert.NotNull(retrieved);
        Assert.Equal("Réveillon", retrieved!.Name);
        Assert.Equal(eventDate, retrieved.Date);

        var list = await context.ChosenLists.FindAsync(retrieved.ChosenListId);
        Assert.NotNull(list);
        Assert.Equal(eventDate.AddDays(1), list!.Expire);
    }

    [Fact]
    public async Task Update_Event_RecalculatesExpire()
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
        anEvent.Name = "Festa Atualizada";

        var eventService = new EventService(context);
        await eventService.UpdateAsync(anEvent);

        var updatedEvent = await context.Events.FindAsync(anEvent.EventId);
        Assert.Equal("Festa Atualizada", updatedEvent!.Name);
        Assert.Equal(new DateTime(2026, 7, 20, 18, 0, 0), updatedEvent.Date);

        var updatedList = await context.ChosenLists.FindAsync(chosenList.ChosenListId);
        Assert.Equal(new DateTime(2026, 7, 21, 18, 0, 0), updatedList!.Expire);
    }

    [Fact]
    public async Task CascadeDelete_Event_RemovesChosenListAndChosens()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        context.Items.Add(new Item { Name = "Cerveja", MaxQuantity = 50, ChosenListId = chosenList.ChosenListId });
        context.Chosens.Add(new Chosen { GuestName = "João", ItemName = "Cerveja", ChosenListId = chosenList.ChosenListId });
        var anEvent = new Event
        {
            AdminId = "admin-1",
            Name = "Festa",
            Date = DateTime.UtcNow,
            ChosenListId = chosenList.ChosenListId
        };
        context.Events.Add(anEvent);
        await context.SaveChangesAsync();

        var eventService = new EventService(context);
        await eventService.DeleteAsync(anEvent);

        Assert.Null(await context.ChosenLists.FindAsync(chosenList.ChosenListId));
        Assert.Empty(await context.Chosens.Where(c => c.ChosenListId == chosenList.ChosenListId).ToListAsync());
    }

    [Fact]
    public async Task GetEvents_ByAdminId()
    {
        using var context = CreateContext();
        var adminId = "admin-1";
        context.Events.AddRange(
            new Event { AdminId = adminId, Name = "Festa 1", Date = DateTime.UtcNow },
            new Event { AdminId = adminId, Name = "Festa 2", Date = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var events = await context.Events
            .Where(e => e.AdminId == adminId)
            .ToListAsync();

        Assert.Equal(2, events.Count);
    }
}
