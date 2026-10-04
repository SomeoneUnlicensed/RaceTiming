using RaceTiming.Api.Domain;
using RaceTiming.Api.Services;
using Xunit;

namespace RaceTiming.Tests;

public class ResultStatusCalculatorTests
{
    private static readonly DateTimeOffset EventStart = new(2026, 10, 4, 10, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Cutoff = TimeSpan.FromHours(6);
    private static readonly Checkpoint FinishCheckpoint = new() { Id = 1, Name = "finish", Type = CheckpointType.Finish };
    private static readonly Checkpoint SplitCheckpoint = new() { Id = 2, Name = "5km", Type = CheckpointType.Intermediate };

    [Fact]
    public void NoPasses_WithinResultsWindow_IsUnknown_NotDnf()
    {
        var now = EventStart.AddHours(1);

        var status = ResultStatusCalculator.Calculate([], EventStart, Cutoff, now);

        Assert.Equal(ParticipantStatus.Unknown, status);
    }

    [Fact]
    public void OnlyIntermediateSplit_NoFinishYet_WithinWindow_IsUnknown()
    {
        var now = EventStart.AddHours(2);
        var passes = new[] { new CheckpointPass { Checkpoint = SplitCheckpoint, Timestamp = now } };

        var status = ResultStatusCalculator.Calculate(passes, EventStart, Cutoff, now);

        Assert.Equal(ParticipantStatus.Unknown, status);
    }

    [Fact]
    public void FinishPassPresent_IsFinished_RegardlessOfTime()
    {
        var finishTime = EventStart.AddMinutes(90);
        var passes = new[] { new CheckpointPass { Checkpoint = FinishCheckpoint, Timestamp = finishTime } };

        var status = ResultStatusCalculator.Calculate(passes, EventStart, Cutoff, now: finishTime);

        Assert.Equal(ParticipantStatus.Finished, status);
    }

    [Fact]
    public void NoFinish_AfterCutoffElapses_BecomesDnf()
    {
        var now = EventStart + Cutoff + TimeSpan.FromMinutes(1);

        var status = ResultStatusCalculator.Calculate([], EventStart, Cutoff, now);

        Assert.Equal(ParticipantStatus.Dnf, status);
    }

    [Fact]
    public void NoFinish_ExactlyAtCutoff_IsAlreadyDnf()
    {
        var now = EventStart + Cutoff;

        var status = ResultStatusCalculator.Calculate([], EventStart, Cutoff, now);

        Assert.Equal(ParticipantStatus.Dnf, status);
    }

    [Fact]
    public void LateFinishArrivingAfterCutoff_StillOverridesDnf()
    {
        // A delayed FINISH read arriving after cutoff must still resolve to Finished.
        var now = EventStart + Cutoff + TimeSpan.FromHours(1);
        var passes = new[] { new CheckpointPass { Checkpoint = FinishCheckpoint, Timestamp = EventStart.AddHours(1) } };

        var status = ResultStatusCalculator.Calculate(passes, EventStart, Cutoff, now);

        Assert.Equal(ParticipantStatus.Finished, status);
    }
}
