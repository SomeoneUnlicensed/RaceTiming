namespace RaceTiming.Api.Services;

/// <summary>
/// A side effect fired after a checkpoint pass is durably recorded — e.g. push to a live
/// leaderboard, write an audit trail. Each implementation is independent and does not
/// touch the database, which is exactly what makes running them concurrently
/// (<see cref="CheckpointPassRecorder"/> uses Task.WhenAll) safe: EF Core's DbContext is
/// not thread-safe, so nothing here is allowed to share one.
/// </summary>
public interface INotificationChannel
{
    Task NotifyAsync(int eventId, int participantId, string checkpointName, CancellationToken cancellationToken);
}
