using TicTacToe.Domain;

namespace TicTacToe.Application.Tests;

public class GameServiceTests
{
    private readonly FakeGameRepository _repository = new();
    private readonly GameService _sut;

    public GameServiceTests()
    {
        _sut = new GameService(_repository);
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
