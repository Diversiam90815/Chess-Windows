using Chess.UI.Services;
using Chess.UI.Coordinates;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;

namespace Chess.UI.Board
{
    public class BoardModel : IBoardModel
    {
        private int[] _currentBoardState;

        private readonly IChessCoordinate _coordinate;


        public BoardModel()
        {
            _coordinate = App.Current.Services.GetService<IChessCoordinate>();
        }


        public int[] GetBoardStateFromNative()
        {
            int[] board = new int[64]; // pre-allocated array

            EngineAPI.GetBoardState(board);
            return board;
        }


        public (Dictionary<int, int>, int[]) UpdateBoardState()
        {
            int[] newBoardState = GetBoardStateFromNative();    // Get the latest board state

            // Dictionary to track changes: <Index, New Value>
            Dictionary<int, int> changedSquares = new Dictionary<int, int>();

            if (_currentBoardState == null) // If current board is not initialized, all squares are considered changed
            {
                _currentBoardState = new int[64];
                for (int i = 0; i < newBoardState.Length; i++)
                {
                    changedSquares[i] = newBoardState[i];
                    _currentBoardState[i] = newBoardState[i];
                }
            }
            else
            {
                // Compare each square and identify changes
                for (int i = 0; i < newBoardState.Length; i++)
                {
                    if (_currentBoardState[i] != newBoardState[i])
                    {
                        changedSquares[i] = newBoardState[i];
                        _currentBoardState[i] = newBoardState[i];
                    }
                }
            }
            return (changedSquares, newBoardState);
        }


        public BoardSquare DecodeBoardState(int index, int[] boardState)
        {
            int encoded = boardState[index];

            // Decode color and piece
            int colorVal = (encoded >> 4) & 0xF;    // top 8 bits
            int pieceVal = encoded & 0xF;          // bottom 8 bits

            PositionInstance enginePos = _coordinate.FromIndex(index);  // Get engine pos from index
            PositionInstance displayPos = _coordinate.ToDisplayCoordinates(enginePos); // Convert it to the UI pos

            var square = new BoardSquare(
                x: displayPos.x,
                y: displayPos.y,
                (PieceTypeInstance)pieceVal,
                (PlayerColor)colorVal
            );

            return square;
        }
    }
}
