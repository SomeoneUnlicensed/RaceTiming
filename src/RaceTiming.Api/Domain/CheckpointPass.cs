namespace RaceTiming.Api.Domain;

/// <summary>One timing read from a checkpoint arch. (ParticipantId, CheckpointId, Timestamp)
/// has a unique index in <see cref="Data.RaceTimingDbContext"/>.</summary>
public class CheckpointPass
{
    public long Id { get; set; }

    public int ParticipantId { get; set; }
    public Participant? Participant { get; set; }

    public int CheckpointId { get; set; }
    public Checkpoint? Checkpoint { get; set; }

    /// <summary>When the participant passed, as reported by the arch.</summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>When the system stored this read; can lag <see cref="Timestamp"/>.</summary>
    public DateTimeOffset ReceivedAt { get; set; }
}
