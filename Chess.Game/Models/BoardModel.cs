using Chess.UI.Board;
using Chess.UI.Services;
using System.Collections.Generic;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Models
{
    public interface IBoardModel
    {
        PieceType[] GetBoardStateFromNative();
        BoardSquare GetSquare(Square square);
        Dictionary<Square, PieceType> UpdateBoardState();
        BoardSquare[] GetAllSquares();
        void SyncBoardStateFromNative();
    }


    public class BoardModel : IBoardModel
    {
        private readonly PieceType[] _currentBoardState;

        private readonly BoardSquare[] _squares;


        public BoardModel()
        {
            _currentBoardState = new PieceType[64];
            _squares = new BoardSquare[64];

            // Initialize all 64 squares
            for (int i = 0; i < 64; i++)
            {
                Square square = (Square)i;
                _squares[i] = new BoardSquare(square, PieceType.None);
            }
        }


        public PieceType[] GetBoardStateFromNative()
        {
            int[] rawBoard = new int[64]; // pre-allocated array
            EngineAPI.GetBoardState(rawBoard);

            PieceType[] board = new PieceType[64];
            for (int i = 0; i < 64; ++i)
            {
                board[i] = (PieceType)rawBoard[i];
            }

            return board;
        }


        public BoardSquare GetSquare(Square square)
        {
            if (square == Square.None || (int)square < 0 || (int)square >= 64)
                return null;

            return _squares[(int)square];
        }


        public BoardSquare[] GetAllSquares()
        {
            return _squares;
        }



        public Dictionary<Square, PieceType> UpdateBoardState()
        {
            PieceType[] newBoardState = GetBoardStateFromNative();    // Get the latest board state
            Dictionary<Square, PieceType> changedSquares = new Dictionary<Square, PieceType>();

            for (int i = 0; i < 64; i++)
            {
                if (_currentBoardState[i] != newBoardState[i])
                {
                    Square square = (Square)i;
                    PieceType newPiece = newBoardState[i];

                    changedSquares[square] = newPiece;
                    _currentBoardState[i] = newPiece;

                    // Update the BoardSquare object
                    UpdateSquarePiece(square, newPiece);
                }
            }

            return changedSquares;
        }


        private void UpdateSquarePiece(Square square, PieceType piece)
        {
            var boardSquare = _squares[(int)square];
            boardSquare.Piece = piece;
        }


        public void SyncBoardStateFromNative()
        {
            PieceType[] freshState = GetBoardStateFromNative();

            for (int i = 0; i < 64; ++i)
            {
                _currentBoardState[i] = freshState[i];
                _squares[i].Piece = freshState[i];
            }
        }

    }
}
