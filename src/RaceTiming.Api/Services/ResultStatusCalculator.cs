using RaceTiming.Api.Domain;

namespace RaceTiming.Api.Services;

/// <summary>
/// Decides a participant's race status from the splits recorded so far. Pure and
/// deterministic on purpose — no DB, no clock side effects — so it is cheap to unit test
/// the one rule that actually matters here: a missing FINISH is not instant DNF.
/// </summary>
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
