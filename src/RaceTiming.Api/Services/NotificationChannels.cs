namespace RaceTiming.Api.Services;

public sealed class AuditLogChannel(ILogger<AuditLogChannel> logger) : INotificationChannel
{
    public Task NotifyAsync(int eventId, int participantId, string checkpointName, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Audit: event {EventId} participant {ParticipantId} passed checkpoint '{Checkpoint}'",
            eventId, participantId, checkpointName);
        return Task.CompletedTask;
    }
}

/// <summary>Invalidates the cached results view so the next GET recomputes it.</summary>
public sealed class LeaderboardCacheChannel(Microsoft.Extensions.Caching.Memory.IMemoryCache cache) : INotificationChannel
{
    public Task NotifyAsync(int eventId, int participantId, string checkpointName, CancellationToken cancellationToken)
    {
        cache.Remove(ResultsCacheKeys.ForEvent(eventId));
        return Task.CompletedTask;
    }
}

public static class ResultsCacheKeys
{
    public static string ForEvent(int eventId) => $"results:{eventId}";
}
