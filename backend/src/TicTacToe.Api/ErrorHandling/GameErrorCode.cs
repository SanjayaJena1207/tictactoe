namespace TicTacToe.Api.ErrorHandling;

/// <summary>
/// Maps a <see cref="TicTacToe.Application.ServiceResult{T}"/> validation-failure message to a
/// stable, machine-readable code so API clients can branch on the failure reason instead of
/// parsing the human-readable message.
/// </summary>
/// <remarks>
/// <see cref="TicTacToe.Application.ServiceResult{T}"/> only distinguishes NotFound from
/// ValidationFailed; the finer-grained reason below is recovered from the fixed set of
/// messages that <c>TicTacToe.Domain.Game</c> produces for <c>TryApplyMove</c> and
/// <c>UndoLastMove</c>. If those messages change, the matches here must be updated too.
/// </remarks>
internal static class GameErrorCode
{
    public const string CellOccupied = "CellOccupied";
    public const string CellIndexOutOfRange = "CellIndexOutOfRange";
    public const string NotPlayersTurn = "NotPlayersTurn";
    public const string NoMovesToUndo = "NoMovesToUndo";
    public const string GameAlreadyCompleted = "GameAlreadyCompleted";
    public const string ValidationFailed = "ValidationFailed";

    public static string Resolve(string message) => message switch
    {
        _ when message.Contains("already occupied", StringComparison.OrdinalIgnoreCase) => CellOccupied,
        _ when message.Contains("between 0 and", StringComparison.OrdinalIgnoreCase) => CellIndexOutOfRange,
        _ when message.Contains("turn", StringComparison.OrdinalIgnoreCase) => NotPlayersTurn,
        _ when message.Contains("no moves have been made", StringComparison.OrdinalIgnoreCase) => NoMovesToUndo,
        _ when message.Contains("already finished", StringComparison.OrdinalIgnoreCase) => GameAlreadyCompleted,
        _ => ValidationFailed
    };
}
