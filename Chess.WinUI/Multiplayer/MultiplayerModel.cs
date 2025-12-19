using Chess.UI.Multiplayer;
using Chess.UI.Services;
using System;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Models
{
    public enum MultiplayerMode
    {
        None = 0,
        Init = 1,
        Server = 2,
        Client = 3
    }


    public class MultiplayerModel : IMultiplayerModel
    {
        public void Init()
        {
            var logicCommunication = App.Current.ChessLogicCommunication as CommunicationLayer;
            logicCommunication.ConnectionStatusEvent += HandleConnectionStatusUpdates;
            logicCommunication.PlayerChanged += HandlePlayerChanged;
            logicCommunication.MultiPlayerChosenByRemote += HandleLocalPlayerChosenByRemote;
        }


        public void StartMultiplayer()
        {
            EngineAPI.StartedMultiplayer();
        }


        public void HandlePlayerChanged(PlayerColor player)
        {
            OnPlayerChanged?.Invoke(player);
        }


        private void HandleConnectionStatusUpdates(ConnectionStatusEvent connectionStatusEvent)
        {
            ConnectionState connectionState = connectionStatusEvent.ConnectionState;

            if (connectionState == ConnectionState.Error)
            {
                OnConnectionErrorOccured?.Invoke(connectionStatusEvent.errorMessage);
                return;
            }

            OnConnectionStatusChanged?.Invoke(connectionState, connectionStatusEvent.remoteName);
        }


        public void StartGameServer()
        {
            EngineAPI.StartRemoteDiscovery(true);
        }


        public void StartGameClient()
        {
            EngineAPI.StartRemoteDiscovery(false);
        }


        public void ConnectToHost()
        {
            EngineAPI.SendConnectionRequestToHost();
        }


        public void ResetToInit()
        {
            EngineAPI.StoppedMultiplayer();
        }


        public void SetLocalPlayerColor(EngineAPI.PlayerColor color)
        {
            EngineAPI.SetLocalPlayer((int)color);
        }


        public void SetPlayerReady(bool flag)
        {
            EngineAPI.SetLocalPlayerReady(flag);
        }


        public void StartMultiplerGame()
        {
            EngineAPI.StartMultiplayerGame();
        }


        public void DisconnectMultiplayer()
        {
            EngineAPI.DisconnectMultiplayerGame();
        }


        public void AnswerConnectionInvitation(bool accepted)
        {
            EngineAPI.AnswerConnectionInvitation(accepted);
        }


        public void HandleLocalPlayerChosenByRemote(PlayerColor local)
        {
            OnMultiplayerPlayerSetFromRemote?.Invoke(local);
        }



        public event Action<string> OnConnectionErrorOccured;
        public event Action<ConnectionState, string> OnConnectionStatusChanged;
        public event Action<PlayerColor> OnPlayerChanged;
        public event Action<PlayerColor> OnMultiplayerPlayerSetFromRemote;
    }
}
