using SimplePartyList.Core.Entities;

namespace SimplePartyList.Core.Interfaces;

public interface IChosenService
{
    Task<Chosen> SubmitAsync(Guid chosenListId, string guestName, Guid itemId);
    Task DeleteAsync(Guid chosenId);
    Task<List<Chosen>> GetByChosenListIdAsync(Guid chosenListId);
}
