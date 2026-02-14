using Chess.UI.Services;
using System;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Models
{
    public interface IMultiplayerModel
    {
        void ResetToInit();

        void StartGameServer();

        void StartGameClient();

        void StartMultiplerGame();

        void StartMultiplayer();

        void DisconnectMultiplayer();

        void ConnectToHost();

        void AnswerConnectionInvitation(bool accept);

        void SetLocalPlayerColor(EngineAPI.Side color);

        void SetPlayerReady(bool ready);

        void HandleLocalPlayerChosenByRemote(Side local);


        event Action<string> OnConnectionErrorOccured;

        event Action<ConnectionState, string> OnConnectionStatusChanged;

        event Action<Side> OnPlayerChanged;

        event Action<Side> OnMultiplayerPlayerSetFromRemote;
    }


    public enum MultiplayerMode
    {
        None = 0,
        Init = 1,
        Server = 2,
        Client = 3
    }


    public class MultiplayerModel : IMultiplayerModel
    {
        private readonly ICommunicationLayer _backendCommunication;


        public MultiplayerModel(ICommunicationLayer backendCommunication)
        {
            _backendCommunication = backendCommunication;
            _backendCommunication.ConnectionStatusEvent += HandleConnectionStatusUpdates;
            _backendCommunication.PlayerChanged += HandlePlayerChanged;
            _backendCommunication.MultiPlayerChosenByRemote += HandleLocalPlayerChosenByRemote;
        }


        public void StartMultiplayer()
        {
            EngineAPI.StartedMultiplayer();
        }


        public void HandlePlayerChanged(Side player)
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


        public void SetLocalPlayerColor(EngineAPI.Side color)
        {
            EngineAPI.SetLocalPlayer((int)color);
        }


        public void SetPlayerReady(bool flag)
        {
            EngineAPI.SetLocalPlayerReady(flag);
        }


        public void StartMultiplerGame()
        {
            // TODO

            //EngineAPI.StartMultiplayerGame();
        }


        public void DisconnectMultiplayer()
        {
            // TODO

            //EngineAPI.DisconnectMultiplayerGame();
        }


        public void AnswerConnectionInvitation(bool accepted)
        {
            EngineAPI.AnswerConnectionInvitation(accepted);
        }


        public void HandleLocalPlayerChosenByRemote(Side local)
        {
            OnMultiplayerPlayerSetFromRemote?.Invoke(local);
        }



        public event Action<string> OnConnectionErrorOccured;
        public event Action<ConnectionState, string> OnConnectionStatusChanged;
        public event Action<Side> OnPlayerChanged;
        public event Action<Side> OnMultiplayerPlayerSetFromRemote;
    }
}
