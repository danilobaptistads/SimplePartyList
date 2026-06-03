using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SimplePartyList.Core.DTOs;
using SimplePartyList.Core.Interfaces;

namespace SimplePartyList.API.Endpoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").RequireAuthorization();

        group.MapPost("/events/{eventId:guid}/items", async (
            Guid eventId,
            [FromBody] CreateItemDto dto,
            IEventService eventService,
            IItemService itemService,
            ClaimsPrincipal user) =>
        {
            var ev = await eventService.GetByIdAsync(eventId);
            if (ev is null) return Results.NotFound();

            var adminId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (ev.AdminId != adminId) return Results.Forbid();

            var item = await itemService.AddNewAsync(ev.ChosenListId, dto.Name, dto.MaxQuantity);
            return Results.Created($"/api/items/{item.ItemId}", new ItemDto
            {
                ItemId = item.ItemId,
                Name = item.Name,
                MaxQuantity = item.MaxQuantity,
                ChosenCount = 0
            });
        });

        group.MapGet("/events/{eventId:guid}/items", async (
            Guid eventId,
            IEventService eventService,
            IItemService itemService,
            IChosenService chosenService) =>
        {
            var ev = await eventService.GetByIdAsync(eventId);
            if (ev is null) return Results.NotFound();

            var items = await itemService.GetByChosenListIdAsync(ev.ChosenListId);
            var chosens = await chosenService.GetByChosenListIdAsync(ev.ChosenListId);
            var chosenCounts = chosens
                .GroupBy(c => c.ItemName)
                .ToDictionary(g => g.Key, g => g.Count());

            var dtos = items.Select(item => new ItemDto
            {
                ItemId = item.ItemId,
                Name = item.Name,
                MaxQuantity = item.MaxQuantity,
                ChosenCount = chosenCounts.TryGetValue(item.Name, out var count) ? count : 0
            }).ToList();

            return Results.Ok(dtos);
        });

        group.MapGet("/items/{id:guid}", async (
            Guid id,
            IItemService itemService) =>
        {
            var item = await itemService.GetByIdAsync(id);
            if (item is null) return Results.NotFound();

            return Results.Ok(new ItemDto
            {
                ItemId = item.ItemId,
                Name = item.Name,
                MaxQuantity = item.MaxQuantity,
                ChosenCount = 0
            });
        });

        group.MapPut("/items/{id:guid}", async (
            Guid id,
            [FromBody] UpdateItemDto dto,
            IItemService itemService,
            IEventService eventService,
            ClaimsPrincipal user) =>
        {
            var item = await itemService.GetByIdAsync(id);
            if (item is null) return Results.NotFound();

            var ev = await eventService.GetByChosenListIdAsync(item.ChosenListId);
            if (ev is null) return Results.NotFound();

            var adminId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (ev.AdminId != adminId) return Results.Forbid();

            item.Name = dto.Name;
            item.MaxQuantity = dto.MaxQuantity;

            var updated = await itemService.UpdateAsync(item);
            return Results.Ok(new ItemDto
            {
                ItemId = updated.ItemId,
                Name = updated.Name,
                MaxQuantity = updated.MaxQuantity,
                ChosenCount = 0
            });
        });

        group.MapDelete("/items/{id:guid}", async (
            Guid id,
            IItemService itemService,
            IEventService eventService,
            IChosenService chosenService,
            ClaimsPrincipal user) =>
        {
            var item = await itemService.GetByIdAsync(id);
            if (item is null) return Results.NotFound();

            var ev = await eventService.GetByChosenListIdAsync(item.ChosenListId);
            if (ev is null) return Results.NotFound();

            var adminId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (ev.AdminId != adminId) return Results.Forbid();

            var chosens = await chosenService.GetByChosenListIdAsync(item.ChosenListId);
            if (chosens.Any(c => c.ItemName == item.Name))
                return Results.Conflict(new { error = "Item possui escolhas vinculadas." });

            await itemService.DeleteAsync(item);
            return Results.NoContent();
        });
    }
}
