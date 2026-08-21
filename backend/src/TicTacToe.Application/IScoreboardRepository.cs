namespace TicTacToe.Application;

// Session-level, process-wide counters -- not tied to any single game.
public interface IScoreboardRepository
{
    int XWins { get; }
    int OWins { get; }
    int Draws { get; }

    void IncrementXWins();
    void IncrementOWins();
    void IncrementDraws();
    void Reset();
}
