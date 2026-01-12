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
        event Action<MoveHistoryEvent> MoveHistoryUpdated;
        event Action<PlayerCapturedPiece> PlayerCapturedPieceEvent;
        event Action<EndGameStateEvent> EndGameStateEvent;
        event Action<ConnectionStatusEvent> ConnectionStatusEvent;
        event Action<Side> MultiPlayerChosenByRemote;
        event Action<PossibleMoveInstance> MoveExecuted;
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
            MoveHistoryUpdated = 5,
            MoveExecuted = 6,
            ConnectionStateChanged = 7,
            MultiplayerPlayerChosen = 8,
            BoardStateChanged = 9,
            PawnPromotion = 10,
            PossibleMovesCalculated = 11,
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
            switch (delegateMessage)
            {
                case DelegateMessage.EndGameState:
                    {
                        HandleEndGameState(data);
                        break;
                    }
                case DelegateMessage.PlayerChanged:
                    {
                        HandlePlayerChanged(data);
                        break;
                    }
                case DelegateMessage.GameStateChanged:
                    {
                        HandleGameStateChanges(data);
                        break;
                    }
                case DelegateMessage.MoveHistoryUpdated:
                    {
                        HandleMoveHistoryUpdated(data);
                        break;
                    }
                case DelegateMessage.MoveExecuted:
                    {
                        HandleMoveExecuted(data);
                        break;
                    }
                case DelegateMessage.PlayerCapturedPiece:
                    {
                        HandlePlayerCapturedPiece(data);
                        break;
                    }
                case DelegateMessage.ConnectionStateChanged:
                    {
                        HandleConnectionStatusChanged(data);
                        break;
                    }
                case DelegateMessage.MultiplayerPlayerChosen:
                    {
                        HandlePlayerChosenForMultiplayerByRemote(data);
                        break;
                    }

                default: break;
            }
        }


        private void HandleMoveHistoryUpdated(nint data)
        {
            MoveHistoryEvent moveHistoryEvent = (MoveHistoryEvent)Marshal.PtrToStructure(data, typeof(MoveHistoryEvent));
            MoveHistoryUpdated?.Invoke(moveHistoryEvent);
        }


        private void HandleMoveExecuted(nint data)
        {
            PossibleMoveInstance moveInstance = (PossibleMoveInstance)Marshal.PtrToStructure(data, typeof(PossibleMoveInstance));
            MoveExecuted?.Invoke(moveInstance);
        }


        private void HandlePlayerChanged(nint data)
        {
            int iPlayer = Marshal.ReadInt32(data);
            Side player = (Side)iPlayer;
            PlayerChanged?.Invoke(player);
        }


        private void HandleEndGameState(nint data)
        {
            EndGameStateEvent endgameEvent = (EndGameStateEvent)Marshal.PtrToStructure(data, typeof(EndGameStateEvent));
            EndGameStateEvent?.Invoke(endgameEvent);
        }


        private void HandlePlayerCapturedPiece(nint data)
        {
            PlayerCapturedPiece capturedEvent = (PlayerCapturedPiece)Marshal.PtrToStructure(data, typeof(PlayerCapturedPiece));
            PlayerCapturedPieceEvent?.Invoke(capturedEvent);
        }


        private void HandleGameStateChanges(nint data)
        {
            int iState = Marshal.ReadInt32(data);
            GamePhase state = (GamePhase)iState;
            GameStateChanged?.Invoke(state);
        }


        private void HandleConnectionStatusChanged(nint data)
        {
            ConnectionStatusEvent connectionEvent = (ConnectionStatusEvent)Marshal.PtrToStructure(data, typeof(ConnectionStatusEvent));
            ConnectionStatusEvent?.Invoke(connectionEvent);
        }


        private void HandlePlayerChosenForMultiplayerByRemote(nint data)
        {
            int iPlayer = Marshal.ReadInt32(data);
            Side player = (Side)iPlayer;
            MultiPlayerChosenByRemote?.Invoke(player);
        }


        #region ViewModel Delegates

        public event Action<Side> PlayerChanged;
        public event Action<GamePhase> GameStateChanged;
        public event Action<MoveHistoryEvent> MoveHistoryUpdated;
        public event Action<PossibleMoveInstance> MoveExecuted;
        public event Action<PlayerCapturedPiece> PlayerCapturedPieceEvent;
        public event Action<EndGameStateEvent> EndGameStateEvent;
        public event Action<ConnectionStatusEvent> ConnectionStatusEvent;
        public event Action<Side> MultiPlayerChosenByRemote;

        #endregion

    }
}
