using TicTacToe.Domain;

namespace TicTacToe.Application;

public static class GameStateMapper
{
    public static GameStateDto ToDto(this Game game)
    {
        var board = game.Board.Cells.Select(cell => cell?.ToString()).ToArray();
        var moveHistory = game.MoveHistory
            .Select(move => new MoveDto(move.MoveNumber, move.Player.ToString(), move.CellIndex, move.Timestamp))
            .ToList();

        return new GameStateDto(
            game.Id,
            board,
            game.CurrentPlayer.ToString(),
            game.GameMode.ToString(),
            game.Status.ToString(),
            game.Winner?.ToString(),
            game.WinningCells,
            moveHistory);
    }
}
