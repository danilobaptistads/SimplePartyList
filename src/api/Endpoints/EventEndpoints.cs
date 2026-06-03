using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SimplePartyList.Core.DTOs;
using SimplePartyList.Core.Interfaces;

namespace SimplePartyList.API.Endpoints;

public static class EventEndpoints
{
    public static void MapEventEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/events")
            .RequireAuthorization();

        group.MapPost("/", async (
            [FromBody] CreateEventDto dto,
            IEventService eventService,
            ClaimsPrincipal user) =>
        {
            var adminId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var ev = await eventService.CreateAsync(adminId, dto.Name, dto.Date);
            return Results.Created($"/api/events/{ev.EventId}", MapToDto(ev));
        });

        group.MapGet("/", async (
            IEventService eventService,
            ClaimsPrincipal user) =>
        {
            var adminId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var events = await eventService.GetByAdminIdAsync(adminId);
            return Results.Ok(events.Select(MapToDto));
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IEventService eventService) =>
        {
            var ev = await eventService.GetByIdAsync(id);
            return ev is null ? Results.NotFound() : Results.Ok(MapToDto(ev));
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateEventDto dto,
            IEventService eventService) =>
        {
            var ev = await eventService.GetByIdAsync(id);
            if (ev is null) return Results.NotFound();

            ev.Name = dto.Name;
            ev.Date = dto.Date;

            var updated = await eventService.UpdateAsync(ev);
            return Results.Ok(MapToDto(updated));
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IEventService eventService) =>
        {
            var ev = await eventService.GetByIdAsync(id);
            if (ev is null) return Results.NotFound();

            await eventService.DeleteAsync(ev);
            return Results.NoContent();
        });
    }

    private static AdminEventResponseDto MapToDto(Core.Entities.Event ev)
    {
        return new AdminEventResponseDto
        {
            EventId = ev.EventId,
            Name = ev.Name,
            Date = ev.Date,
            ChosenListId = ev.ChosenListId,
        };
    }
}
