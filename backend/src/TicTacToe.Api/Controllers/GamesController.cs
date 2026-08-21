using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Contracts;
using TicTacToe.Api.ErrorHandling;
using TicTacToe.Application;
using TicTacToe.Domain;

namespace TicTacToe.Api.Controllers;

/// <summary>
/// Create, play, undo, and reset Tic-Tac-Toe games.
/// </summary>
[ApiController]
[Route("api/games")]
[Produces("application/json")]
public sealed class GamesController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(GameService gameService, ILogger<GamesController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// Starts a new game.
    /// </summary>
    /// <param name="request">The desired game mode.</param>
    /// <response code="201">The game was created.</response>
    /// <response code="400">The request body was missing or the mode was invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GameStateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<GameStateDto> CreateGame([FromBody] CreateGameRequest request)
    {
        var mode = Enum.Parse<GameMode>(request.Mode);
        var game = _gameService.CreateGame(mode);

        _logger.LogInformation("Created game {GameId} with mode {Mode}", game.GameId, mode);

        return CreatedAtAction(nameof(GetGame), new { id = game.GameId }, game);
    }

    /// <summary>
    /// Gets the current state of a game.
    /// </summary>
    /// <param name="id">The game identifier.</param>
    /// <response code="200">The current game state.</response>
    /// <response code="404">No game exists with the given id.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GameStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<GameStateDto> GetGame(Guid id)
    {
        var result = _gameService.GetGame(id);
        return result.IsSuccess ? Ok(result.Value) : ToErrorResult(result);
    }

    /// <summary>
    /// Applies a move for the given player.
    /// </summary>
    /// <param name="id">The game identifier.</param>
    /// <param name="request">The player and cell index of the move.</param>
    /// <remarks>
    /// In <c>VsComputer</c> mode, if the human's move does not end the game, the computer's
    /// reply move is applied automatically and reflected in the response.
    /// </remarks>
    /// <response code="200">The move was applied; the response reflects the resulting state.</response>
    /// <response code="400">
    /// The move was invalid: the cell was occupied, the cell index was out of range, it
    /// wasn't the requesting player's turn, or the game had already finished.
    /// </response>
    /// <response code="404">No game exists with the given id.</response>
    [HttpPost("{id:guid}/moves")]
    [ProducesResponseType(typeof(GameStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<GameStateDto> ApplyMove(Guid id, [FromBody] MoveRequest request)
    {
        var player = Enum.Parse<Player>(request.Player);
        var result = _gameService.ApplyMove(id, player, request.CellIndex);
        if (!result.IsSuccess)
        {
            return ToErrorResult(result);
        }

        _logger.LogInformation("Move applied to game {GameId}: {Player} -> cell {CellIndex}", id, player, request.CellIndex);
        return Ok(result.Value);
    }

    /// <summary>
    /// Undoes the most recent move. In <c>VsComputer</c> mode this undoes the computer's
    /// reply together with the human move that triggered it, as a single step.
    /// </summary>
    /// <param name="id">The game identifier.</param>
    /// <response code="200">The undo succeeded.</response>
    /// <response code="400">There are no moves to undo, or the game has already finished.</response>
    /// <response code="404">No game exists with the given id.</response>
    [HttpPost("{id:guid}/undo")]
    [ProducesResponseType(typeof(GameStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<GameStateDto> Undo(Guid id)
    {
        var result = _gameService.Undo(id);
        if (!result.IsSuccess)
        {
            return ToErrorResult(result);
        }

        _logger.LogInformation("Undo applied to game {GameId}", id);
        return Ok(result.Value);
    }

    /// <summary>
    /// Resets a game to a fresh, empty board while keeping the same game id.
    /// </summary>
    /// <param name="id">The game identifier.</param>
    /// <response code="200">The game was reset.</response>
    /// <response code="404">No game exists with the given id.</response>
    [HttpPost("{id:guid}/reset")]
    [ProducesResponseType(typeof(GameStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<GameStateDto> Reset(Guid id)
    {
        var result = _gameService.ResetGame(id);
        if (!result.IsSuccess)
        {
            return ToErrorResult(result);
        }

        _logger.LogInformation("Game {GameId} reset", id);
        return Ok(result.Value);
    }

    private ObjectResult ToErrorResult(ServiceResult<GameStateDto> result)
    {
        var message = result.ErrorMessage!;

        if (result.ErrorReason == ServiceErrorReason.NotFound)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Game not found",
                Detail = message
            });
        }

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid game action",
            Detail = message
        };
        problem.Extensions["reason"] = GameErrorCode.Resolve(message);

        return BadRequest(problem);
    }
}
