using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;

namespace Chess.UI.Score
{
    public interface IScoreModel
    {
        // Properties
        Dictionary<PieceTypeInstance, int> WhiteCapturedPieces { get; }
        Dictionary<PieceTypeInstance, int> BlackCapturedPieces { get; }

        // Events
        event Action<PlayerCapturedPiece> PlayerCapturedPiece;
        event Action<Services.EngineAPI.Score> PlayerScoreUpdated;
    }
}
