namespace TicTacToe.Application;

public sealed record GameStateDto(
    Guid GameId,
    string?[] Board,
    string CurrentPlayer,
    string GameMode,
    string Status,
    string? Winner,
    IReadOnlyList<int>? WinningCells,
    IReadOnlyList<MoveDto> MoveHistory);

public sealed record MoveDto(int MoveNumber, string Player, int CellIndex, DateTimeOffset Timestamp);
