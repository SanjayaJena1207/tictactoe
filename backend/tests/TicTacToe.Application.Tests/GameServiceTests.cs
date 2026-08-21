using TicTacToe.Domain;

namespace TicTacToe.Application.Tests;

public class GameServiceTests
{
    private readonly FakeGameRepository _repository = new();
    private readonly FakeScoreboardRepository _scoreboardRepository = new();
    private readonly ScoreboardService _scoreboardService;
    private readonly GameService _sut;

    public GameServiceTests()
    {
        _scoreboardService = new ScoreboardService(_scoreboardRepository);
        _sut = new GameService(_repository, new ComputerPlayerService(), _scoreboardService);
    }

    [Fact]
    public void CreateGame_ProducesFreshInProgressGame_WithXToMove()
    {
        var dto = _sut.CreateGame(GameMode.TwoPlayer);

        Assert.NotEqual(Guid.Empty, dto.GameId);
        Assert.Equal(nameof(GameStatus.InProgress), dto.Status);
        Assert.Equal(nameof(Player.X), dto.CurrentPlayer);
        Assert.Equal(nameof(GameMode.TwoPlayer), dto.GameMode);
        Assert.Null(dto.Winner);
        Assert.Null(dto.WinningCells);
        Assert.Empty(dto.MoveHistory);
        Assert.All(dto.Board, cell => Assert.Null(cell));
        Assert.NotNull(_repository.GetById(dto.GameId));
    }

    [Fact]
    public void ApplyMove_HappyPath_ReturnsUpdatedState()
    {
        var created = _sut.CreateGame(GameMode.TwoPlayer);

        var result = _sut.ApplyMove(created.GameId, Player.X, 4);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("X", result.Value!.Board[4]);
        Assert.Equal(nameof(Player.O), result.Value.CurrentPlayer);
        Assert.Single(result.Value.MoveHistory);
        Assert.Equal(4, result.Value.MoveHistory[0].CellIndex);
        Assert.Equal("X", result.Value.MoveHistory[0].Player);
    }

