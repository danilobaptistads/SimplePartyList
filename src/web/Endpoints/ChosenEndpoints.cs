using System.Security.Claims;
using SimplePartyList.Core.DTOs;
using SimplePartyList.Core.Interfaces;

namespace SimplePartyList.Web.Endpoints;

public static class ChosenEndpoints
{
    public static void MapChosenEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").RequireAuthorization();

        group.MapGet("/events/{eventId:guid}/chosens", async (
            Guid eventId,
            IEventService eventService,
            IChosenService chosenService,
            ClaimsPrincipal user) =>
        {
            var ev = await eventService.GetByIdAsync(eventId);
            if (ev is null) return Results.NotFound();

            var adminId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (ev.AdminId != adminId) return Results.Forbid();

            var chosens = await chosenService.GetByChosenListIdAsync(ev.ChosenListId);
            var dtos = chosens.Select(c => new ChosenResponseDto
            {
                ChosenId = c.ChosenId,
                GuestName = c.GuestName,
                ItemName = c.ItemName
            }).ToList();

            return Results.Ok(dtos);
        });

        group.MapDelete("/chosens/{id:guid}", async (
            Guid id,
            IChosenService chosenService,
            IEventService eventService,
            ClaimsPrincipal user) =>
        {
            var chosen = await chosenService.GetByIdAsync(id);
            if (chosen is null) return Results.NotFound();

            var ev = await eventService.GetByChosenListIdAsync(chosen.ChosenListId);
            if (ev is null) return Results.NotFound();

            var adminId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (ev.AdminId != adminId) return Results.Forbid();

            await chosenService.DeleteAsync(chosen);
            return Results.NoContent();
        });

        var publicGroup = app.MapGroup("/api/lists");

        publicGroup.MapPost("/{listUrl}/chosens", async (
            Guid listUrl,
            [Microsoft.AspNetCore.Mvc.FromBody] SubmitChosenDto dto,
            IChosenListService chosenListService,
            IChosenService chosenService) =>
        {
            var chosenList = await chosenListService.GetByListUrlAsync(listUrl);
            if (chosenList is null) return Results.NotFound();

            try
            {
                var chosen = await chosenService.SubmitAsync(chosenList.ChosenListId, dto.GuestName, dto.ItemId);
                return Results.Created($"/api/lists/{listUrl}/chosens/{chosen.ChosenId}", new ChosenResponseDto
                {
                    ChosenId = chosen.ChosenId,
                    GuestName = chosen.GuestName,
                    ItemName = chosen.ItemName
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(ex.Message);
            }
        });
    }
}
