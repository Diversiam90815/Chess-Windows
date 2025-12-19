using Chess.UI.Communication_Layer.Interfaces;
using System;
using System.Runtime.InteropServices;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Services
{
    public class CommunicationLayer : ICommunicationLayer
    {
        private GCHandle _delegateHandle;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void APIDelegate(int message, nint data);


        public enum DelegateMessage
        {
            EndGameState = 1,
            PlayerScoreUpdated = 2,
            PlayerCapturedPiece = 3,
            PlayerChanged = 4,
            GameStateChanged = 5,
            MoveHistoryUpdated = 6,
            MoveExecuted = 7,
            ConnectionStateChanged = 8,
            MultiplayerPlayerChosen = 9,
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
                case DelegateMessage.PlayerScoreUpdated:
                    {
                        HandlePlayerScoreUpdate(data);
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


        private void HandlePlayerScoreUpdate(nint data)
        {
            EngineAPI.Score score = (EngineAPI.Score)Marshal.PtrToStructure(data, typeof(EngineAPI.Score));
            PlayerScoreUpdated?.Invoke(score);
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
            PlayerColor player = (PlayerColor)iPlayer;
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
            GameState state = (GameState)iState;
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
            PlayerColor player = (PlayerColor)iPlayer;
            MultiPlayerChosenByRemote?.Invoke(player);
        }


        #region ViewModel Delegates

        public event Action<PlayerColor> PlayerChanged;
        public event Action<GameState> GameStateChanged;
        public event Action<MoveHistoryEvent> MoveHistoryUpdated;
        public event Action<PossibleMoveInstance> MoveExecuted;
        public event Action<PlayerCapturedPiece> PlayerCapturedPieceEvent;
        public event Action<EngineAPI.Score> PlayerScoreUpdated;
        public event Action<EndGameStateEvent> EndGameStateEvent;
        public event Action<ConnectionStatusEvent> ConnectionStatusEvent;
        public event Action<PlayerColor> MultiPlayerChosenByRemote;

        #endregion

    }
}
