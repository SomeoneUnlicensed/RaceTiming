using RaceTiming.Api.Domain;

namespace RaceTiming.Api.Data;

/// <summary>Seeds one demo event so the API is explorable from Swagger with no setup.</summary>
public static class DbInitializer
{
    public static void Seed(RaceTimingDbContext db)
    {
        if (db.Events.Any())
        {
            return;
        }

        // Anchored to "now" rather than a fixed calendar date, so the seeded scenario keeps
        // demonstrating the thing it's meant to show — Maria and Alex sitting at Unknown,
        // not DNF — no matter when this project is actually cloned and run.
        var startTime = DateTimeOffset.UtcNow.AddHours(-1);

        var raceEvent = new Event
        {
            Name = "City Marathon 2026",
            StartTime = startTime,
            ResultsCutoff = TimeSpan.FromHours(6)
        };

        var startCp = new Checkpoint { Name = "start", Type = CheckpointType.Start, SequenceOrder = 0, Event = raceEvent };
        var fiveKm = new Checkpoint { Name = "5km", Type = CheckpointType.Intermediate, SequenceOrder = 1, Event = raceEvent };
        var tenKm = new Checkpoint { Name = "10km", Type = CheckpointType.Intermediate, SequenceOrder = 2, Event = raceEvent };
        var finish = new Checkpoint { Name = "finish", Type = CheckpointType.Finish, SequenceOrder = 3, Event = raceEvent };
        raceEvent.Checkpoints.AddRange([startCp, fiveKm, tenKm, finish]);

        var ivan = new Participant { BibNumber = 1042, Name = "Иван Петров", Event = raceEvent };
        var maria = new Participant { BibNumber = 1077, Name = "Мария Соколова", Event = raceEvent };
        var alex = new Participant { BibNumber = 1103, Name = "Alex Johnson", Event = raceEvent };
        raceEvent.Participants.AddRange([ivan, maria, alex]);

        db.Events.Add(raceEvent);
        db.SaveChanges();

        DateTimeOffset At(TimeSpan sinceStart) => startTime + sinceStart;

        var passes = new List<CheckpointPass>
        {
            // Ivan: full set of splits, already finished ~20 minutes ago.
            Pass(ivan, startCp, At(TimeSpan.FromMinutes(0))),
            Pass(ivan, fiveKm, At(TimeSpan.FromMinutes(24))),
            Pass(ivan, tenKm, At(TimeSpan.FromMinutes(49))),
            Pass(ivan, finish, At(TimeSpan.FromMinutes(61))),

            // Maria: started and has one split, but no FINISH yet — still within the
            // 6-hour results window, so she must read as Unknown, not DNF.
            Pass(maria, startCp, At(TimeSpan.FromSeconds(5))),
            Pass(maria, fiveKm, At(TimeSpan.FromMinutes(26))),

            // Alex: never even reported a START read from the arch — also Unknown.
        };

        db.CheckpointPasses.AddRange(passes);
        db.SaveChanges();

        CheckpointPass Pass(Participant participant, Checkpoint checkpoint, DateTimeOffset timestamp) => new()
        {
            Participant = participant,
            Checkpoint = checkpoint,
            Timestamp = timestamp,
            ReceivedAt = timestamp.AddSeconds(2)
        };
    }
}
