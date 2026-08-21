using TicTacToe.Domain;

namespace TicTacToe.Application.Tests;

public class ComputerPlayerServiceTests
{
    private readonly ComputerPlayerService _sut = new();

    [Fact]
    public void PicksWinningCell_OverBlockingCell_WhenComputerCanWin()
    {
        // O: 0,1 -> can win at 2. X: 3,4 -> could win at 5. Win must take priority over block.
        var board = new Board()
            .PlaceMark(0, Player.O)
            .PlaceMark(1, Player.O)
            .PlaceMark(3, Player.X)
            .PlaceMark(4, Player.X);

        var move = _sut.SelectMove(board);

        Assert.Equal(2, move);
    }

    [Fact]
    public void PicksBlockingCell_WhenNoWinAvailable_EvenWithCenterAndCornersOpen()
    {
        // X: 6,8 -> can win at 7 (an edge cell). Center (4) and corners 0/2 are still open,
        // so this also proves block outranks center/corner.
        var board = new Board()
            .PlaceMark(6, Player.X)
            .PlaceMark(8, Player.X)
            .PlaceMark(0, Player.O);

        var move = _sut.SelectMove(board);

        Assert.Equal(7, move);
    }

    [Fact]
    public void PicksCenter_OnEmptyBoard()
    {
        var move = _sut.SelectMove(new Board());

        Assert.Equal(4, move);
    }

    [Fact]
    public void PicksACorner_WhenCenterIsTaken_AndNoWinOrBlockAvailable()
    {
        var board = new Board().PlaceMark(4, Player.X);

        var move = _sut.SelectMove(board);

        Assert.Contains(move, new[] { 0, 2, 6, 8 });
    }

    [Fact]
    public void PicksAnyAvailableCell_WhenOnlyEdgesRemain()
    {
        // Center + all 4 corners filled, no threat on any remaining (edge) cell.
        var board = new Board()
            .PlaceMark(4, Player.O)
            .PlaceMark(0, Player.O)
            .PlaceMark(2, Player.X)
            .PlaceMark(6, Player.X)
            .PlaceMark(8, Player.O);

        var move = _sut.SelectMove(board);

        Assert.Contains(move, new[] { 1, 3, 5, 7 });
    }
}
