namespace RaceTiming.Api.Domain;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// How long after StartTime a participant without a FINISH pass is considered DNF.
    /// Before this elapses, their status is reported as Unknown, not DNF.
    /// </summary>
    public TimeSpan ResultsCutoff { get; set; } = TimeSpan.FromHours(6);

    public List<Participant> Participants { get; set; } = [];
    public List<Checkpoint> Checkpoints { get; set; } = [];
}
