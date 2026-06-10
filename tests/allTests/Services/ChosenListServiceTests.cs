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

}
