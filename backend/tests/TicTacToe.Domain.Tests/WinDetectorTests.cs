using TicTacToe.Domain;

namespace TicTacToe.Domain.Tests;

public class WinDetectorTests
{
    private static Game PlaySequence(params (int Cell, Player Player)[] moves)
    {
        var game = new Game(GameMode.TwoPlayer);
        foreach (var (cell, player) in moves)
        {
            var result = game.TryApplyMove(cell, player);
            Assert.True(result.IsSuccess, result.ErrorMessage);
        }

        return game;
    }

    [Theory]
    [InlineData(0, 1, 2)]
    [InlineData(3, 4, 5)]
    [InlineData(6, 7, 8)]
    public void RowWin_IsDetectedWithCorrectWinningCells(int a, int b, int c)
    {
        // X takes the target row across three turns; O plays elsewhere and never completes a line.
        var otherCells = Enumerable.Range(0, 9).Except([a, b, c]).ToArray();
        var game = PlaySequence(
            (a, Player.X), (otherCells[0], Player.O),
            (b, Player.X), (otherCells[1], Player.O),
            (c, Player.X));

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(new[] { a, b, c }, game.WinningCells);
    }

    [Theory]
    [InlineData(0, 3, 6)]
    [InlineData(1, 4, 7)]
    [InlineData(2, 5, 8)]
    public void ColumnWin_IsDetectedWithCorrectWinningCells(int a, int b, int c)
    {
        var otherCells = Enumerable.Range(0, 9).Except([a, b, c]).ToArray();
        var game = PlaySequence(
            (a, Player.X), (otherCells[0], Player.O),
            (b, Player.X), (otherCells[1], Player.O),
            (c, Player.X));

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(new[] { a, b, c }, game.WinningCells);
    }

    [Fact]
    public void MainDiagonalWin_IsDetectedWithCorrectWinningCells()
    {
        // X: 0, 4, 8 | O: 1, 2
        var game = PlaySequence(
            (0, Player.X), (1, Player.O),
            (4, Player.X), (2, Player.O),
            (8, Player.X));

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(new[] { 0, 4, 8 }, game.WinningCells);
    }

    [Fact]
    public void AntiDiagonalWin_IsDetectedWithCorrectWinningCells()
    {
        // X: 2, 4, 6 | O: 0, 1
        var game = PlaySequence(
            (2, Player.X), (0, Player.O),
            (4, Player.X), (1, Player.O),
            (6, Player.X));

        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(Player.X, game.Winner);
        Assert.Equal(new[] { 2, 4, 6 }, game.WinningCells);
    }

    [Fact]
    public void Detect_ReturnsNoWin_ForEmptyBoard()
    {
        var result = WinDetector.Detect(new Board());

        Assert.False(result.HasWinner);
        Assert.Null(result.Winner);
        Assert.Null(result.WinningCells);
    }
}
