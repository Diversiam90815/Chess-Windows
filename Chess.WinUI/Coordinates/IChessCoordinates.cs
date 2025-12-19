using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Coordinates
{
    public interface IChessCoordinate
    {
        int GetNumBoardSquares();
        PositionInstance FromIndex(int index);
        int ToIndex(PositionInstance pos, bool forDisplay = false);
        PositionInstance ToDisplayCoordinates(PositionInstance enginePos);
        PositionInstance FromDisplayCoordinates(PositionInstance displayPos);
    }
}