using Chess.UI.Board;
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

        // A complete move, wherever it comes from. The engine plays it only if it is legal.
        void SubmitMove(Square from, Square to, PieceType promotion);

        // Events for notifying UI components, raised on the UI thread
        event Action<Side> PlayerChanged;
        event Action<GamePhase> GameStateChanged;
        event Action<PlayerCapturedPiece> PlayerCapturedPieceEvent;
        event Action<EndGameStateEvent> EndGameStateEvent;
        event Action<Move, string> MoveExecuted;
        event Action MoveUndone;
        event Action<IReadOnlyList<Move>> LegalMovesCalculated;
        event Action PawnPromotionRequired;
        event Action<PieceType[]> BoardStateChanged;
    }


    public class CommunicationLayer : ICommunicationLayer
    {
        private readonly IDispatcherQueueWrapper _dispatcherQueue;
        private GCHandle _delegateHandle;

        // The engine calls it from its threads until it is removed: it must stay alive that long
        private readonly EngineLogDelegate _logDelegate;

        // The engine declares PFN_CALLBACK as __stdcall
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void APIDelegate(int message, nint data);


        public CommunicationLayer(IDispatcherQueueWrapper dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;
            _logDelegate = HandleEngineLog;
        }


        public enum DelegateMessage
        {
            EndGameState = 1,
            PlayerCapturedPiece = 2,
            PlayerChanged = 3,
            GameStateChanged = 4,
            MoveExecuted = 5,
            MoveUndone = 6,
            BoardStateChanged = 7,
            PawnPromotion = 8,
            LegalMovesCalculated = 9,
        }


        public void Init()
        {
            // Before Init, so the lines the engine writes while it starts are in the app's log too
            EngineAPI.SetLogLevel((int)Logger.Level);
            EngineAPI.SetLogCallback(_logDelegate, 0);
            Logger.LevelChanged += SetEngineLogLevel;

            EngineAPI.Init();
            SetDelegate();
        }


        public void Deinit()
        {
            EngineAPI.Deinit();
            _delegateHandle.Free();

            // Returns once the engine is not in the callback anymore
            Logger.LevelChanged -= SetEngineLogLevel;
            EngineAPI.SetLogCallback(null, 0);
        }


        private static void SetEngineLogLevel(LogLevel level)
        {
            EngineAPI.SetLogLevel((int)level);
        }


        /// <summary>
        /// A log line of the engine, on the engine thread that wrote it. Never throws back into the engine.
        /// </summary>
        private static void HandleEngineLog(int level, nint file, int line, nint function, nint message, nint context)
        {
            try
            {
                string component = System.IO.Path.GetFileNameWithoutExtension(Marshal.PtrToStringUTF8(file) ?? string.Empty);
                Logger.Write((LogLevel)level, component, Marshal.PtrToStringUTF8(function), Marshal.PtrToStringUTF8(message));
            }
            catch
            {
                // A log line is not worth crashing the engine thread for
            }
        }


        public void StartGame(GameConfiguration config)
        {
            // Where the moves come from does not matter to the engine: a game against a player on the network is
            // a game of two players to it, the opponent's moves are submitted like any other
            if (config.Mode == GameModeSelection.MultiPlayer)
                config = GameConfiguration.CreateLocalCoop();

            EngineAPI.StartGame(config);
        }


        public void SubmitMove(Square from, Square to, PieceType promotion)
        {
            EngineAPI.SubmitMove((int)from, (int)to, (int)promotion);
        }


        public void ResetGame()
        {
            EngineAPI.ResetGame();
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
        public event Action<IReadOnlyList<Move>> LegalMovesCalculated;
        public event Action PawnPromotionRequired;
        public event Action<PieceType[]> BoardStateChanged;

        #endregion

    }
}
