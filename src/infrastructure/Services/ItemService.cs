using Microsoft.EntityFrameworkCore;
using SimplePartyList.Core.Entities;
using SimplePartyList.Core.Interfaces;
using SimplePartyList.Infrastructure.Data;

namespace SimplePartyList.Infrastructure.Services;

public class ItemService : IItemService
{
    private readonly SimplePartyListContext _context;

    public ItemService(SimplePartyListContext context)
    {
        _context = context;
    }

    public async Task<Item> AddNewAsync(Guid chosenListId, string name, int? maxQuantity = null)
    {
        var item = new Item
        {
            ChosenListId = chosenListId,
            Name = name,
            MaxQuantity = maxQuantity
        };

        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<Item?> GetByIdAsync(Guid itemId)
    {
        return await _context.Items.FindAsync(itemId);
    }

    public async Task<Item> UpdateAsync(Item itemToUpdate)
    {
        var item = await _context.Items.FindAsync(itemToUpdate.ItemId)
            ?? throw new KeyNotFoundException($"Item with Id {itemToUpdate.ItemId} not found.");

        item.Name = itemToUpdate.Name;
        item.MaxQuantity = itemToUpdate.MaxQuantity;

        await _context.SaveChangesAsync();

        return item;
    }

    public async Task DeleteAsync(Item itemToDelete)
    {
        var item = await _context.Items.FindAsync(itemToDelete.ItemId)
            ?? throw new KeyNotFoundException($"Item with Id {itemToDelete.ItemId} not found.");

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Item>> GetByChosenListIdAsync(Guid chosenListId)
    {
        return await _context.Items
            .Where(i => i.ChosenListId == chosenListId)
            .ToListAsync();
    }
}
