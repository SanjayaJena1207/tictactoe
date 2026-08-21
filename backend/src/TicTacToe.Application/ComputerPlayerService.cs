using TicTacToe.Domain;

namespace TicTacToe.Application;

public sealed class ComputerPlayerService
{
    private const Player Computer = Player.O;
    private const Player Human = Player.X;
    private const int CenterCell = 4;
    private static readonly int[] CornerCells = [0, 2, 6, 8];

    // Caller must ensure at least one cell is empty (GameService only calls this while
    // Status is InProgress, which guarantees the board isn't full).
    public int SelectMove(Board board)
    {
        var emptyCells = Enumerable.Range(0, Board.Size).Where(board.IsCellEmpty).ToList();

        var winningCell = FindWinningCell(board, emptyCells, Computer);
        if (winningCell is not null)
        {
            return winningCell.Value;
        }

        var blockingCell = FindWinningCell(board, emptyCells, Human);
        if (blockingCell is not null)
        {
            return blockingCell.Value;
        }

        if (board.IsCellEmpty(CenterCell))
        {
            return CenterCell;
        }

        var openCorner = CornerCells.FirstOrDefault(board.IsCellEmpty, -1);
        if (openCorner != -1)
        {
            return openCorner;
        }

        return emptyCells[0];
    }

    private static int? FindWinningCell(Board board, IReadOnlyList<int> emptyCells, Player player)
    {
        foreach (var cell in emptyCells)
        {
            var candidate = WinDetector.Detect(board.PlaceMark(cell, player));
            if (candidate.HasWinner && candidate.Winner == player)
            {
                return cell;
            }
        }

        return null;
    }
}
