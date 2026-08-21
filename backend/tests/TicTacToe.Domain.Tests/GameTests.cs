using TicTacToe.Domain;

namespace TicTacToe.Domain.Tests;

public class GameTests
{
    [Fact]
    public void ValidMove_UpdatesBoardAndSwitchesTurn()
    {
        var game = new Game(GameMode.TwoPlayer);

        var result = game.TryApplyMove(4, Player.X);

        Assert.True(result.IsSuccess);
        Assert.Equal(Player.X, game.Board[4]);
        Assert.Equal(Player.O, game.CurrentPlayer);
        Assert.Single(game.MoveHistory);
        Assert.Equal(new Move(1, Player.X, 4, game.MoveHistory[0].Timestamp), game.MoveHistory[0]);
    }

    [Fact]
    public void Move_OnOccupiedCell_IsRejectedAndTurnDoesNotChange()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.TryApplyMove(0, Player.X);

        var result = game.TryApplyMove(0, Player.O);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal(Player.O, game.CurrentPlayer);
        Assert.Equal(Player.X, game.Board[0]);
        Assert.Single(game.MoveHistory);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9)]
    public void Move_OutOfRange_IsRejected(int cellIndex)
    {
        var game = new Game(GameMode.TwoPlayer);

        var result = game.TryApplyMove(cellIndex, Player.X);

        Assert.False(result.IsSuccess);
        Assert.Empty(game.MoveHistory);
        Assert.Equal(Player.X, game.CurrentPlayer);
    }

    [Fact]
    public void Move_ByWrongPlayer_IsRejected()
    {
        var game = new Game(GameMode.TwoPlayer);

        var result = game.TryApplyMove(0, Player.O);

        Assert.False(result.IsSuccess);
        Assert.Empty(game.MoveHistory);
        Assert.Equal(Player.X, game.CurrentPlayer);
    }

    [Fact]
    public void DrawIsDetected_OnlyWhenBoardFullAndNoWinner()
    {
        var game = new Game(GameMode.TwoPlayer);

        // X | O | X
        // X | O | O
        // O | X | X
        var moves = new[]
        {
            (0, Player.X), (1, Player.O), (2, Player.X),
            (4, Player.O), (3, Player.X), (5, Player.O),
            (7, Player.X), (6, Player.O), (8, Player.X),
        };

        foreach (var (cell, player) in moves)
        {
            var result = game.TryApplyMove(cell, player);
            Assert.True(result.IsSuccess, result.ErrorMessage);
        }

        Assert.Equal(GameStatus.Draw, game.Status);
        Assert.Null(game.Winner);
        Assert.Null(game.WinningCells);
        Assert.True(game.Board.IsFull());
    }

    [Fact]
    public void NoMovesAreAccepted_AfterGameIsWon()
    {
        var game = new Game(GameMode.TwoPlayer);

        // X wins the top row: X X X / O O .
        game.TryApplyMove(0, Player.X);
        game.TryApplyMove(3, Player.O);
        game.TryApplyMove(1, Player.X);
        game.TryApplyMove(4, Player.O);
        game.TryApplyMove(2, Player.X);

        Assert.Equal(GameStatus.Won, game.Status);

        var result = game.TryApplyMove(5, Player.O);

        Assert.False(result.IsSuccess);
        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Null(game.Board[5]);
    }

    [Fact]
    public void NoMovesAreAccepted_AfterGameIsDraw()
    {
        var game = new Game(GameMode.TwoPlayer);
        var moves = new[]
        {
            (0, Player.X), (1, Player.O), (2, Player.X),
            (4, Player.O), (3, Player.X), (5, Player.O),
            (7, Player.X), (6, Player.O), (8, Player.X),
        };
        foreach (var (cell, player) in moves)
        {
            game.TryApplyMove(cell, player);
        }

        Assert.Equal(GameStatus.Draw, game.Status);

        var result = game.TryApplyMove(0, Player.O);

        Assert.False(result.IsSuccess);
        Assert.Equal(GameStatus.Draw, game.Status);
    }

    [Fact]
    public void TwoPlayerMode_Undo_RemovesOnlyMostRecentMove_TurnRevertsToThatPlayer()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.TryApplyMove(0, Player.X);
        game.TryApplyMove(1, Player.O);

        var result = game.UndoLastMove();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(game.MoveHistory);
        Assert.Equal(Player.X, game.MoveHistory[0].Player);
        Assert.Equal(Player.X, game.Board[0]);
        Assert.Null(game.Board[1]);
        Assert.Equal(Player.O, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void VsComputerMode_Undo_RemovesLastTwoMoves_TurnRevertsToX()
    {
        var game = new Game(GameMode.VsComputer);
        game.TryApplyMove(0, Player.X);
        game.TryApplyMove(4, Player.O); // simulated computer move

        var result = game.UndoLastMove();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(game.MoveHistory);
        Assert.Null(game.Board[0]);
        Assert.Null(game.Board[4]);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void VsComputerMode_Undo_WhenOnlyHumanHasMoved_RemovesJustThatOneMove()
    {
        var game = new Game(GameMode.VsComputer);
        game.TryApplyMove(0, Player.X); // computer hasn't responded yet

        var result = game.UndoLastMove();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(game.MoveHistory);
        Assert.Null(game.Board[0]);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void Undo_IsRejected_WhenGameIsWon()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.TryApplyMove(0, Player.X);
        game.TryApplyMove(3, Player.O);
        game.TryApplyMove(1, Player.X);
        game.TryApplyMove(4, Player.O);
        game.TryApplyMove(2, Player.X); // X completes the top row

        Assert.Equal(GameStatus.Won, game.Status);

        var result = game.UndoLastMove();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal(GameStatus.Won, game.Status);
        Assert.Equal(5, game.MoveHistory.Count);
    }

    [Fact]
    public void Undo_IsRejected_WhenGameIsDraw()
    {
        var game = new Game(GameMode.TwoPlayer);
        var moves = new[]
        {
            (0, Player.X), (1, Player.O), (2, Player.X),
            (4, Player.O), (3, Player.X), (5, Player.O),
            (7, Player.X), (6, Player.O), (8, Player.X),
        };
        foreach (var (cell, player) in moves)
        {
            game.TryApplyMove(cell, player);
        }

        Assert.Equal(GameStatus.Draw, game.Status);

        var result = game.UndoLastMove();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal(GameStatus.Draw, game.Status);
        Assert.Equal(9, game.MoveHistory.Count);
    }

    [Fact]
    public void Undo_IsRejected_WhenNoMovesHaveBeenMade()
    {
        var game = new Game(GameMode.TwoPlayer);

        var result = game.UndoLastMove();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void Reset_ClearsBoardHistoryAndStatus()
    {
        var game = new Game(GameMode.TwoPlayer);
        game.TryApplyMove(0, Player.X);
        game.TryApplyMove(1, Player.O);

        game.Reset();

        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal(Player.X, game.CurrentPlayer);
        Assert.Empty(game.MoveHistory);
        Assert.Null(game.Winner);
        Assert.Null(game.WinningCells);
        for (var i = 0; i < Board.Size; i++)
        {
            Assert.True(game.Board.IsCellEmpty(i));
        }
    }
}
