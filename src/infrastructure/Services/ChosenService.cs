using Microsoft.EntityFrameworkCore;
using SimplePartyList.Core.Entities;
using SimplePartyList.Core.Interfaces;
using SimplePartyList.Infrastructure.Data;

namespace SimplePartyList.Infrastructure.Services;

public class ChosenService : IChosenService
{
    private readonly SimplePartyListContext _context;

    public ChosenService(SimplePartyListContext context)
    {
        _context = context;
    }

    public async Task<Chosen> SubmitAsync(Guid chosenListId, string guestName, Guid itemId)
    {
        var chosenList = await _context.ChosenLists
            .Include(cl => cl.Chosens)
            .FirstOrDefaultAsync(cl => cl.ChosenListId == chosenListId)
            ?? throw new KeyNotFoundException($"ChosenList {chosenListId} not found.");

        if (DateTime.UtcNow > chosenList.Expire)
            throw new InvalidOperationException("Lista expirada.");

        var item = await _context.Items.FindAsync(itemId)
            ?? throw new KeyNotFoundException($"Item {itemId} not found.");

        if (item.MaxQuantity.HasValue)
        {
            var currentCount = chosenList.Chosens.Count(c => c.ItemName == item.Name);
            if (currentCount >= item.MaxQuantity.Value)
                throw new InvalidOperationException("Cota esgotada para este item.");
        }

        var chosen = new Chosen
        {
            GuestName = guestName,
            ItemName = item.Name,
            ChosenListId = chosenListId
        };

        _context.Chosens.Add(chosen);
        await _context.SaveChangesAsync();

        return chosen;
    }

    public async Task DeleteAsync(Guid chosenId)
    {
        var chosen = await _context.Chosens.FindAsync(chosenId)
            ?? throw new KeyNotFoundException($"Chosen {chosenId} not found.");

        _context.Chosens.Remove(chosen);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Chosen>> GetByChosenListIdAsync(Guid chosenListId)
    {
        return await _context.Chosens
            .Where(c => c.ChosenListId == chosenListId)
            .ToListAsync();
    }
}
