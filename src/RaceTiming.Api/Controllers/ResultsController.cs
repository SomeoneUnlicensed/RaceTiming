using Microsoft.AspNetCore.Mvc;
using RaceTiming.Api.Dtos;
using RaceTiming.Api.Services;

namespace RaceTiming.Api.Controllers;

[ApiController]
[Route("api/results")]
public class ResultsController(ResultsService resultsService) : ControllerBase
{
    [HttpGet("{eventId:int}")]
    public async Task<ActionResult<List<ParticipantResultDto>>> GetResults(int eventId, CancellationToken cancellationToken)
    {
        var results = await resultsService.GetResultsAsync(eventId, cancellationToken);
        return Ok(results);
    }
}
