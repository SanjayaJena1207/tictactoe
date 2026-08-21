using TicTacToe.Domain;

namespace TicTacToe.Application.Tests;

public class ScoreboardServiceTests
{
    private readonly FakeScoreboardRepository _repository = new();
    private readonly ScoreboardService _sut;

    public ScoreboardServiceTests()
    {
        _sut = new ScoreboardService(_repository);
    }

    [Fact]
    public void GetScoreboard_StartsAtZero()
    {
        var scoreboard = _sut.GetScoreboard();

        Assert.Equal(new ScoreboardDto(0, 0, 0), scoreboard);
    }

    [Fact]
    public void IncrementForResult_WonByX_IncrementsOnlyXWins()
    {
        _sut.IncrementForResult(GameStatus.Won, Player.X);

        Assert.Equal(new ScoreboardDto(1, 0, 0), _sut.GetScoreboard());
    }

    [Fact]
    public void IncrementForResult_WonByO_IncrementsOnlyOWins()
    {
        _sut.IncrementForResult(GameStatus.Won, Player.O);

        Assert.Equal(new ScoreboardDto(0, 1, 0), _sut.GetScoreboard());
    }

    [Fact]
    public void IncrementForResult_Draw_IncrementsOnlyDraws()
    {
        _sut.IncrementForResult(GameStatus.Draw, null);

        Assert.Equal(new ScoreboardDto(0, 0, 1), _sut.GetScoreboard());
    }

    [Fact]
    public void IncrementForResult_InProgress_DoesNotIncrementAnything()
    {
        _sut.IncrementForResult(GameStatus.InProgress, null);

        Assert.Equal(new ScoreboardDto(0, 0, 0), _sut.GetScoreboard());
    }

    [Fact]
    public void ResetScoreboard_ZeroesAllCounters()
    {
        _sut.IncrementForResult(GameStatus.Won, Player.X);
        _sut.IncrementForResult(GameStatus.Won, Player.O);
        _sut.IncrementForResult(GameStatus.Draw, null);

        _sut.ResetScoreboard();

        Assert.Equal(new ScoreboardDto(0, 0, 0), _sut.GetScoreboard());
    }
}