    [Fact]
    public void ApplyMove_UnknownGameId_ReturnsNotFoundError()
    {
        var result = _sut.ApplyMove(Guid.NewGuid(), Player.X, 0);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(ServiceErrorReason.NotFound, result.ErrorReason);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public void ApplyMove_InvalidMove_ReturnsValidationErrorWithoutMutatingState()
    {
        var created = _sut.CreateGame(GameMode.TwoPlayer);
        _sut.ApplyMove(created.GameId, Player.X, 0);

        var result = _sut.ApplyMove(created.GameId, Player.O, 0); // cell already occupied

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorReason.ValidationFailed, result.ErrorReason);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public void ApplyMove_VsComputerMode_AutomaticallyAppliesExactlyOneComputerResponse()
    {
        var created = _sut.CreateGame(GameMode.VsComputer);

        var result = _sut.ApplyMove(created.GameId, Player.X, 0);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var dto = result.Value!;
        Assert.Equal(2, dto.MoveHistory.Count);
        Assert.Equal(nameof(Player.X), dto.MoveHistory[0].Player);
        Assert.Equal(nameof(Player.O), dto.MoveHistory[1].Player);
        Assert.Equal(nameof(Player.X), dto.CurrentPlayer);
        Assert.Equal(nameof(GameStatus.InProgress), dto.Status);
    }

    [Fact]
    public void ApplyMove_VsComputerMode_DoesNotTriggerComputerMove_WhenHumanMoveWinsTheGame()
    {
        var created = _sut.CreateGame(GameMode.VsComputer);
        var game = _repository.GetById(created.GameId)!;
        game.TryApplyMove(0, Player.X);
        game.TryApplyMove(3, Player.O);
        game.TryApplyMove(1, Player.X);
        game.TryApplyMove(4, Player.O);

        var result = _sut.ApplyMove(created.GameId, Player.X, 2); // completes the top row

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var dto = result.Value!;
        Assert.Equal(nameof(GameStatus.Won), dto.Status);
        Assert.Equal(nameof(Player.X), dto.Winner);
        Assert.Equal(5, dto.MoveHistory.Count);
    }

    [Fact]
    public void ApplyMove_VsComputerMode_DoesNotTriggerComputerMove_WhenHumanMoveEndsInDraw()
    {
        var created = _sut.CreateGame(GameMode.VsComputer);
        var game = _repository.GetById(created.GameId)!;
        game.TryApplyMove(0, Player.X);
        game.TryApplyMove(1, Player.O);
        game.TryApplyMove(2, Player.X);
        game.TryApplyMove(4, Player.O);
        game.TryApplyMove(3, Player.X);
        game.TryApplyMove(5, Player.O);
        game.TryApplyMove(7, Player.X);
        game.TryApplyMove(6, Player.O);

        var result = _sut.ApplyMove(created.GameId, Player.X, 8);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var dto = result.Value!;
        Assert.Equal(nameof(GameStatus.Draw), dto.Status);
        Assert.Equal(9, dto.MoveHistory.Count);
    }

    [Fact]
    public void ApplyMove_WhenXWinsTwoPlayerMode_IncrementsXWinsExactlyOnce()
    {
        var created = _sut.CreateGame(GameMode.TwoPlayer);
        _sut.ApplyMove(created.GameId, Player.X, 0);
        _sut.ApplyMove(created.GameId, Player.O, 3);
        _sut.ApplyMove(created.GameId, Player.X, 1);
        _sut.ApplyMove(created.GameId, Player.O, 4);

        var result = _sut.ApplyMove(created.GameId, Player.X, 2); // X completes the top row

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(new ScoreboardDto(1, 0, 0), _scoreboardService.GetScoreboard());

        // Defensive: a further move on the now-completed game must be rejected and must
        // not double-count the win.
        var repeated = _sut.ApplyMove(created.GameId, Player.O, 5);
        Assert.False(repeated.IsSuccess);
        Assert.Equal(new ScoreboardDto(1, 0, 0), _scoreboardService.GetScoreboard());
    }

    [Fact]
    public void ApplyMove_VsComputerMode_WhenComputersAutoMoveWins_IncrementsOWinsExactlyOnce()
    {
        var created = _sut.CreateGame(GameMode.VsComputer);
        var game = _repository.GetById(created.GameId)!;
        game.TryApplyMove(0, Player.X);
        game.TryApplyMove(3, Player.O);
        game.TryApplyMove(1, Player.X);
        game.TryApplyMove(4, Player.O); // O now threatens to complete 3,4,5

        var result = _sut.ApplyMove(created.GameId, Player.X, 6); // harmless human move

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var dto = result.Value!;
        Assert.Equal(nameof(GameStatus.Won), dto.Status);
        Assert.Equal(nameof(Player.O), dto.Winner);
        Assert.Equal(new ScoreboardDto(0, 1, 0), _scoreboardService.GetScoreboard());
    }

    [Fact]
    public void ApplyMove_WhenGameEndsInDraw_IncrementsDrawsExactlyOnce()
    {
        var created = _sut.CreateGame(GameMode.TwoPlayer);
        _sut.ApplyMove(created.GameId, Player.X, 0);
        _sut.ApplyMove(created.GameId, Player.O, 1);
        _sut.ApplyMove(created.GameId, Player.X, 2);
        _sut.ApplyMove(created.GameId, Player.O, 4);
        _sut.ApplyMove(created.GameId, Player.X, 3);
        _sut.ApplyMove(created.GameId, Player.O, 5);
        _sut.ApplyMove(created.GameId, Player.X, 7);
        _sut.ApplyMove(created.GameId, Player.O, 6);

        var result = _sut.ApplyMove(created.GameId, Player.X, 8);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(new ScoreboardDto(0, 0, 1), _scoreboardService.GetScoreboard());
    }

    [Fact]
    public void ResetGame_DoesNotAffectScoreboard()
    {
        var created = _sut.CreateGame(GameMode.TwoPlayer);
        _sut.ApplyMove(created.GameId, Player.X, 0);
        _sut.ApplyMove(created.GameId, Player.O, 3);
        _sut.ApplyMove(created.GameId, Player.X, 1);
        _sut.ApplyMove(created.GameId, Player.O, 4);
        _sut.ApplyMove(created.GameId, Player.X, 2); // X wins
        var scoreboardBefore = _scoreboardService.GetScoreboard();

        _sut.ResetGame(created.GameId);

        Assert.Equal(scoreboardBefore, _scoreboardService.GetScoreboard());
    }

    [Fact]
    public void MultipleGamesInSequence_AccumulateScoreboardCorrectly()
    {
        var game1 = _sut.CreateGame(GameMode.TwoPlayer); // X wins
        _sut.ApplyMove(game1.GameId, Player.X, 0);
        _sut.ApplyMove(game1.GameId, Player.O, 3);
        _sut.ApplyMove(game1.GameId, Player.X, 1);
        _sut.ApplyMove(game1.GameId, Player.O, 4);
        _sut.ApplyMove(game1.GameId, Player.X, 2);

        var game2 = _sut.CreateGame(GameMode.TwoPlayer); // O wins
        _sut.ApplyMove(game2.GameId, Player.X, 0);
        _sut.ApplyMove(game2.GameId, Player.O, 3);
        _sut.ApplyMove(game2.GameId, Player.X, 1);
        _sut.ApplyMove(game2.GameId, Player.O, 4);
        _sut.ApplyMove(game2.GameId, Player.X, 8);
        _sut.ApplyMove(game2.GameId, Player.O, 5);

        var game3 = _sut.CreateGame(GameMode.TwoPlayer); // draw
        _sut.ApplyMove(game3.GameId, Player.X, 0);
        _sut.ApplyMove(game3.GameId, Player.O, 1);
        _sut.ApplyMove(game3.GameId, Player.X, 2);
        _sut.ApplyMove(game3.GameId, Player.O, 4);
        _sut.ApplyMove(game3.GameId, Player.X, 3);
        _sut.ApplyMove(game3.GameId, Player.O, 5);
        _sut.ApplyMove(game3.GameId, Player.X, 7);
        _sut.ApplyMove(game3.GameId, Player.O, 6);
        _sut.ApplyMove(game3.GameId, Player.X, 8);

        Assert.Equal(new ScoreboardDto(1, 1, 1), _scoreboardService.GetScoreboard());
    }

    [Fact]
    public void Undo_TwoPlayerMode_RemovesOnlyLastMove()
    {
        var created = _sut.CreateGame(GameMode.TwoPlayer);
        _sut.ApplyMove(created.GameId, Player.X, 0);
        _sut.ApplyMove(created.GameId, Player.O, 1);

        var result = _sut.Undo(created.GameId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var dto = result.Value!;
        Assert.Single(dto.MoveHistory);
        Assert.Equal("X", dto.Board[0]);
        Assert.Null(dto.Board[1]);
        Assert.Equal(nameof(Player.O), dto.CurrentPlayer);
    }

    [Fact]
    public void Undo_VsComputerMode_RemovesLastTwoMoves()
    {
        var created = _sut.CreateGame(GameMode.VsComputer);
        _sut.ApplyMove(created.GameId, Player.X, 0);
        _sut.ApplyMove(created.GameId, Player.O, 4); // simulated computer move

        var result = _sut.Undo(created.GameId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var dto = result.Value!;
        Assert.Empty(dto.MoveHistory);
        Assert.All(dto.Board, cell => Assert.Null(cell));
        Assert.Equal(nameof(Player.X), dto.CurrentPlayer);
    }

    [Fact]
    public void Undo_VsComputerMode_WhenOnlyHumanHasMoved_RemovesJustThatOneMove()
    {
        var created = _sut.CreateGame(GameMode.VsComputer);
        _sut.ApplyMove(created.GameId, Player.X, 0); // computer hasn't responded yet

        var result = _sut.Undo(created.GameId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var dto = result.Value!;
        Assert.Empty(dto.MoveHistory);
        Assert.All(dto.Board, cell => Assert.Null(cell));
        Assert.Equal(nameof(Player.X), dto.CurrentPlayer);
    }

    [Fact]
    public void Undo_RejectedWithClearError_WhenGameStatusIsWon()
    {
        var created = _sut.CreateGame(GameMode.TwoPlayer);
        _sut.ApplyMove(created.GameId, Player.X, 0);
        _sut.ApplyMove(created.GameId, Player.O, 3);
        _sut.ApplyMove(created.GameId, Player.X, 1);
        _sut.ApplyMove(created.GameId, Player.O, 4);
        _sut.ApplyMove(created.GameId, Player.X, 2); // X completes the top row

        var result = _sut.Undo(created.GameId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorReason.ValidationFailed, result.ErrorReason);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public void Undo_RejectedWithClearError_WhenMoveHistoryIsEmpty()
    {
        var created = _sut.CreateGame(GameMode.TwoPlayer);

        var result = _sut.Undo(created.GameId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorReason.ValidationFailed, result.ErrorReason);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public void Undo_UnknownGameId_ReturnsNotFoundError()
    {
        var result = _sut.Undo(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorReason.NotFound, result.ErrorReason);
    }

    [Fact]
    public void ResetGame_ClearsBoardHistoryAndStatus_ButKeepsSameGameId()
    {
        var created = _sut.CreateGame(GameMode.TwoPlayer);
        _sut.ApplyMove(created.GameId, Player.X, 0);
        _sut.ApplyMove(created.GameId, Player.O, 1);

        var result = _sut.ResetGame(created.GameId);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal(created.GameId, dto.GameId);
        Assert.Equal(nameof(GameStatus.InProgress), dto.Status);
        Assert.Equal(nameof(Player.X), dto.CurrentPlayer);
        Assert.Empty(dto.MoveHistory);
        Assert.All(dto.Board, cell => Assert.Null(cell));
    }

    [Fact]
    public void ResetGame_UnknownGameId_ReturnsNotFoundError()
    {
        var result = _sut.ResetGame(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorReason.NotFound, result.ErrorReason);
    }

    [Fact]
    public void GetGame_ReturnsMappedState_ForKnownGame()
    {
        var created = _sut.CreateGame(GameMode.VsComputer);

        var result = _sut.GetGame(created.GameId);

        Assert.True(result.IsSuccess);
        Assert.Equal(created.GameId, result.Value!.GameId);
        Assert.Equal(nameof(GameMode.VsComputer), result.Value.GameMode);
    }
}
