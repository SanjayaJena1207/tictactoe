using TicTacToe.Application;

namespace TicTacToe.Infrastructure;

// Registered as a singleton: one instance shared across all requests for the process's
// lifetime, so the counters below are updated concurrently and must be lock-free/atomic.
public sealed class InMemoryScoreboardRepository : IScoreboardRepository
{
    private int _xWins;
    private int _oWins;
    private int _draws;

    public int XWins => Volatile.Read(ref _xWins);
    public int OWins => Volatile.Read(ref _oWins);
    public int Draws => Volatile.Read(ref _draws);

    public void IncrementXWins() => Interlocked.Increment(ref _xWins);

    public void IncrementOWins() => Interlocked.Increment(ref _oWins);

    public void IncrementDraws() => Interlocked.Increment(ref _draws);

    public void Reset()
    {
        Volatile.Write(ref _xWins, 0);
        Volatile.Write(ref _oWins, 0);
        Volatile.Write(ref _draws, 0);
    }
}
