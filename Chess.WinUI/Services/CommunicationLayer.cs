using Chess.UI.Models;
using System;
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

        // Events for notifying UI components
        event Action<Side> PlayerChanged;
        event Action<GamePhase> GameStateChanged;
        event Action<PlayerCapturedPiece> PlayerCapturedPieceEvent;
        event Action<EndGameStateEvent> EndGameStateEvent;
        event Action<ConnectionStatusEvent> ConnectionStatusEvent;
        event Action<Side> MultiPlayerChosenByRemote;
        event Action<Move, string> MoveExecuted;
        event Action MoveUndone;
        event Action LegalMovesCalculated;
        event Action PawnPromotionRequired;
        event Action BoardStateChanged;
    }


    public class CommunicationLayer : ICommunicationLayer
    {
        private GCHandle _delegateHandle;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void APIDelegate(int message, nint data);


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
        }


        public void Init()
        {
            EngineAPI.SetUnvirtualizedAppDataPath(Project.AppDataDirectory);
            EngineAPI.Init();
            SetDelegate();
        }


        public void Deinit()
        {
            EngineAPI.Deinit();
            _delegateHandle.Free();
        }


        private void SetDelegate()
        {
            APIDelegate mDelegate = new(DelegateHandler);
            _delegateHandle = GCHandle.Alloc(mDelegate);
            EngineAPI.SetDelegate(mDelegate);
        }


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
            MoveUndone?.Invoke();
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

                MoveExecuted?.Invoke(move, notation);

            }
            catch (Exception ex)
            {
                Logger.LogError($"Error occured handling move executed: {ex.Message}");
            }
        }


        private void HandleLegalMovesCalculated()
        {
            Logger.LogInfo("Legal moves calculated");
            LegalMovesCalculated?.Invoke();
        }


        private void HandlePawnPromotionRequired()
        {
            Logger.LogInfo("Pawn promotion required");
            PawnPromotionRequired?.Invoke();
        }


        private void HandleBoardStateChanged()
        {
            Logger.LogInfo("Board state changed");
            BoardStateChanged?.Invoke();
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
            PlayerChanged?.Invoke(player);
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
            EndGameStateEvent?.Invoke(endgameEvent);
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
            PlayerCapturedPieceEvent?.Invoke(capturedEvent);
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
            GameStateChanged?.Invoke(state);
        }


        private void HandleConnectionStatusChanged(nint data)
        {
            if (data == nint.Zero)
            {
                Logger.LogError("HandleConnectionStatusChanged received null data pointer");
                return;
            }

            ConnectionStatusEvent connectionEvent = Marshal.PtrToStructure<ConnectionStatusEvent>(data);

            Logger.LogInfo($"Connection status changed: {connectionEvent.ConnectionState}");
            ConnectionStatusEvent?.Invoke(connectionEvent);
        }


        private void HandlePlayerChosenForMultiplayerByRemote(nint data)
        {
            if (data == nint.Zero)
            {
                Logger.LogError("HandlePlayerChosenForMultiplayerByRemote received null data pointer");
                return;
            }

            int iPlayer = Marshal.ReadInt32(data);
            Side player = (Side)iPlayer;

            Logger.LogInfo($"Multiplayer player chosen by remote: {player}");
            MultiPlayerChosenByRemote?.Invoke(player);
        }


        #region Events

        public event Action<Side> PlayerChanged;
        public event Action<GamePhase> GameStateChanged;
        public event Action<Move, string> MoveExecuted;
        public event Action MoveUndone;
        public event Action<PlayerCapturedPiece> PlayerCapturedPieceEvent;
        public event Action<EndGameStateEvent> EndGameStateEvent;
        public event Action<ConnectionStatusEvent> ConnectionStatusEvent;
        public event Action<Side> MultiPlayerChosenByRemote;
        public event Action LegalMovesCalculated;
        public event Action PawnPromotionRequired;
        public event Action BoardStateChanged;

        #endregion

    }
}
