namespace TicTacToe.Domain;

public sealed record Move(int MoveNumber, Player Player, int CellIndex, DateTimeOffset Timestamp);
