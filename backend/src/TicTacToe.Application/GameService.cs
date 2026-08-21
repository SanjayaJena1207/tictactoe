using TicTacToe.Domain;

namespace TicTacToe.Application;

public sealed class GameService
{
    private readonly IGameRepository _repository;
    private readonly ComputerPlayerService _computerPlayer;

    public GameService(IGameRepository repository, ComputerPlayerService computerPlayer)
    {
        _repository = repository;
        _computerPlayer = computerPlayer;
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

        // Only the human's move ever leaves the computer (O) on the clock; if the human's
        // move already ended the game, CurrentPlayer no longer advances and this is skipped.
        if (game.GameMode == GameMode.VsComputer
            && game.Status == GameStatus.InProgress
            && game.CurrentPlayer == Player.O)
        {
            game.TryApplyMove(_computerPlayer.SelectMove(game.Board), Player.O);
        }

        _repository.Save(game);
        return ServiceResult<GameStateDto>.Success(game.ToDto());
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
