using System.ComponentModel.DataAnnotations;

namespace TicTacToe.Api.Contracts;

/// <summary>
/// Request body for starting a new game.
/// </summary>
/// <param name="Mode">The game mode: <c>TwoPlayer</c> or <c>VsComputer</c>.</param>
public sealed record CreateGameRequest(
    [Required(ErrorMessage = "Mode is required.")]
    [AllowedValues("TwoPlayer", "VsComputer", ErrorMessage = "Mode must be 'TwoPlayer' or 'VsComputer'.")]
    string Mode);
