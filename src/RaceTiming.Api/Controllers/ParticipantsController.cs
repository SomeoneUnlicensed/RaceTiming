using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceTiming.Api.Data;
using RaceTiming.Api.Dtos;

namespace RaceTiming.Api.Controllers;

[ApiController]
[Route("api/participants")]
public class ParticipantsController(RaceTimingDbContext db) : ControllerBase
{
    /// <summary>
    /// Bib numbers are only unique within one event, so when more than one event has the
    /// same bib in play, pass ?eventId= to disambiguate; otherwise the first match wins.
    /// </summary>
    [HttpGet("{bibNumber:int}")]
    public async Task<ActionResult<ParticipantDto>> GetByBibNumber(
        int bibNumber, [FromQuery] int? eventId, CancellationToken cancellationToken)
    {
        var query = db.Participants.Where(p => p.BibNumber == bibNumber);
        if (eventId is not null)
        {
            query = query.Where(p => p.EventId == eventId);
        }

        var participant = await query
            .Select(p => new ParticipantDto(p.Id, p.BibNumber, p.Name, p.EventId))
            .FirstOrDefaultAsync(cancellationToken);

        if (participant is null)
        {
            throw new NotFoundException($"Participant with bib number {bibNumber} not found.");
        }

        return Ok(participant);
    }
}
