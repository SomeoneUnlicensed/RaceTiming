using RaceTiming.Api.Domain;

namespace RaceTiming.Api.Services;

/// <summary>Decides a participant's status from splits recorded so far. Pure function,
/// no DB access, for easy unit testing.</summary>
public static class ResultStatusCalculator
{
    public static ParticipantStatus Calculate(
        IReadOnlyCollection<CheckpointPass> passes,
        DateTimeOffset eventStartTime,
        TimeSpan resultsCutoff,
        DateTimeOffset now)
    {
        var finishPass = passes.FirstOrDefault(p => p.Checkpoint?.Type == CheckpointType.Finish);
        if (finishPass is not null)
        {
            return ParticipantStatus.Finished;
        }

        var cutoffAt = eventStartTime + resultsCutoff;
        return now >= cutoffAt ? ParticipantStatus.Dnf : ParticipantStatus.Unknown;
    }
}
