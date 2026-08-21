using TicTacToe.Domain;

namespace TicTacToe.Application;

public sealed class GameService
{
    private readonly IGameRepository _repository;
    private readonly ComputerPlayerService _computerPlayer;
    private readonly ScoreboardService _scoreboardService;

    public GameService(IGameRepository repository, ComputerPlayerService computerPlayer, ScoreboardService scoreboardService)
    {
        _repository = repository;
        _computerPlayer = computerPlayer;
        _scoreboardService = scoreboardService;
    }

    public GameStateDto CreateGame(GameMode mode)
    {
        var game = new Game(mode);
        _repository.Save(game);
        return game.ToDto();
    }

    public ServiceResult<GameStateDto> GetGame(Guid gameId)
    {
        var game = _repository.GetById(gameId);
        return game is null
            ? NotFound(gameId)
            : ServiceResult<GameStateDto>.Success(game.ToDto());
    }

    public ServiceResult<GameStateDto> ApplyMove(Guid gameId, Player player, int cellIndex)
    {
        var game = _repository.GetById(gameId);
        if (game is null)
        {
            return NotFound(gameId);
        }

        var result = game.TryApplyMove(cellIndex, player);
        if (!result.IsSuccess)
        {
            return ServiceResult<GameStateDto>.Failure(result.ErrorMessage!, ServiceErrorReason.ValidationFailed);
        }

        RecordResultIfGameJustEnded(game);

        // Only the human's move ever leaves the computer (O) on the clock; if the human's
        // move already ended the game, CurrentPlayer no longer advances and this is skipped.
        if (game.GameMode == GameMode.VsComputer
            && game.Status == GameStatus.InProgress
            && game.CurrentPlayer == Player.O)
        {
            var computerResult = game.TryApplyMove(_computerPlayer.SelectMove(game.Board), Player.O);
            if (computerResult.IsSuccess)
            {
                RecordResultIfGameJustEnded(game);
            }
        }

        _repository.Save(game);
        return ServiceResult<GameStateDto>.Success(game.ToDto());
    }

    // TryApplyMove only mutates state (and only ever reaches Won/Draw) when called while
    // Status is InProgress -- a move on an already-finished game fails validation before
    // this point is ever reached. So the only calls that land here with Status now Won/Draw
    // are the exact, one-time transition into that status; there is no way to double-count.
    private void RecordResultIfGameJustEnded(Game game)
    {
        if (game.Status is GameStatus.Won or GameStatus.Draw)
        {
            _scoreboardService.IncrementForResult(game.Status, game.Winner);
        }
    }

    public ServiceResult<GameStateDto> Undo(Guid gameId)
    {
        var game = _repository.GetById(gameId);
        if (game is null)
        {
            return NotFound(gameId);
        }

        var result = game.UndoLastMove();
        if (!result.IsSuccess)
        {
            return ServiceResult<GameStateDto>.Failure(result.ErrorMessage!, ServiceErrorReason.ValidationFailed);
        }

        _repository.Save(game);
        return ServiceResult<GameStateDto>.Success(game.ToDto());
    }

    public ServiceResult<GameStateDto> ResetGame(Guid gameId)
    {
        var game = _repository.GetById(gameId);
        if (game is null)
        {
            return NotFound(gameId);
        }

        game.Reset();
        _repository.Save(game);
        return ServiceResult<GameStateDto>.Success(game.ToDto());
    }

    private static ServiceResult<GameStateDto> NotFound(Guid gameId) =>
        ServiceResult<GameStateDto>.Failure($"Game '{gameId}' was not found.", ServiceErrorReason.NotFound);
}
