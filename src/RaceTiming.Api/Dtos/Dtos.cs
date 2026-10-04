using RaceTiming.Api.Domain;

namespace RaceTiming.Api.Dtos;

public record EventDto(int Id, string Name, DateTimeOffset StartTime, TimeSpan ResultsCutoff);

public record ParticipantDto(int Id, int BibNumber, string Name, int EventId);

/// <summary>Payload shape straight from an arch: it knows a participant's bib and its own
/// checkpoint name, never our internal database ids. EventId disambiguates — every event
/// has its own "finish" checkpoint, so the name alone isn't globally unique.</summary>
public record CheckpointPassRequest(int EventId, int ParticipantNumber, string Checkpoint, DateTimeOffset Timestamp);

/// <summary>One reading within a batch upload. EventId lives on the envelope, not here,
/// since a single arch burst-uploading 1000 reads belongs to one event.</summary>
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
