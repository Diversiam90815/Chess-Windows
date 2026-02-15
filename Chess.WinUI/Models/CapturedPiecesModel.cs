using Chess.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT.ChessVtableClasses;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Models
{
    public interface ICapturedPiecesModel
    {
        Dictionary<PieceType, int> CapturedPieces { get; }
        event Action<PlayerCapturedPiece> PieceCaptured;
        event Action CapturedPiecesCleared;
    }

    public class CapturedPiecesModel : ICapturedPiecesModel
    {
        private readonly ICommunicationLayer _backendCommunication;

        public Dictionary<PieceType, int> CapturedPieces { get; } = new()
        {
            { PieceType.WPawn, 0 },
            { PieceType.WBishop, 0 },
            { PieceType.WKnight, 0 },
            { PieceType.WRook, 0 },
            { PieceType.WQueen, 0 },
            { PieceType.BPawn, 0 },
            { PieceType.BBishop, 0 },
            { PieceType.BKnight, 0 },
            { PieceType.BRook, 0 },
            { PieceType.BQueen, 0 }
        };

        public event Action<PlayerCapturedPiece> PieceCaptured;
        public event Action CapturedPiecesCleared;


        public CapturedPiecesModel(ICommunicationLayer communicationLayer)
        {
            _backendCommunication = communicationLayer;
            // TODO:
            //_backendCommunication.PieceCaptured += OnPieceCaptured;
        }

        private void OnPieceCaptured(PlayerCapturedPiece piece)
        {
            if (!CapturedPieces.ContainsKey(piece.pieceType))
                return;

            CapturedPieces[piece.pieceType] += piece.captured ? 1 : -1;

            // Clamp to prevent negative values
            if (CapturedPieces[piece.pieceType] < 0)
                CapturedPieces[piece.pieceType] = 0;

            PieceCaptured?.Invoke(piece);
        }

        public void Clear()
        {
            foreach (var key in CapturedPieces.Keys)
            {
                CapturedPieces[key] = 0;
            }
            CapturedPiecesCleared?.Invoke();
        }


    }
}
