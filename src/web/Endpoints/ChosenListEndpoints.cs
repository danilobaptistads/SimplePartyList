using Microsoft.AspNetCore.Mvc;
using SimplePartyList.Core.DTOs;
using SimplePartyList.Core.Interfaces;

namespace SimplePartyList.Web.Endpoints;

public static class ChosenListEndpoints
{
    public static void MapChosenListEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/lists");

        group.MapGet("/{listUrl}", async (
            Guid listUrl,
            IChosenListService chosenListService,
            IEventService eventService) =>
        {
            var chosenList = await chosenListService.GetByListUrlWithItemsAsync(listUrl);
            if (chosenList is null) return Results.NotFound();

            var anEvent = await eventService.GetByChosenListIdAsync(chosenList.ChosenListId);
            if (anEvent is null) return Results.NotFound();

            var chosenCounts = chosenList.Chosens
                .GroupBy(c => c.ItemName)
                .ToDictionary(g => g.Key, g => g.Count());

            var items = chosenList.Items.Select(item => new ItemDto
            {
                ItemId = item.ItemId,
                Name = item.Name,
                MaxQuantity = item.MaxQuantity,
                ChosenCount = chosenCounts.TryGetValue(item.Name, out var count) ? count : 0
            }).ToList();

            return Results.Ok(new PublicListResponseDto
            {
                EventName = anEvent.Name,
                EventDate = anEvent.Date,
                IsExpired = DateTime.UtcNow > chosenList.Expire,
                Items = items
            });
        });

        group.MapGet("/{listUrl}/expired", async (
            Guid listUrl,
            IChosenListService chosenListService) =>
        {
            var chosenList = await chosenListService.GetByListUrlAsync(listUrl);
            if (chosenList is null) return Results.NotFound();

            return Results.Ok(DateTime.UtcNow > chosenList.Expire);
        });
    }
}
