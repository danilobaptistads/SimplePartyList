using Microsoft.EntityFrameworkCore;
using Moq;
using SimplePartyList.Core.Entities;
using SimplePartyList.Infrastructure.Data;

namespace SimplePartyList.Tests.Services;

public class ChosenListServiceTests
{
    private static Mock<SimplePartyListContext> CreateMockContext()
    {
        return new Mock<SimplePartyListContext>(new DbContextOptions<SimplePartyListContext>());
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        return mockSet;
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateChosenList_WithValidExpiration()
    {
        var mockContext = CreateMockContext();
        var service = new ChosenListService(mockContext.Object);

        var eventDate = new DateTime(2026, 6, 15, 20, 0, 0);
        var result = await service.CreateAsync(Guid.NewGuid(), eventDate);

        Assert.NotEqual(Guid.Empty, result.ChosenListId);
        Assert.NotEqual(Guid.Empty, result.ListUrl);
        Assert.Equal(eventDate.AddDays(1), result.Expire);
    }

    [Fact]
    public async Task GetByListUrlAsync_ShouldReturnChosenList_WhenFound()
    {
        var listGuid = Guid.NewGuid();
        var data = new List<ChosenList>
        {
            new() { ChosenListId = Guid.NewGuid(), ListUrl = listGuid, Expire = DateTime.UtcNow.AddDays(5) }
        };

        var mockContext = CreateMockContext();
        mockContext.Setup(c => c.ChosenLists).Returns(CreateMockDbSet(data).Object);

        var service = new ChosenListService(mockContext.Object);
        var result = await service.GetByListUrlAsync(listGuid);

        Assert.NotNull(result);
        Assert.Equal(listGuid, result.ListUrl);
    }

    [Fact]
    public async Task GetByListUrlAsync_ShouldReturnNull_WhenNotFound()
    {
        var mockContext = CreateMockContext();
        mockContext.Setup(c => c.ChosenLists).Returns(CreateMockDbSet(new List<ChosenList>()).Object);

        var service = new ChosenListService(mockContext.Object);
        var result = await service.GetByListUrlAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task IsExpiredAsync_ShouldReturnTrue_WhenExpired()
    {
        var pastDate = DateTime.UtcNow.AddDays(-10);
        var list = new ChosenList
        {
            ChosenListId = Guid.NewGuid(),
            ListUrl = Guid.NewGuid(),
            Expire = pastDate.AddDays(1)
        };
        var data = new List<ChosenList> { list };

        var mockContext = CreateMockContext();
        mockContext.Setup(c => c.ChosenLists).Returns(CreateMockDbSet(data).Object);

        var service = new ChosenListService(mockContext.Object);
        var result = await service.IsExpiredAsync(list.ListUrl);

        Assert.True(result);
    }

    [Fact]
    public async Task IsExpiredAsync_ShouldReturnFalse_WhenNotExpired()
    {
        var futureDate = DateTime.UtcNow.AddDays(30);
        var list = new ChosenList
        {
            ChosenListId = Guid.NewGuid(),
            ListUrl = Guid.NewGuid(),
            Expire = futureDate.AddDays(1)
        };
        var data = new List<ChosenList> { list };

        var mockContext = CreateMockContext();
        mockContext.Setup(c => c.ChosenLists).Returns(CreateMockDbSet(data).Object);

        var service = new ChosenListService(mockContext.Object);
        var result = await service.IsExpiredAsync(list.ListUrl);

        Assert.False(result);
    }

    [Fact]
    public async Task GetByAdminIdAsync_ShouldReturnLists_ForGivenAdmin()
    {
        var adminId = "admin-test-123";
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();

        var lists = new List<ChosenList>
        {
            new() { ChosenListId = eventId1, ListUrl = Guid.NewGuid(), Expire = DateTime.UtcNow.AddDays(5) },
            new() { ChosenListId = eventId2, ListUrl = Guid.NewGuid(), Expire = DateTime.UtcNow.AddDays(10) }
        };

        var events = new List<Event>
        {
            new() { EventId = eventId1, AdminId = adminId, Name = "Evento 1", Date = DateTime.UtcNow, ChosenListId = eventId1 },
            new() { EventId = eventId2, AdminId = adminId, Name = "Evento 2", Date = DateTime.UtcNow, ChosenListId = eventId2 }
        };

        var mockContext = CreateMockContext();
        mockContext.Setup(c => c.ChosenLists).Returns(CreateMockDbSet(lists).Object);
        mockContext.Setup(c => c.Events).Returns(CreateMockDbSet(events).Object);

        var service = new ChosenListService(mockContext.Object);
        var result = await service.GetByAdminIdAsync(adminId);

        Assert.Equal(2, result.Count);
    }
}
