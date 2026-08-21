using Microsoft.AspNetCore.Mvc;
using TicTacToe.Application;

namespace TicTacToe.Api.Controllers;

/// <summary>
/// Reads and resets the running win/draw tally kept across games for the life of the process.
/// </summary>
[ApiController]
[Route("api/scoreboard")]
[Produces("application/json")]
public sealed class ScoreboardController : ControllerBase
{
    private readonly ScoreboardService _scoreboardService;
    private readonly ILogger<ScoreboardController> _logger;

    public ScoreboardController(ScoreboardService scoreboardService, ILogger<ScoreboardController> logger)
    {
        _scoreboardService = scoreboardService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current scoreboard.
    /// </summary>
    /// <response code="200">The current win/draw tally.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ScoreboardDto), StatusCodes.Status200OK)]
    public ActionResult<ScoreboardDto> GetScoreboard() => Ok(_scoreboardService.GetScoreboard());

    /// <summary>
    /// Resets the scoreboard back to zero.
    /// </summary>
    /// <response code="200">The zeroed scoreboard.</response>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(ScoreboardDto), StatusCodes.Status200OK)]
    public ActionResult<ScoreboardDto> ResetScoreboard()
    {
        _scoreboardService.ResetScoreboard();
        _logger.LogInformation("Scoreboard reset");
        return Ok(_scoreboardService.GetScoreboard());
    }
}
