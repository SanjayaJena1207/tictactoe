using TicTacToe.Domain;

namespace TicTacToe.Application.Tests;

internal sealed class FakeGameRepository : IGameRepository
{
    private readonly Dictionary<Guid, Game> _games = [];

    public Game? GetById(Guid id) => _games.TryGetValue(id, out var game) ? game : null;

    public void Save(Game game) => _games[game.Id] = game;
}
