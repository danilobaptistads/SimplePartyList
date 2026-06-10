using Microsoft.EntityFrameworkCore;
using SimplePartyList.Core.Entities;
using SimplePartyList.Infrastructure.Data;
using SimplePartyList.Infrastructure.Services;

namespace SimplePartyList.Tests.Services;

public class ItemServiceTests
{
    private static SimplePartyListContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SimplePartyListContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new SimplePartyListContext(options);
    }

    [Fact]
    public async Task AddNewAsync_ShouldCreateItem_WhithCota()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        await context.SaveChangesAsync();

        var itemService = new ItemService(context);
        var result = await itemService.AddNewAsync(chosenList.ChosenListId, "Cerveja", 50);

        Assert.NotNull(result);
        Assert.Equal("Cerveja", result.Name);
        Assert.Equal(50, result.MaxQuantity);
        Assert.Equal(chosenList.ChosenListId, result.ChosenListId);
    }

    [Fact]
    public async Task AddNewAsync_ShouldCreateItem_WithoutCota()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        await context.SaveChangesAsync();

        var itemService = new ItemService(context);
        var result = await itemService.AddNewAsync(chosenList.ChosenListId, "Refrigerante");

        Assert.Null(result.MaxQuantity);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnItem()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        var item = new Item { Name = "Cerveja", MaxQuantity = 50, ChosenListId = chosenList.ChosenListId };
        context.Items.Add(item);
        await context.SaveChangesAsync();

        var itemService = new ItemService(context);
        var result = await itemService.GetByIdAsync(item.ItemId);

        Assert.NotNull(result);
        Assert.Equal(item.ItemId, result.ItemId);
        Assert.Equal("Cerveja", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        using var context = CreateContext();
        var itemService = new ItemService(context);
        var result = await itemService.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        var item = new Item { Name = "Cerveja", MaxQuantity = 50, ChosenListId = chosenList.ChosenListId };
        context.Items.Add(item);
        await context.SaveChangesAsync();

        item.Name = "Cerveja Skol";
        item.MaxQuantity = 100;

        var itemService = new ItemService(context);
        var result = await itemService.UpdateAsync(item);

        Assert.NotNull(result);
        Assert.Equal("Cerveja Skol", result.Name);
        Assert.Equal(100, result.MaxQuantity);

        var updated = await context.Items.FindAsync(item.ItemId);
        Assert.Equal("Cerveja Skol", updated!.Name);
        Assert.Equal(100, updated.MaxQuantity);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenNotFound()
    {
        using var context = CreateContext();
        var itemService = new ItemService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => itemService.UpdateAsync(new Item { ItemId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveItem()
    {
        using var context = CreateContext();
        var chosenList = new ChosenList { Expire = DateTime.UtcNow.AddDays(5) };
        context.ChosenLists.Add(chosenList);
        var item = new Item { Name = "Cerveja", MaxQuantity = 50, ChosenListId = chosenList.ChosenListId };
        context.Items.Add(item);
        await context.SaveChangesAsync();

        var itemService = new ItemService(context);
        await itemService.DeleteAsync(item);

        Assert.Null(await context.Items.FindAsync(item.ItemId));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenNotFound()
    {
        using var context = CreateContext();
        var itemService = new ItemService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => itemService.DeleteAsync(new Item { ItemId = Guid.NewGuid() }));
    }

}
