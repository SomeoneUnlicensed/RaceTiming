namespace RaceTiming.Api.Domain;

public class Checkpoint
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CheckpointType Type { get; set; }

    /// <summary>Position along the course, used to order splits in the results view.</summary>
    public int SequenceOrder { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public List<CheckpointPass> Passes { get; set; } = [];
}
