namespace TicTacToe.Domain;

public sealed class Game
{
    private readonly List<Move> _moveHistory = [];

    public Guid Id { get; }
    public Board Board { get; private set; }
    public GameMode GameMode { get; }
    public Player CurrentPlayer { get; private set; }
    public GameStatus Status { get; private set; }
    public Player? Winner { get; private set; }
    public IReadOnlyList<int>? WinningCells { get; private set; }
    public IReadOnlyList<Move> MoveHistory => _moveHistory;

    public Game(GameMode gameMode)
    {
        Id = Guid.NewGuid();
        GameMode = gameMode;
        Board = new Board();
        CurrentPlayer = Player.X;
        Status = GameStatus.InProgress;
    }

    public OperationResult TryApplyMove(int cellIndex, Player requestingPlayer)
    {
        if (Status != GameStatus.InProgress)
        {
            return OperationResult.Failure("Game is already finished.");
        }

        if (cellIndex < 0 || cellIndex >= Board.Size)
        {
            return OperationResult.Failure($"Cell index must be between 0 and {Board.Size - 1}.");
        }

        if (requestingPlayer != CurrentPlayer)
        {
            return OperationResult.Failure($"It is not {requestingPlayer}'s turn.");
        }

        if (!Board.IsCellEmpty(cellIndex))
        {
            return OperationResult.Failure("Cell is already occupied.");
        }

        Board = Board.PlaceMark(cellIndex, requestingPlayer);
        _moveHistory.Add(new Move(_moveHistory.Count + 1, requestingPlayer, cellIndex, DateTimeOffset.UtcNow));

        var winResult = WinDetector.Detect(Board);
        if (winResult.HasWinner)
        {
            Status = GameStatus.Won;
            Winner = winResult.Winner;
            WinningCells = winResult.WinningCells;
        }
        else if (Board.IsFull())
        {
            Status = GameStatus.Draw;
        }
        else
        {
            CurrentPlayer = requestingPlayer == Player.X ? Player.O : Player.X;
        }

        return OperationResult.Success();
    }

    public void Reset()
    {
        Board = new Board();
        _moveHistory.Clear();
        Status = GameStatus.InProgress;
        Winner = null;
        WinningCells = null;
        CurrentPlayer = Player.X;
    }
}
