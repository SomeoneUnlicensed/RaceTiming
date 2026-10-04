namespace RaceTiming.Api.Domain;

public class Participant
{
    public int Id { get; set; }
    public int BibNumber { get; set; }
    public string Name { get; set; } = string.Empty;

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public List<CheckpointPass> Passes { get; set; } = [];
}
