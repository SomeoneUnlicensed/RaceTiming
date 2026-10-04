using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceTiming.Api.Data;
using RaceTiming.Api.Dtos;

namespace RaceTiming.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(RaceTimingDbContext db) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> GetEvent(int id, CancellationToken cancellationToken)
    {
        var raceEvent = await db.Events.FindAsync([id], cancellationToken);
        if (raceEvent is null)
        {
            throw new NotFoundException($"Event {id} not found.");
        }

        return Ok(new EventDto(raceEvent.Id, raceEvent.Name, raceEvent.StartTime, raceEvent.ResultsCutoff));
    }

    [HttpGet("{id:int}/participants")]
    public async Task<ActionResult<List<ParticipantDto>>> GetParticipants(int id, CancellationToken cancellationToken)
    {
        if (!await db.Events.AnyAsync(e => e.Id == id, cancellationToken))
        {
            throw new NotFoundException($"Event {id} not found.");
        }

        var participants = await db.Participants
            .Where(p => p.EventId == id)
            .OrderBy(p => p.BibNumber)
            .Select(p => new ParticipantDto(p.Id, p.BibNumber, p.Name, p.EventId))
            .ToListAsync(cancellationToken);

        return Ok(participants);
    }
}
