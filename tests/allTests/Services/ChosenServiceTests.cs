using Microsoft.EntityFrameworkCore;
using SimplePartyList.Core.Entities;
using SimplePartyList.Infrastructure.Data;
using SimplePartyList.Infrastructure.Services;

namespace SimplePartyList.Tests.Services;

public class ChosenServiceTests
{
    private static SimplePartyListContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SimplePartyListContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new SimplePartyListContext(options);
    }

    [Fact]
    public async Task SubmitAsync_ShouldCreateChosen()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        var chosenItem = new Item { Name = "Cerveja", MaxQuantity = 50, ChosenListId = chosenList.ChosenListId };
        context.ChosenLists.Add(chosenList);
        context.Items.Add(chosenItem);
        await context.SaveChangesAsync();
        var guestName = "João";

        var chosenService = new ChosenService(context);
        var result = await chosenService.SubmitAsync(chosenList.ChosenListId, guestName, chosenItem.ItemId);

        Assert.NotNull(result);
        Assert.Equal("João", result.GuestName);
        Assert.Equal("Cerveja", result.ItemName);
        Assert.Equal(chosenList.ChosenListId, result.ChosenListId);
    }

    [Fact]
    public async Task SubmitAsync_ShouldThrow_WhenListExpired()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(-1) };
        var chosenItem = new Item { Name = "Cerveja", MaxQuantity = 50, ChosenListId = chosenList.ChosenListId };
        context.ChosenLists.Add(chosenList);
        context.Items.Add(chosenItem);
        await context.SaveChangesAsync();
        var guestName = "João";

        var chosenService = new ChosenService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => chosenService.SubmitAsync(chosenList.ChosenListId, guestName, chosenItem.ItemId));
    }

    [Fact]
    public async Task SubmitAsync_ShouldThrow_WhenQuotaExceeded()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        var chosenItem = new Item { Name = "Cerveja", MaxQuantity = 1, ChosenListId = chosenList.ChosenListId };
        context.ChosenLists.Add(chosenList);
        context.Items.Add(chosenItem);
        var guestName = "Maria";
        context.Chosens.Add(new Chosen { GuestName = guestName, ItemName = chosenItem.Name, ChosenListId = chosenList.ChosenListId });
        await context.SaveChangesAsync();

        var chosenService = new ChosenService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => chosenService.SubmitAsync(chosenList.ChosenListId, "João", chosenItem.ItemId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveChosen()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        var chosen = new Chosen { GuestName = "João", ItemName = "Cerveja", ChosenListId = chosenList.ChosenListId };
        context.ChosenLists.Add(chosenList);
        context.Chosens.Add(chosen);
        await context.SaveChangesAsync();

        var chosenService = new ChosenService(context);
        await chosenService.DeleteAsync(chosen.ChosenId);

        Assert.Null(await context.Chosens.FindAsync(chosen.ChosenId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        using var context = CreateContext();
        var chosenService = new ChosenService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => chosenService.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetByChosenListIdAsync_ShouldReturnChosens()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        context.Chosens.AddRange(
            new Chosen { GuestName = "João", ItemName = "Cerveja", ChosenListId = chosenList.ChosenListId },
            new Chosen { GuestName = "Maria", ItemName = "Refrigerante", ChosenListId = chosenList.ChosenListId }
        );
        await context.SaveChangesAsync();

        var chosenService = new ChosenService(context);
        var result = await chosenService.GetByChosenListIdAsync(chosenList.ChosenListId);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByChosenListIdAsync_ShouldReturnEmpty_WhenNoChosens()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        await context.SaveChangesAsync();

        var chosenService = new ChosenService(context);
        var result = await chosenService.GetByChosenListIdAsync(chosenList.ChosenListId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SubmitAsync_ShouldThrow_WhenItemNotFound()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        await context.SaveChangesAsync();
        var guestName = "João";

        var chosenService = new ChosenService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => chosenService.SubmitAsync(chosenList.ChosenListId, guestName, Guid.NewGuid()));
    }
}
