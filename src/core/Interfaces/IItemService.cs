using SimplePartyList.Core.Entities;

namespace SimplePartyList.Core.Interfaces;

public interface IItemService
{
    Task<Item> AddNewAsync(Guid chosenListId, string name, int? maxQuantity = null);
    Task<Item?> GetByIdAsync(Guid itemId);
    Task<Item> UpdateAsync(Item itemToUpdate);
    Task DeleteAsync(Item itemToDelete);
    Task<List<Item>> GetByChosenListIdAsync(Guid chosenListId);
}
