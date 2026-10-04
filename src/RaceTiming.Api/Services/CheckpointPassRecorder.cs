using Microsoft.EntityFrameworkCore;
using RaceTiming.Api.Data;
using RaceTiming.Api.Domain;
using RaceTiming.Api.Dtos;

namespace RaceTiming.Api.Services;

public class CheckpointPassRecorder(
    RaceTimingDbContext db,
    TimeProvider clock,
    IEnumerable<INotificationChannel> notificationChannels)
{
    public async Task<CheckpointPassResult> RecordAsync(CheckpointPassRequest request, CancellationToken cancellationToken)
    {
        var result = await RecordManyAsync(
            request.EventId,
            [new CheckpointPassEntry(request.ParticipantNumber, request.Checkpoint, request.Timestamp)],
            cancellationToken);

        return result.Results[0];
    }

    public async Task<BatchCheckpointPassResponse> RecordManyAsync(
        int eventId, List<CheckpointPassEntry> entries, CancellationToken cancellationToken)
    {
        var participantsByBib = await db.Participants
            .Where(p => p.EventId == eventId)
            .ToDictionaryAsync(p => p.BibNumber, cancellationToken);

        var checkpointsByName = await db.Checkpoints
            .Where(c => c.EventId == eventId)
            .ToDictionaryAsync(c => c.Name, StringComparer.OrdinalIgnoreCase, cancellationToken);

        // One query for every (participant, checkpoint) pair touched by this batch, instead
        // of one existence check per entry — this is what makes a 1000-row burst upload
        // cheap, and it's what idempotency actually hinges on: skip anything already stored.
        var touchedParticipantIds = entries
            .Select(e => participantsByBib.GetValueOrDefault(e.ParticipantNumber)?.Id)
            .Where(id => id is not null)
            .Select(id => id!.Value)
            .ToHashSet();

        var existingKeys = await db.CheckpointPasses
            .Where(p => touchedParticipantIds.Contains(p.ParticipantId))
            .Select(p => new { p.ParticipantId, p.CheckpointId, p.Timestamp })
            .ToListAsync(cancellationToken);
        var existingKeySet = existingKeys
            .Select(k => (k.ParticipantId, k.CheckpointId, k.Timestamp))
            .ToHashSet();

        var toInsert = new List<CheckpointPass>();
        var results = new List<CheckpointPassResult>(entries.Count);
        var seenInThisBatch = new HashSet<(int ParticipantId, int CheckpointId, DateTimeOffset Timestamp)>();

        foreach (var entry in entries)
        {
            if (!participantsByBib.TryGetValue(entry.ParticipantNumber, out var participant))
            {
                results.Add(new CheckpointPassResult(entry.ParticipantNumber, entry.Checkpoint, false,
                    "Unknown participant number for this event."));
                continue;
            }

            if (!checkpointsByName.TryGetValue(entry.Checkpoint, out var checkpoint))
            {
                results.Add(new CheckpointPassResult(entry.ParticipantNumber, entry.Checkpoint, false,
                    "Unknown checkpoint for this event."));
                continue;
            }

            var key = (participant.Id, checkpoint.Id, entry.Timestamp);
            if (existingKeySet.Contains(key) || !seenInThisBatch.Add(key))
            {
                results.Add(new CheckpointPassResult(entry.ParticipantNumber, entry.Checkpoint, false,
                    "Already recorded — duplicate arch read, ignored."));
                continue;
            }

            toInsert.Add(new CheckpointPass
            {
                ParticipantId = participant.Id,
                CheckpointId = checkpoint.Id,
                Timestamp = entry.Timestamp,
                ReceivedAt = clock.GetUtcNow()
            });
            results.Add(new CheckpointPassResult(entry.ParticipantNumber, entry.Checkpoint, true, null));
        }

        if (toInsert.Count > 0)
        {
            db.CheckpointPasses.AddRange(toInsert);
            await db.SaveChangesAsync(cancellationToken);

            var checkpointNameById = checkpointsByName.Values.ToDictionary(c => c.Id, c => c.Name);

            // Each channel is independent and owns no shared, non-thread-safe state (unlike
            // the DbContext above), so firing them concurrently is actually safe here.
            await Task.WhenAll(toInsert.Select(pass => Task.WhenAll(notificationChannels.Select(channel =>
                channel.NotifyAsync(eventId, pass.ParticipantId, checkpointNameById[pass.CheckpointId], cancellationToken)))));
        }

        return new BatchCheckpointPassResponse(
            Accepted: results.Count(r => r.Recorded),
            Duplicates: results.Count(r => !r.Recorded && r.Reason!.StartsWith("Already")),
            Rejected: results.Count(r => !r.Recorded && !r.Reason!.StartsWith("Already")),
            Results: results);
    }
}
