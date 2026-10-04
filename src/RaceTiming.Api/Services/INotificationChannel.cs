namespace RaceTiming.Api.Services;

/// <summary>A side effect fired after a pass is recorded (audit log, cache invalidation).
/// Implementations must not touch the DbContext — <see cref="CheckpointPassRecorder"/> runs
/// them concurrently via Task.WhenAll.</summary>
public interface INotificationChannel
{
    Task NotifyAsync(int eventId, int participantId, string checkpointName, CancellationToken cancellationToken);
}
