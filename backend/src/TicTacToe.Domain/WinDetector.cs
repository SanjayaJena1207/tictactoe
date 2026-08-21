namespace TicTacToe.Domain;

public sealed record WinResult(bool HasWinner, Player? Winner, IReadOnlyList<int>? WinningCells)
{
    public static WinResult NoWin { get; } = new(false, null, null);

    public static WinResult Won(Player winner, int[] winningCells) => new(true, winner, winningCells);
}

public static class WinDetector
{
    private static readonly int[][] Lines =
    [
        [0, 1, 2], [3, 4, 5], [6, 7, 8], // rows
        [0, 3, 6], [1, 4, 7], [2, 5, 8], // columns
        [0, 4, 8], [2, 4, 6]             // diagonals
    ];

    public static WinResult Detect(Board board)
    {
        foreach (var line in Lines)
        {
            var first = board[line[0]];
            if (first is null)
            {
                continue;
            }

            if (board[line[1]] == first && board[line[2]] == first)
            {
                return WinResult.Won(first.Value, line);
            }
        }

        return WinResult.NoWin;
    }
}
