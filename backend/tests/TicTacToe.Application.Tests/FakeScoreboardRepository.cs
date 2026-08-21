namespace TicTacToe.Application.Tests;

internal sealed class FakeScoreboardRepository : IScoreboardRepository
{
    public int XWins { get; private set; }
    public int OWins { get; private set; }
    public int Draws { get; private set; }

    public void IncrementXWins() => XWins++;

    public void IncrementOWins() => OWins++;

    public void IncrementDraws() => Draws++;

    public void Reset()
    {
        XWins = 0;
        OWins = 0;
        Draws = 0;
    }
}
