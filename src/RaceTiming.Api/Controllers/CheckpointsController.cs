using Microsoft.AspNetCore.Mvc;
using RaceTiming.Api.Dtos;
using RaceTiming.Api.Services;

namespace RaceTiming.Api.Controllers;

[ApiController]
[Route("api/checkpoints")]
public class CheckpointsController(CheckpointPassRecorder recorder) : ControllerBase
{
    /// <summary>One checkpoint read, as it arrives in real time.</summary>
    [HttpPost("pass")]
    public async Task<ActionResult<CheckpointPassResult>> RecordPass(
        [FromBody] CheckpointPassRequest request, CancellationToken cancellationToken)
    {
        var result = await recorder.RecordAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Burst upload for reads buffered by an arch during a connectivity gap. Safe
    /// to retry with the same payload: rows are deduplicated against stored data too.</summary>
    [HttpPost("batch")]
    public async Task<ActionResult<BatchCheckpointPassResponse>> RecordBatch(
        [FromBody] BatchCheckpointPassRequest request, CancellationToken cancellationToken)
    {
        var response = await recorder.RecordManyAsync(request.EventId, request.Passes, cancellationToken);
        return Ok(response);
    }
}
