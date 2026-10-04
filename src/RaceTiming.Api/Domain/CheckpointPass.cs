namespace RaceTiming.Api.Domain;

/// <summary>
/// One timing read from an arch/checkpoint. The natural key (ParticipantId, CheckpointId,
/// Timestamp) has a unique index — see <see cref="Data.RaceTimingDbContext"/> — so the same
/// arch event replayed twice (lost connection, retried batch) never double-records.
/// </summary>
public class CheckpointPass
{
    public long Id { get; set; }

    public int ParticipantId { get; set; }
    public Participant? Participant { get; set; }

    public int CheckpointId { get; set; }
    public Checkpoint? Checkpoint { get; set; }

    /// <summary>When the participant actually passed the checkpoint, reported by the arch.</summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>When our system actually received and stored this read — can lag Timestamp.</summary>
    public DateTimeOffset ReceivedAt { get; set; }
}
