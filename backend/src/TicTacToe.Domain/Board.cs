namespace TicTacToe.Domain;

public sealed class Board
{
    public const int Size = 9;

    private readonly Player?[] _cells;

    public Board()
    {
        _cells = new Player?[Size];
    }

    private Board(Player?[] cells)
    {
        _cells = cells;
    }

    public IReadOnlyList<Player?> Cells => _cells;

    public Player? this[int index] => _cells[RequireInRange(index)];

    public bool IsCellEmpty(int index) => _cells[RequireInRange(index)] is null;

    public bool IsFull() => _cells.All(cell => cell.HasValue);

    // Board only enforces its own invariants (index range, cell vacancy) and throws on
    // violation; Game validates both before calling this, so these throws are a safety
    // net for misuse rather than a path expected users hit.
    public Board PlaceMark(int index, Player player)
    {
        RequireInRange(index);
        if (!IsCellEmpty(index))
        {
            throw new InvalidOperationException($"Cell {index} is already occupied.");
        }

        var newCells = (Player?[])_cells.Clone();
        newCells[index] = player;
        return new Board(newCells);
    }

    private static int RequireInRange(int index)
    {
        if (index < 0 || index >= Size)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"Cell index must be between 0 and {Size - 1}.");
        }

        return index;
    }
}
