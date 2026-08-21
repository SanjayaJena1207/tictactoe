using TicTacToe.Domain;

namespace TicTacToe.Application;

public interface IGameRepository
{
    Game? GetById(Guid id);

    void Save(Game game);
}
