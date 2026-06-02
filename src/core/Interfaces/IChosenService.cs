using SimplePartyList.Core.Entities;

namespace SimplePartyList.Core.Interfaces;

public interface IChosenService
{
    Task<Chosen> SubmitAsync(Guid chosenListId, string guestName, Guid itemId);
    Task DeleteAsync(Chosen chosenToDelete);
    Task<List<Chosen>> GetByChosenListIdAsync(Guid chosenListId);
}
