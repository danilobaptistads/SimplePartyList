using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimplePartyList.Core.DTOs;
using SimplePartyList.Core.Interfaces;
using SimplePartyList.Infrastructure.Data;

namespace SimplePartyList.Web.Endpoints;

public static class EventEndpoints
{
    public static void MapEventEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/events")
            .RequireAuthorization();

        group.MapPost("/", async (
            [FromBody] CreateEventDto dto,
            IEventService eventService,
            ClaimsPrincipal user,
            SimplePartyListContext dbContext) =>
        {
            var adminId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var ev = await eventService.CreateAsync(adminId, dto.Name, dto.Date);
            return Results.Created($"/api/events/{ev.EventId}", await MapToDtoAsync(ev, dbContext));
        });

        group.MapGet("/", async (
            IEventService eventService,
            ClaimsPrincipal user,
            SimplePartyListContext dbContext) =>
        {
            var adminId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var events = await eventService.GetByAdminIdAsync(adminId);
            var dtos = new List<AdminEventResponseDto>();
            foreach (var ev in events)
                dtos.Add(await MapToDtoAsync(ev, dbContext));
            return Results.Ok(dtos);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IEventService eventService,
            SimplePartyListContext dbContext) =>
        {
            var ev = await eventService.GetByIdAsync(id);
            return ev is null ? Results.NotFound() : Results.Ok(await MapToDtoAsync(ev, dbContext));
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateEventDto dto,
            IEventService eventService,
            ClaimsPrincipal user,
            SimplePartyListContext dbContext) =>
        {
            var ev = await eventService.GetByIdAsync(id);
            if (ev is null) return Results.NotFound();

            var adminId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (ev.AdminId != adminId) return Results.Forbid();

            ev.Name = dto.Name;
            ev.Date = dto.Date;

            var updated = await eventService.UpdateAsync(ev);
            return Results.Ok(await MapToDtoAsync(updated, dbContext));
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IEventService eventService,
            ClaimsPrincipal user) =>
        {
            var ev = await eventService.GetByIdAsync(id);
            if (ev is null) return Results.NotFound();

            var adminId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (ev.AdminId != adminId) return Results.Forbid();

            await eventService.DeleteAsync(ev);
            return Results.NoContent();
        });
    }

    private static async Task<AdminEventResponseDto> MapToDtoAsync(Core.Entities.Event ev, SimplePartyListContext dbContext)
    {
        var chosenList = await dbContext.ChosenLists.FindAsync(ev.ChosenListId);
        return new AdminEventResponseDto
        {
            EventId = ev.EventId,
            Name = ev.Name,
            Date = ev.Date,
            ChosenListId = ev.ChosenListId,
            ListUrl = chosenList?.ListUrl ?? Guid.Empty,
        };
    }
}
