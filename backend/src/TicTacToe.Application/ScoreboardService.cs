using TicTacToe.Domain;

namespace TicTacToe.Application;

public sealed class ScoreboardService
{
    private readonly IScoreboardRepository _repository;

    public ScoreboardService(IScoreboardRepository repository)
    {
        _repository = repository;
    }

    public ScoreboardDto GetScoreboard() => new(_repository.XWins, _repository.OWins, _repository.Draws);

    // Callers are responsible for invoking this exactly once per completed game -- see
    // GameService.ApplyMove, which only calls it on the single TryApplyMove call that
    // transitions a game from InProgress into Won/Draw.
    public void IncrementForResult(GameStatus status, Player? winner)
    {
        switch (status)
        {
            case GameStatus.Won when winner == Player.X:
                _repository.IncrementXWins();
                break;
            case GameStatus.Won when winner == Player.O:
                _repository.IncrementOWins();
                break;
            case GameStatus.Draw:
                _repository.IncrementDraws();
                break;
        }
    }

    public void ResetScoreboard() => _repository.Reset();
}
