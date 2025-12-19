using Chess.UI.Services;
using System;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Multiplayer
{
    public interface IMultiplayerModel
    {
        void Init();

        void ResetToInit();

        void StartGameServer();

        void StartGameClient();

        void StartMultiplerGame();

        void StartMultiplayer();

        void DisconnectMultiplayer();

        void ConnectToHost();

        void AnswerConnectionInvitation(bool accept);

        void SetLocalPlayerColor(EngineAPI.PlayerColor color);
        
        void SetPlayerReady(bool ready);

        void HandleLocalPlayerChosenByRemote(PlayerColor local);


        event Action<string> OnConnectionErrorOccured;

        event Action<ConnectionState, string> OnConnectionStatusChanged;

        event Action<PlayerColor> OnPlayerChanged;

        event Action<PlayerColor> OnMultiplayerPlayerSetFromRemote;
    }
}
