using Chess.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;

namespace Chess.UI.Score
{
    public class ScoreModel : IScoreModel
    {

        public Dictionary<PieceTypeInstance, int> WhiteCapturedPieces { get; } = new Dictionary<PieceTypeInstance, int>
    {
        { PieceTypeInstance.Pawn, 0 },
        { PieceTypeInstance.Bishop, 0 },
        { PieceTypeInstance.Knight, 0 },
        { PieceTypeInstance.Rook, 0 },
        { PieceTypeInstance.Queen, 0 }
    };

        public Dictionary<PieceTypeInstance, int> BlackCapturedPieces { get; } = new Dictionary<PieceTypeInstance, int>
    {
        { PieceTypeInstance.Pawn, 0 },
        { PieceTypeInstance.Bishop, 0 },
        { PieceTypeInstance.Knight, 0 },
        { PieceTypeInstance.Rook, 0 },
        { PieceTypeInstance.Queen, 0 }
    };


        public ScoreModel()
        {
            var logicCommunication = App.Current.ChessLogicCommunication as CommunicationLayer;
            logicCommunication.PlayerCapturedPieceEvent += OnPlayerCapturedPiece;
            logicCommunication.PlayerScoreUpdated += HandlePlayerScoreUpdated;

        }


        private void OnPlayerCapturedPiece(PlayerCapturedPiece captureEvent)
        {
            PlayerCapturedPiece?.Invoke(captureEvent);
        }


        private void HandlePlayerScoreUpdated(EngineAPI.Score score)
        {
            PlayerScoreUpdated?.Invoke(score);
        }


        public event Action<PlayerCapturedPiece> PlayerCapturedPiece;
        public event Action<EngineAPI.Score> PlayerScoreUpdated;

    }
}
