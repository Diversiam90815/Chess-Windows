using Chess.UI.Services;
using System;
using System.Collections.Generic;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Score
{
    public interface IScoreModel
    {
        // Properties
        Dictionary<PieceType, int> CapturedPieces { get; }

        // Events
        event Action<PlayerCapturedPiece> PlayerCapturedPiece;
    }


    public class ScoreModel : IScoreModel
    {

        public Dictionary<PieceType, int> CapturedPieces { get; } = new Dictionary<PieceType, int>
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


        public ScoreModel()
        {
            var logicCommunication = App.Current.ChessLogicCommunication as CommunicationLayer;
            logicCommunication.PlayerCapturedPieceEvent += OnPlayerCapturedPiece;
        }


        private void OnPlayerCapturedPiece(PlayerCapturedPiece captureEvent)
        {
            PlayerCapturedPiece?.Invoke(captureEvent);
        }


        public event Action<PlayerCapturedPiece> PlayerCapturedPiece;
    }
}
