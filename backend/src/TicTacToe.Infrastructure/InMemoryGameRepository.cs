using System.Collections.Concurrent;
using TicTacToe.Application;
using TicTacToe.Domain;

namespace TicTacToe.Infrastructure;

public sealed class InMemoryGameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<Guid, Game> _games = new();

    public Game? GetById(Guid id) => _games.TryGetValue(id, out var game) ? game : null;

    public void Save(Game game) => _games[game.Id] = game;
}
