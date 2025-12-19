using Chess.UI.Services;
using System;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Communication_Layer.Interfaces
{
    public interface ICommunicationLayer
    {
        // Initialization methods
        void Init();
        void Deinit();

        // Event handlers for communication from native code
        void DelegateHandler(int message, nint data);

        // Events for notifying UI components
        event Action<PlayerColor> PlayerChanged;
        event Action<GameState> GameStateChanged;
        event Action<MoveHistoryEvent> MoveHistoryUpdated;
        event Action<PlayerCapturedPiece> PlayerCapturedPieceEvent;
        event Action<EngineAPI.Score> PlayerScoreUpdated;
        event Action<EndGameStateEvent> EndGameStateEvent;
        event Action<ConnectionStatusEvent> ConnectionStatusEvent;
        event Action<PlayerColor> MultiPlayerChosenByRemote;
        event Action<PossibleMoveInstance> MoveExecuted;
    }
}
