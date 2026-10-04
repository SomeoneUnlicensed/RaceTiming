using RaceTiming.Api.Domain;

namespace RaceTiming.Api.Dtos;

public record EventDto(int Id, string Name, DateTimeOffset StartTime, TimeSpan ResultsCutoff);

public record ParticipantDto(int Id, int BibNumber, string Name, int EventId);

/// <summary>Identifies a checkpoint by name, as an arch reports it — never our internal id.
/// EventId is required since checkpoint names repeat across events (every event has a
/// "finish").</summary>
public record CheckpointPassRequest(int EventId, int ParticipantNumber, string Checkpoint, DateTimeOffset Timestamp);

/// <summary>One reading in a batch; EventId lives on the envelope instead.</summary>
public record CheckpointPassEntry(int ParticipantNumber, string Checkpoint, DateTimeOffset Timestamp);

public record BatchCheckpointPassRequest(int EventId, List<CheckpointPassEntry> Passes);

public record CheckpointPassResult(int ParticipantNumber, string Checkpoint, bool Recorded, string? Reason);

public record BatchCheckpointPassResponse(int Accepted, int Duplicates, int Rejected, List<CheckpointPassResult> Results);

public record SplitDto(string Checkpoint, DateTimeOffset Timestamp);

public record ParticipantResultDto(
    int BibNumber,
    string Name,
    ParticipantStatus Status,
    DateTimeOffset? FinishTime,
    List<SplitDto> Splits);
