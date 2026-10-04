namespace RaceTiming.Api.Domain;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }

    /// <summary>How long after StartTime a participant without FINISH becomes Dnf.</summary>
    public TimeSpan ResultsCutoff { get; set; } = TimeSpan.FromHours(6);

    public List<Participant> Participants { get; set; } = [];
    public List<Checkpoint> Checkpoints { get; set; } = [];
}
