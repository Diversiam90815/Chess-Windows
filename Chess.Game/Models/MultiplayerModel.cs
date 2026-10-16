using Chess.UI.Board;
using Chess.UI.Services;
using Chess.UI.Settings;
using System;
using System.Collections.Generic;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Models
{
    public enum MultiplayerState
    {
        None = 0,
        Searching = 1,
        OpponentFound = 2,
        ConnectionRequested = 3,
        Connected = 4,
        PlayerSetup = 5,
        ReadyToStart = 6,
        InGame = 7,
        Disconnected = 8,
        Error = 9
    }


    /// <summary>
    /// What the two players send each other. The values are the protocol: never renumber them.
    /// </summary>
    public enum ChessMessageType : uint
    {
        Move = 1,           // from square, to square, promotion (0-3 = knight, bishop, rook, queen; 0xFF = none)
        PlayerChosen = 2,   // the side the sender plays
        PlayerReady = 3,    // 1 = ready
    }


    public interface IMultiplayerModel
    {
        void StartMultiplayer();

        void StopMultiplayer();

        void FindOpponent();

        void ConnectToOpponent(int index);

        void RespondToConnectionRequest(bool accept);

        void CancelSearch();

        void SetLocalPlayerColor(EngineAPI.Side color);

        void SetPlayerReady(bool ready);


        event Action<string> OnConnectionErrorOccured;

        event Action<MultiplayerState, string> OnMultiplayerStateChanged;

        event Action<string> OnOpponentDiscovered;

        event Action<Side> OnPlayerChanged;

        event Action<Side> OnMultiplayerPlayerSetFromRemote;
    }


    /// <summary>
    /// A game against a player on the local network. Finds and connects to the other player through the network
    /// service and passes the moves both ways: the engine gets the opponent's moves the same way as any other move.
    /// </summary>
    public class MultiplayerModel : IMultiplayerModel
    {
        private const byte NoPromotion = 0xFF;

        private readonly ICommunicationLayer _backendCommunication;
        private readonly INetworkService _network;
        private readonly ISettingsService _settings;
        private readonly IChessGameService _gameService;

        private readonly List<NetworkPeer> _discovered = [];
        private NetworkPeer _requestingPeer;
        private NetworkPeer _opponent;

        private bool _localReady;
        private bool _remoteReady;

        // The opponent's move the engine was given last: the move it reports back must not be sent again
        private (Square From, Square To)? _moveFromOpponent;


        public MultiplayerModel(ICommunicationLayer backendCommunication, INetworkService network, ISettingsService settings, IChessGameService gameService)
        {
            _backendCommunication = backendCommunication;
            _network = network;
            _settings = settings;
            _gameService = gameService;

            _backendCommunication.PlayerChanged += HandlePlayerChanged;
            _backendCommunication.MoveExecuted += HandleMoveExecuted;

            _network.PeerDiscovered += HandlePeerDiscovered;
            _network.PeerLost += HandlePeerLost;
            _network.ConnectionRequested += HandleConnectionRequested;
            _network.Connected += HandleConnected;
            _network.Disconnected += HandleDisconnected;
            _network.MessageReceived += HandleMessageReceived;
        }


        #region Lobby

        public void StartMultiplayer()
        {
            if (!_network.Start(_settings.PlayerName))
                OnConnectionErrorOccured?.Invoke("Networking could not be started");
        }


        public void StopMultiplayer()
        {
            if (_opponent != null)
                _network.Disconnect(_opponent.Id);

            _network.Stop();
            ResetSession();
        }


        public void FindOpponent()
        {
            if (!_network.IsRunning)
                StartMultiplayer();

            _network.StartDiscovery();
            ChangeState(MultiplayerState.Searching);

            foreach (var peer in _discovered)
                OnOpponentDiscovered?.Invoke(peer.Name);
        }


        public void ConnectToOpponent(int index)
        {
            if (index < 0 || index >= _discovered.Count)
                return;

            NetworkPeer peer = _discovered[index];

            if (!_network.Connect(peer.Id))
                OnConnectionErrorOccured?.Invoke($"Could not connect to {peer.Name}");
        }


        public void RespondToConnectionRequest(bool accept)
        {
            if (_requestingPeer == null)
                return;

            if (accept)
                _network.Accept(_requestingPeer.Id);
            else
                _network.Decline(_requestingPeer.Id);

            _requestingPeer = null;
        }


        public void CancelSearch()
        {
            _network.StopDiscovery();
            _discovered.Clear();
            ChangeState(MultiplayerState.None);
        }


        public void SetLocalPlayerColor(Side color)
        {
            SendToOpponent(ChessMessageType.PlayerChosen, [(byte)(sbyte)color]);
        }


        public void SetPlayerReady(bool ready)
        {
            _localReady = ready;
            SendToOpponent(ChessMessageType.PlayerReady, [ready ? (byte)1 : (byte)0]);
            StartGameIfBothReady();
        }

        #endregion


        #region Network Events

        private void HandlePeerDiscovered(NetworkPeer peer)
        {
            if (_discovered.Exists(known => known.Id == peer.Id))
                return;

            _discovered.Add(peer);
            OnOpponentDiscovered?.Invoke(peer.Name);
        }


        private void HandlePeerLost(ulong peer)
        {
            _discovered.RemoveAll(known => known.Id == peer);
        }


        private void HandleConnectionRequested(NetworkPeer peer)
        {
            _requestingPeer = peer;
            ChangeState(MultiplayerState.ConnectionRequested, peer.Name);
        }


        private void HandleConnected(NetworkPeer peer)
        {
            Logger.LogInfo($"Connected to {peer.Name}");

            _opponent = peer;
            _localReady = false;
            _remoteReady = false;

            _network.StopDiscovery();

            ChangeState(MultiplayerState.Connected, peer.Name);
            ChangeState(MultiplayerState.PlayerSetup, peer.Name);
        }


        private void HandleDisconnected(ulong peer, NetLinkAPI.DisconnectReason reason)
        {
            Logger.LogInfo($"Session with peer {peer} ended: {reason}");

            if (_opponent == null || _opponent.Id != peer)
            {
                // A request or connect() that did not lead to a session
                if (reason == NetLinkAPI.DisconnectReason.Declined || reason == NetLinkAPI.DisconnectReason.Incompatible)
                    OnConnectionErrorOccured?.Invoke($"Connection {reason.ToString().ToLowerInvariant()}");

                return;
            }

            string name = _opponent.Name;
            ResetSession();
            ChangeState(MultiplayerState.Disconnected, name);
        }


        private void HandleMessageReceived(ulong from, uint type, byte[] data)
        {
            if (_opponent == null || _opponent.Id != from)
                return;

            switch ((ChessMessageType)type)
            {
                case ChessMessageType.Move:
                    HandleOpponentMove(data);
                    break;

                case ChessMessageType.PlayerChosen:
                    if (data.Length < 1)
                        return;

                    // The opponent plays this side, so the local player gets the other one
                    Side remote = (Side)(sbyte)data[0];
                    OnMultiplayerPlayerSetFromRemote?.Invoke(remote == Side.White ? Side.Black : Side.White);
                    break;

                case ChessMessageType.PlayerReady:
                    _remoteReady = data.Length > 0 && data[0] != 0;
                    StartGameIfBothReady();
                    break;

                default:
                    Logger.LogWarning($"Unknown message {type} from the opponent");
                    break;
            }
        }

        #endregion


        #region Moves

        private void HandleOpponentMove(byte[] data)
        {
            if (data.Length < 3 || !_gameService.IsMultiplayerGame)
                return;

            var from = (Square)data[0];
            var to = (Square)data[1];

            if ((int)from >= 64 || (int)to >= 64)
                return;

            // The engine only needs to know which piece; it checks the move like any other
            PieceType promotion = data[2] switch
            {
                0 => PieceType.WKnight,
                1 => PieceType.WBishop,
                2 => PieceType.WRook,
                3 => PieceType.WQueen,
                _ => PieceType.None,
            };

            Logger.LogInfo($"Opponent moved {from} -> {to}");

            _moveFromOpponent = (from, to);
            _backendCommunication.SubmitMove(from, to, promotion);
        }


        private void HandleMoveExecuted(Move move, string notation)
        {
            if (!_gameService.IsMultiplayerGame || _opponent == null)
                return;

            if (_moveFromOpponent == (move.From, move.To))
            {
                _moveFromOpponent = null;
                return;
            }

            byte promotion = move.IsPromotion ? (byte)move.PromotionPieceOffset : NoPromotion;
            SendToOpponent(ChessMessageType.Move, [(byte)move.From, (byte)move.To, promotion]);
        }

        #endregion


        private void StartGameIfBothReady()
        {
            if (_localReady && _remoteReady && _opponent != null)
                ChangeState(MultiplayerState.InGame, _opponent.Name);
        }


        private void SendToOpponent(ChessMessageType type, byte[] data)
        {
            if (_opponent == null)
                return;

            if (!_network.Send(_opponent.Id, (uint)type, data))
                OnConnectionErrorOccured?.Invoke($"Could not send {type} to {_opponent.Name}");
        }


        private void ResetSession()
        {
            _opponent = null;
            _requestingPeer = null;
            _localReady = false;
            _remoteReady = false;
            _moveFromOpponent = null;
            _discovered.Clear();
        }


        private void ChangeState(MultiplayerState state, string remoteName = "")
        {
            OnMultiplayerStateChanged?.Invoke(state, remoteName);
        }


        public void HandlePlayerChanged(Side player)
        {
            OnPlayerChanged?.Invoke(player);
        }


        public event Action<string> OnConnectionErrorOccured;
        public event Action<MultiplayerState, string> OnMultiplayerStateChanged;
        public event Action<string> OnOpponentDiscovered;
        public event Action<Side> OnPlayerChanged;
        public event Action<Side> OnMultiplayerPlayerSetFromRemote;
    }
}
