using Microsoft.EntityFrameworkCore;
using RaceTiming.Api.Domain;

namespace RaceTiming.Api.Data;

public class RaceTimingDbContext(DbContextOptions<RaceTimingDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Checkpoint> Checkpoints => Set<Checkpoint>();
    public DbSet<CheckpointPass> CheckpointPasses => Set<CheckpointPass>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Participant>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.HasIndex(x => new { x.EventId, x.BibNumber }).IsUnique();
            e.HasOne(x => x.Event)
                .WithMany(x => x.Participants)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Checkpoint>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(x => new { x.EventId, x.Name }).IsUnique();
            e.HasOne(x => x.Event)
                .WithMany(x => x.Checkpoints)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CheckpointPass>(e =>
        {
            // Idempotency at the storage layer: a replayed read can never persist twice.
            e.HasIndex(x => new { x.ParticipantId, x.CheckpointId, x.Timestamp }).IsUnique();
            e.HasOne(x => x.Participant)
                .WithMany(x => x.Passes)
                .HasForeignKey(x => x.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Checkpoint)
                .WithMany(x => x.Passes)
                .HasForeignKey(x => x.CheckpointId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
