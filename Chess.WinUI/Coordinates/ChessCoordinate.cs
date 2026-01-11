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


    public class ChessCoordinate : IChessCoordinate
    {
        public const int BOARD_SIZE = 8;

        /// <summary>
        /// Gets the number of board pieces on a chessboard
        /// </summary>
        public int GetNumBoardSquares()
        {
            return BOARD_SIZE * BOARD_SIZE;
        }

        /// <summary>
        /// Gets position from a 1D board index (engine coordinates)
        /// </summary>
        public PositionInstance FromIndex(int index)
        {
            int x = index % BOARD_SIZE;
            int y = index / BOARD_SIZE;
            return new PositionInstance(x, y);
        }

        /// <summary>
        /// Calculates the 1D array index from position
        /// </summary>
        public int ToIndex(PositionInstance pos, bool forDisplay = false)
        {
            if (forDisplay)
            {
                var displayPos = ToDisplayCoordinates(pos);
                return displayPos.y * BOARD_SIZE + displayPos.x;
            }
            return pos.y * BOARD_SIZE + pos.x;
        }

        /// <summary>
        /// Converts from engine coordinates to UI display coordinates
        /// Engine: (0,0) is bottom-left
        /// UI: (0,0) is top-left
        /// </summary>
        public PositionInstance ToDisplayCoordinates(PositionInstance enginePos)
        {
            return new PositionInstance(enginePos.x, 7 - enginePos.y);
        }

        /// <summary>
        /// Converts from UI display coordinates to engine coordinates
        /// UI: (0,0) is top-left
        /// Engine: (0,0) is bottom-left
        /// </summary>
        public PositionInstance FromDisplayCoordinates(PositionInstance displayPos)
        {
            return new PositionInstance(displayPos.x, 7 - displayPos.y);
        }
    }
}
