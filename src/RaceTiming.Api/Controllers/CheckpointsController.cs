using Microsoft.AspNetCore.Mvc;
using RaceTiming.Api.Dtos;
using RaceTiming.Api.Services;

namespace RaceTiming.Api.Controllers;

[ApiController]
[Route("api/checkpoints")]
public class CheckpointsController(CheckpointPassRecorder recorder) : ControllerBase
{
    /// <summary>A single read from an arch, as it arrives in real time.</summary>
    [HttpPost("pass")]
    public async Task<ActionResult<CheckpointPassResult>> RecordPass(
        [FromBody] CheckpointPassRequest request, CancellationToken cancellationToken)
    {
        var result = await recorder.RecordAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// A burst upload — e.g. an arch that lost connectivity and is replaying everything it
    /// buffered, possibly including reads already received individually. Safe to call with
    /// the same payload more than once: every row is deduplicated against what's already
    /// stored, not just against the rest of the batch.
    /// </summary>
    [HttpPost("batch")]
    public async Task<ActionResult<BatchCheckpointPassResponse>> RecordBatch(
        [FromBody] BatchCheckpointPassRequest request, CancellationToken cancellationToken)
    {
        var response = await recorder.RecordManyAsync(request.EventId, request.Passes, cancellationToken);
        return Ok(response);
    }
}
