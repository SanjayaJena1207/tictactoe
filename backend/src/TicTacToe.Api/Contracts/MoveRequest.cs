using System.ComponentModel.DataAnnotations;

namespace TicTacToe.Api.Contracts;

/// <summary>
/// Request body for submitting a move.
/// </summary>
/// <param name="Player">The player making the move: <c>X</c> or <c>O</c>.</param>
/// <param name="CellIndex">The zero-based board cell (0-8) to place the mark in.</param>
public sealed record MoveRequest(
    [Required(ErrorMessage = "Player is required.")]
    [AllowedValues("X", "O", ErrorMessage = "Player must be 'X' or 'O'.")]
    string Player,

    [Range(0, 8, ErrorMessage = "CellIndex must be between 0 and 8.")]
    int CellIndex);
