using System.Net.Http.Json;
using RaceTiming.Api.Dtos;
using Xunit;

namespace RaceTiming.Tests;

public class CheckpointIdempotencyTests : IClassFixture<RaceTimingApiFactory>
{
    private readonly HttpClient _client;

    public CheckpointIdempotencyTests(RaceTimingApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SamePassSubmittedTwice_IsRecordedOnce()
    {
        // Bib 1103 (Alex) has no seeded passes, and this timestamp is unique to this test —
        // keeps this test's assertions independent of the demo data seeded at startup.
        var request = new CheckpointPassRequest(EventId: 1, ParticipantNumber: 1103, Checkpoint: "start",
            Timestamp: DateTimeOffset.Parse("2026-10-04T10:00:03Z"));

        var first = await _client.PostAsJsonAsync("/api/checkpoints/pass", request);
        var second = await _client.PostAsJsonAsync("/api/checkpoints/pass", request);

        first.EnsureSuccessStatusCode();
        second.EnsureSuccessStatusCode();

        var firstResult = await first.Content.ReadFromJsonAsync<CheckpointPassResult>();
        var secondResult = await second.Content.ReadFromJsonAsync<CheckpointPassResult>();

        Assert.True(firstResult!.Recorded);
        Assert.False(secondResult!.Recorded);
        Assert.Contains("duplicate", secondResult.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BatchContainingDuplicateOfAnAlreadyStoredRead_DoesNotDoubleRecordIt()
    {
        // Bib 1077 (Maria) already has seeded "start"/"5km" passes, so this test uses her
        // "10km" split instead — a combination nothing else in this fixture touches.
        var timestamp = DateTimeOffset.Parse("2026-10-04T10:50:00Z");
        var alreadyStored = new CheckpointPassRequest(1, 1077, "10km", timestamp);
        await _client.PostAsJsonAsync("/api/checkpoints/pass", alreadyStored);

        // The arch reconnects and replays a burst that re-includes the read above twice,
        // plus one genuinely new read for a different participant.
        var batch = new BatchCheckpointPassRequest(1,
        [
            new CheckpointPassEntry(1077, "10km", timestamp),
            new CheckpointPassEntry(1077, "10km", timestamp),
            new CheckpointPassEntry(1103, "5km", DateTimeOffset.Parse("2026-10-04T10:27:00Z"))
        ]);

        var response = await _client.PostAsJsonAsync("/api/checkpoints/batch", batch);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BatchCheckpointPassResponse>();

        Assert.Equal(1, result!.Accepted);
        Assert.Equal(2, result.Duplicates);
        Assert.Equal(0, result.Rejected);
    }

    [Fact]
    public async Task UnknownBibOrCheckpoint_IsRejectedWithoutThrowing()
    {
        var batch = new BatchCheckpointPassRequest(1,
        [
            new CheckpointPassEntry(999999, "finish", DateTimeOffset.UtcNow),
            new CheckpointPassEntry(1042, "teleporter", DateTimeOffset.UtcNow)
        ]);

        var response = await _client.PostAsJsonAsync("/api/checkpoints/batch", batch);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BatchCheckpointPassResponse>();

        Assert.Equal(0, result!.Accepted);
        Assert.Equal(2, result.Rejected);
    }
}
