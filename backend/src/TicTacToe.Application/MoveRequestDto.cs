using TicTacToe.Domain;

namespace TicTacToe.Application;

public sealed record MoveRequestDto(Player Player, int CellIndex);
