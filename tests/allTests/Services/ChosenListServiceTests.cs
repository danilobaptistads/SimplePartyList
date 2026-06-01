using Microsoft.EntityFrameworkCore;
using SimplePartyList.Core.Entities;
using SimplePartyList.Infrastructure.Data;
using SimplePartyList.Infrastructure.Services;

namespace SimplePartyList.Tests.Services;

public class ChosenListServiceTests
{
    private static SimplePartyListContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SimplePartyListContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new SimplePartyListContext(options);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateChosenList_WithValidExpiration()
    {
        using var context = CreateContext();
        var eventId = Guid.NewGuid();

        context.Events.Add(new Event
        {
            EventId = eventId,
            AdminId = "admin-1",
            Name = "Festa",
            Date = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new ChosenListService(context);
        var eventDate = new DateTime(2026, 6, 15, 20, 0, 0);
        var result = await service.CreateAsync(eventId, eventDate);

        Assert.NotEqual(Guid.Empty, result.ChosenListId);
        Assert.NotEqual(Guid.Empty, result.ListUrl);
        Assert.Equal(eventDate.AddDays(1), result.Expire);
    }

    [Fact]
    public async Task GetByListUrlAsync_ShouldReturnChosenList_WhenFound()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList
        {
            ChosenListId = Guid.NewGuid(),
            ListUrl = Guid.NewGuid(),
            Expire = DateTime.UtcNow.AddDays(5)
        };
        context.ChosenLists.Add(chosenList);
        await context.SaveChangesAsync();

        var service = new ChosenListService(context);
        var result = await service.GetByListUrlAsync(chosenList.ListUrl);

        Assert.NotNull(result);
        Assert.Equal(chosenList.ListUrl, result.ListUrl);
    }

    [Fact]
    public async Task GetByListUrlAsync_ShouldReturnNull_WhenNotFound()
    {
        using var context = CreateContext();
        var service = new ChosenListService(context);
        var result = await service.GetByListUrlAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task IsExpiredAsync_ShouldReturnTrue_WhenExpired()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList
        {
            ChosenListId = Guid.NewGuid(),
            ListUrl = Guid.NewGuid(),
            Expire = DateTime.UtcNow.AddDays(-1)
        };
        context.ChosenLists.Add(chosenList);
        await context.SaveChangesAsync();

        var service = new ChosenListService(context);
        var result = await service.IsExpiredAsync(chosenList.ListUrl);

        Assert.True(result);
    }

    [Fact]
    public async Task IsExpiredAsync_ShouldReturnFalse_WhenNotExpired()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList
        {
            ChosenListId = Guid.NewGuid(),
            ListUrl = Guid.NewGuid(),
            Expire = DateTime.UtcNow.AddDays(30)
        };
        context.ChosenLists.Add(chosenList);
        await context.SaveChangesAsync();

        var service = new ChosenListService(context);
        var result = await service.IsExpiredAsync(chosenList.ListUrl);

        Assert.False(result);
    }

    [Fact]
    public async Task GetByAdminIdAsync_ShouldReturnLists_ForGivenAdmin()
    {
        using var context = CreateContext();
        var adminId = "admin-test-123";

        var list1 = new ChosenList
        {
            ChosenListId = Guid.NewGuid(),
            ListUrl = Guid.NewGuid(),
            Expire = DateTime.UtcNow.AddDays(5)
        };
        var list2 = new ChosenList
        {
            ChosenListId = Guid.NewGuid(),
            ListUrl = Guid.NewGuid(),
            Expire = DateTime.UtcNow.AddDays(10)
        };

        context.ChosenLists.AddRange(list1, list2);
        context.Events.AddRange(
            new Event { EventId = Guid.NewGuid(), AdminId = adminId, Name = "Evento 1", Date = DateTime.UtcNow, ChosenListId = list1.ChosenListId },
            new Event { EventId = Guid.NewGuid(), AdminId = adminId, Name = "Evento 2", Date = DateTime.UtcNow, ChosenListId = list2.ChosenListId }
        );
        await context.SaveChangesAsync();

        var service = new ChosenListService(context);
        var result = await service.GetByAdminIdAsync(adminId);

        Assert.Equal(2, result.Count);
    }
}
