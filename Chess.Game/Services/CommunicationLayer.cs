using Chess.UI.Models;
using Chess.UI.Settings;
using Chess.UI.Wrappers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Services
{
    public interface ICommunicationLayer
    {
        // Initialization methods
        void Init();
        void Deinit();

        // Event handlers for communication from native code
        void DelegateHandler(int message, nint data);

        // Game lifecycle requests to the engine, it answers with GameStateChanged
        void StartGame(GameConfiguration config);
        void ResetGame();

        // Multiplayer settings of the engine
        IReadOnlyList<NetworkAdapter> GetNetworkAdapters();
        int GetCurrentNetworkAdapterID();
        void ChangeNetworkAdapter(int adapterID);
        void SetLocalPlayerName(string name);

        // Events for notifying UI components, raised on the UI thread
        event Action<Side> PlayerChanged;
        event Action<GamePhase> GameStateChanged;
        event Action<PlayerCapturedPiece> PlayerCapturedPieceEvent;
        event Action<EndGameStateEvent> EndGameStateEvent;
        event Action<MultiplayerStateEvent> MultiplayerStateChanged;
        event Action<MultiplayerStateEvent> OpponentDiscoveredEvent;
        event Action<Side> MultiPlayerChosenByRemote;
        event Action<Move, string> MoveExecuted;
        event Action MoveUndone;
        event Action<IReadOnlyList<Move>> LegalMovesCalculated;
        event Action PawnPromotionRequired;
        event Action<PieceType[]> BoardStateChanged;
    }


    public class CommunicationLayer : ICommunicationLayer
    {
        private readonly ISettingsService _settingsService;
        private readonly IDispatcherQueueWrapper _dispatcherQueue;
        private GCHandle _delegateHandle;

        // The engine declares PFN_CALLBACK as __stdcall
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void APIDelegate(int message, nint data);


        public CommunicationLayer(ISettingsService settingsService, IDispatcherQueueWrapper dispatcherQueue)
        {
            _settingsService = settingsService;
            _dispatcherQueue = dispatcherQueue;
        }


        public enum DelegateMessage
        {
            EndGameState = 1,
            PlayerCapturedPiece = 2,
            PlayerChanged = 3,
            GameStateChanged = 4,
            MoveExecuted = 5,
            MoveUndone = 6,
            ConnectionStateChanged = 7,
            MultiplayerPlayerChosen = 8,
            BoardStateChanged = 9,
            PawnPromotion = 10,
            LegalMovesCalculated = 11,
            OpponentDiscovered = 12,
        }


        public void Init()
        {
            var settings = new UserSettingsInit
            {
                PlayerName = _settingsService.PlayerName ?? "",
                DiscoveryUDPPort = _settingsService.DiscoveryUDPPort,
                LogFolder = Project.AppDataDirectory
            };

            EngineAPI.Init(settings);
            SetDelegate();
        }


        public void Deinit()
        {
            EngineAPI.Deinit();
            _delegateHandle.Free();
        }


        public void StartGame(GameConfiguration config)
        {
            EngineAPI.StartGame(config);
        }


        public void ResetGame()
        {
            EngineAPI.ResetGame();
        }


        public IReadOnlyList<NetworkAdapter> GetNetworkAdapters()
        {
            var adapters = new List<NetworkAdapter>();
            int adapterCount = EngineAPI.GetNetworkAdapterCount();

            for (uint i = 0; i < adapterCount; ++i)
            {
                if (!EngineAPI.GetNetworkAdapterAtIndex(i, out NetworkAdapterInstance adapter))
                    continue;

                adapters.Add(new NetworkAdapter
                {
                    AdapterName = adapter.AdapterName,
                    NetworkName = adapter.NetworkName,
                    ID = adapter.ID,
                    Visibility = (AdapterVisibility)adapter.Priority,
                });
            }

            return adapters;
        }


        public int GetCurrentNetworkAdapterID()
        {
            return EngineAPI.GetCurrentNetworkAdapterID();
        }


        public void ChangeNetworkAdapter(int adapterID)
        {
            EngineAPI.ChangeCurrentAdapter(adapterID);
        }


        public void SetLocalPlayerName(string name)
        {
            EngineAPI.SetLocalPlayerName(name ?? "");
        }


        private void SetDelegate()
        {
            APIDelegate mDelegate = new(DelegateHandler);
            _delegateHandle = GCHandle.Alloc(mDelegate);
            EngineAPI.SetDelegate(mDelegate);
        }


        /// <summary>
        /// Called by the engine on its own threads. The payload behind 'data' is only valid during the call and
        /// the engine moves on right after it, so everything is read here; the events are raised on the UI thread.
        /// </summary>
        public void DelegateHandler(int message, nint data)
        {
            DelegateMessage delegateMessage = (DelegateMessage)message;

            try
            {
                switch (delegateMessage)
                {
                    case DelegateMessage.EndGameState:
                        HandleEndGameState(data);
                        break;

                    case DelegateMessage.PlayerChanged:
                        HandlePlayerChanged(data);
                        break;

                    case DelegateMessage.GameStateChanged:
                        HandleGameStateChanges(data);
                        break;

                    case DelegateMessage.MoveExecuted:
                        HandleMoveExecuted(data);
                        break;

                    case DelegateMessage.MoveUndone:
                        HandleMoveUndone(data);
                        break;

                    case DelegateMessage.PlayerCapturedPiece:
                        HandlePlayerCapturedPiece(data);
                        break;

                    case DelegateMessage.ConnectionStateChanged:
                        HandleConnectionStatusChanged(data);
                        break;

                    case DelegateMessage.MultiplayerPlayerChosen:
                        HandlePlayerChosenForMultiplayerByRemote(data);
                        break;

                    case DelegateMessage.OpponentDiscovered:
                        HandleOpponentDiscovered(data);
                        break;

                    case DelegateMessage.BoardStateChanged:
                        HandleBoardStateChanged();
                        break;

                    case DelegateMessage.PawnPromotion:
                        HandlePawnPromotionRequired();
                        break;

                    case DelegateMessage.LegalMovesCalculated:
                        HandleLegalMovesCalculated();
                        break;

                    default:
                        Logger.LogWarning($"Unhandled delegate message: {delegateMessage}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error handling delegate message {delegateMessage}: {ex.Message}");
            }
        }


        private void HandleMoveUndone(nint data)
        {
            Logger.LogDebug("Move undone");
            RaiseOnUIThread(() => MoveUndone?.Invoke());
        }


        private void HandleMoveExecuted(nint data)
        {
            if (data == nint.Zero)
            {
                Logger.LogError("HandleMoveExecuted received null data pointer");
                return;
            }

            try
            {
                MoveEvent moveEvent = (MoveEvent)Marshal.PtrToStructure(data, typeof(MoveEvent));

                Move move = new Move(moveEvent.data);
                string notation = moveEvent.moveNotation ?? string.Empty;

                Logger.LogDebug($"Move executed: {move} ({notation})");

                RaiseOnUIThread(() => MoveExecuted?.Invoke(move, notation));

            }
            catch (Exception ex)
            {
                Logger.LogError($"Error occured handling move executed: {ex.Message}");
            }
        }


        private void HandleLegalMovesCalculated()
        {
            List<Move> legalMoves = ReadLegalMoves();

            Logger.LogInfo($"{legalMoves.Count} legal moves calculated");
            RaiseOnUIThread(() => LegalMovesCalculated?.Invoke(legalMoves));
        }


        private void HandlePawnPromotionRequired()
        {
            Logger.LogInfo("Pawn promotion required");
            RaiseOnUIThread(() => PawnPromotionRequired?.Invoke());
        }


        private void HandleBoardStateChanged()
        {
            PieceType[] board = ReadBoardState();

            Logger.LogInfo("Board state changed");
            RaiseOnUIThread(() => BoardStateChanged?.Invoke(board));
        }


        private void HandlePlayerChanged(nint data)
        {
            if (data == nint.Zero)
            {
                Logger.LogError("HandlePlayerChanged received null data pointer");
                return;
            }

            int iPlayer = Marshal.ReadInt32(data);
            Side player = (Side)iPlayer;

            Logger.LogInfo($"Player changed to: {player}");
            RaiseOnUIThread(() => PlayerChanged?.Invoke(player));
        }


        private void HandleEndGameState(nint data)
        {
            if (data == nint.Zero)
            {
                Logger.LogError("HandleEndGameState received null data pointer");
                return;
            }

            EndGameStateEvent endgameEvent = Marshal.PtrToStructure<EndGameStateEvent>(data);

            Logger.LogInfo($"Game ended: State={endgameEvent.State}, Winner={endgameEvent.winner}");
            RaiseOnUIThread(() => EndGameStateEvent?.Invoke(endgameEvent));
        }


        private void HandlePlayerCapturedPiece(nint data)
        {
            if (data == nint.Zero)
            {
                Logger.LogError("HandlePlayerCapturedPiece received null data pointer");
                return;
            }

            PlayerCapturedPiece capturedEvent = Marshal.PtrToStructure<PlayerCapturedPiece>(data);

            Logger.LogInfo($"Piece captured: Player={capturedEvent.playerColor}, Piece={capturedEvent.pieceType}, Captured={capturedEvent.captured}");
            RaiseOnUIThread(() => PlayerCapturedPieceEvent?.Invoke(capturedEvent));
        }


        private void HandleGameStateChanges(nint data)
        {
            if (data == nint.Zero)
            {
                Logger.LogError("HandleGameStateChanges received null data pointer");
                return;
            }

            int iState = Marshal.ReadInt32(data);
            GamePhase state = (GamePhase)iState;

            Logger.LogInfo($"Game state changed to: {state}");
            RaiseOnUIThread(() => GameStateChanged?.Invoke(state));
        }


        private void HandleConnectionStatusChanged(nint data)
        {
            if (data == nint.Zero)
            {
                Logger.LogError("HandleConnectionStatusChanged received null data pointer");
                return;
            }

            MultiplayerStateEvent connectionEvent = Marshal.PtrToStructure<MultiplayerStateEvent>(data);

            Logger.LogInfo($"Multiplayer state changed: {connectionEvent.State}");
            RaiseOnUIThread(() => MultiplayerStateChanged?.Invoke(connectionEvent));
        }


        private void HandleOpponentDiscovered(nint data)
        {
            if (data == nint.Zero)
            {
                Logger.LogError("HandleOpponentDiscovered received null data pointer");
                return;
            }

            MultiplayerStateEvent opponentEvent = Marshal.PtrToStructure<MultiplayerStateEvent>(data);

            Logger.LogInfo($"Opponent discovered: {opponentEvent.remoteName}");
            RaiseOnUIThread(() => OpponentDiscoveredEvent?.Invoke(opponentEvent));
        }


        private void HandlePlayerChosenForMultiplayerByRemote(nint data)
        {
            if (data == nint.Zero)
            {
                Logger.LogError("HandlePlayerChosenForMultiplayerByRemote received null data pointer");
                return;
            }

            // The engine passes its Side enum here, which is a single signed byte
            Side player = (Side)(sbyte)Marshal.ReadByte(data);

            Logger.LogInfo($"Multiplayer player chosen by remote: {player}");
            RaiseOnUIThread(() => MultiPlayerChosenByRemote?.Invoke(player));
        }


        private void RaiseOnUIThread(Action raiseEvent)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    raiseEvent();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error in a handler of an engine event: {ex.Message}");
                }
            });
        }


        // Legal moves of the piece that is currently selected in the engine
        private static List<Move> ReadLegalMoves()
        {
            var legalMoves = new List<Move>();
            int numMoves = EngineAPI.GetNumLegalMoves();

            for (int i = 0; i < numMoves; ++i)
            {
                if (EngineAPI.GetLegalMoveAtIndex(i, out ushort moveData))
                {
                    legalMoves.Add(new Move(moveData));
                }
            }

            return legalMoves;
        }


        public static PieceType[] ReadBoardState()
        {
            int[] rawBoard = new int[64]; // pre-allocated array
            EngineAPI.GetBoardState(rawBoard);

            return Array.ConvertAll(rawBoard, piece => (PieceType)piece);
        }


        #region Events

        public event Action<Side> PlayerChanged;
        public event Action<GamePhase> GameStateChanged;
        public event Action<Move, string> MoveExecuted;
        public event Action MoveUndone;
        public event Action<PlayerCapturedPiece> PlayerCapturedPieceEvent;
        public event Action<EndGameStateEvent> EndGameStateEvent;
        public event Action<MultiplayerStateEvent> MultiplayerStateChanged;
        public event Action<MultiplayerStateEvent> OpponentDiscoveredEvent;
        public event Action<Side> MultiPlayerChosenByRemote;
        public event Action<IReadOnlyList<Move>> LegalMovesCalculated;
        public event Action PawnPromotionRequired;
        public event Action<PieceType[]> BoardStateChanged;

        #endregion

    }
}
