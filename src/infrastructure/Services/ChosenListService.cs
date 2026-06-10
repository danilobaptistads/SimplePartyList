using Microsoft.EntityFrameworkCore;
using SimplePartyList.Core.Entities;
using SimplePartyList.Core.Interfaces;
using SimplePartyList.Infrastructure.Data;

namespace SimplePartyList.Infrastructure.Services;

public class ChosenListService : IChosenListService
{
    private readonly SimplePartyListContext _context;

    public ChosenListService(SimplePartyListContext context)
    {
        _context = context;
    }

    public async Task<ChosenList?> GetByListUrlAsync(Guid listUrl)
    {
        return await _context.ChosenLists.FirstOrDefaultAsync(cl => cl.ListUrl == listUrl);
    }

    public async Task<ChosenList?> GetByListUrlWithItemsAsync(Guid listUrl)
    {
        return await _context.ChosenLists
            .Include(cl => cl.Items)
            .Include(cl => cl.Chosens)
            .FirstOrDefaultAsync(cl => cl.ListUrl == listUrl);
    }
}
