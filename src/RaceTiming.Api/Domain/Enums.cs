namespace RaceTiming.Api.Domain;

public enum CheckpointType
{
    Start,
    Intermediate,
    Finish
}

/// <summary>Unknown applies until <see cref="Event.ResultsCutoff"/> elapses after
/// <see cref="Event.StartTime"/> with no FINISH recorded; only then does it become Dnf.</summary>
public enum ParticipantStatus
{
    Unknown,
    Finished,
    Dnf
}
