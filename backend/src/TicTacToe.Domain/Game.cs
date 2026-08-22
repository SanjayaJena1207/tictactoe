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

        RecomputeStatus();
        if (Status == GameStatus.InProgress)
        {
            CurrentPlayer = requestingPlayer == Player.X ? Player.O : Player.X;
        }

        return OperationResult.Success();
    }

    // Two Player Mode undoes one move (turn reverts to whoever made it); Vs Computer
    // Mode undoes the computer's move plus the human's preceding move as one atomic
    // step (turn reverts to X), except when only the human has moved so far, where
    // there is no computer move yet to pair it with...
    public OperationResult UndoLastMove()
    {
        if (Status != GameStatus.InProgress)
        {
            return OperationResult.Failure("Cannot undo: the game is already finished.");
        }

        if (_moveHistory.Count == 0)
        {
            return OperationResult.Failure("Cannot undo: no moves have been made yet.");
        }

        var movesToRemove = GameMode == GameMode.VsComputer ? Math.Min(2, _moveHistory.Count) : 1;
        _moveHistory.RemoveRange(_moveHistory.Count - movesToRemove, movesToRemove);

        Board = Replay(_moveHistory);
        RecomputeStatus();
        CurrentPlayer = _moveHistory.Count == 0
            ? Player.X
            : _moveHistory[^1].Player == Player.X ? Player.O : Player.X;

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

    private void RecomputeStatus()
    {
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
            Status = GameStatus.InProgress;
            Winner = null;
            WinningCells = null;
        }
    }

    private static Board Replay(IEnumerable<Move> moves)
    {
        var board = new Board();
        foreach (var move in moves)
        {
            board = board.PlaceMark(move.CellIndex, move.Player);
        }

        return board;
    }
}
