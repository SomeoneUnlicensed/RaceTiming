using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RaceTiming.Api.Data;
using RaceTiming.Api.Domain;
using RaceTiming.Api.Dtos;

namespace RaceTiming.Api.Services;

public class ResultsService(RaceTimingDbContext db, TimeProvider clock, IMemoryCache cache)
{
    public async Task<List<ParticipantResultDto>> GetResultsAsync(int eventId, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(ResultsCacheKeys.ForEvent(eventId), out List<ParticipantResultDto>? cached) && cached is not null)
        {
            return cached;
        }

        var raceEvent = await db.Events.FindAsync([eventId], cancellationToken)
            ?? throw new NotFoundException($"Event {eventId} not found.");

        var participants = await db.Participants
            .Where(p => p.EventId == eventId)
            .Include(p => p.Passes).ThenInclude(pass => pass.Checkpoint)
            .ToListAsync(cancellationToken);

        var now = clock.GetUtcNow();
        var results = participants
            .Select(p => BuildResult(p, raceEvent, now))
            .OrderBy(r => r.Status == ParticipantStatus.Finished ? 0 : 1)
            .ThenBy(r => r.FinishTime)
            .ThenBy(r => r.BibNumber)
            .ToList();

        cache.Set(ResultsCacheKeys.ForEvent(eventId), results, TimeSpan.FromSeconds(30));
        return results;
    }

    private static ParticipantResultDto BuildResult(Participant participant, Event raceEvent, DateTimeOffset now)
    {
        var status = ResultStatusCalculator.Calculate(participant.Passes, raceEvent.StartTime, raceEvent.ResultsCutoff, now);

        var splits = participant.Passes
            .OrderBy(p => p.Checkpoint!.SequenceOrder)
            .Select(p => new SplitDto(p.Checkpoint!.Name, p.Timestamp))
            .ToList();

        var finishTime = status == ParticipantStatus.Finished
            ? participant.Passes.First(p => p.Checkpoint!.Type == CheckpointType.Finish).Timestamp
            : (DateTimeOffset?)null;

        return new ParticipantResultDto(participant.BibNumber, participant.Name, status, finishTime, splits);
    }
}
