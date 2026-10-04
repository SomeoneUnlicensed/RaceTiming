namespace RaceTiming.Api.Domain;

public enum CheckpointType
{
    Start,
    Intermediate,
    Finish
}

/// <summary>
/// Unknown is the default for anyone without a FINISH pass while the event's results
/// window is still open — DNF is only assigned once <see cref="Event.ResultsCutoff"/>
/// has elapsed since <see cref="Event.StartTime"/>. This is the core business rule:
/// a single missed/delayed checkpoint read must never read as DNF.
/// </summary>
public enum ParticipantStatus
{
    Unknown,
    Finished,
    Dnf
}
