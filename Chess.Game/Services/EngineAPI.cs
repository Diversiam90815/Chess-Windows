using System;
using System.Runtime.InteropServices;
using System.Text;
using static Chess.UI.Services.CommunicationLayer;


namespace Chess.UI.Services
{
    public class EngineAPI
    {
        private const string LOGIC_API_PATH = @"Chess.Engine.API.dll";
        
        
        #region DLL Defines


        #region Core Lifecycle

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Init();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Deinit();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetDelegate(APIDelegate pDelegate);

        #endregion // Core Lifecycle


        #region Game Management

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartGame([In, MarshalAs(UnmanagedType.Struct)] GameConfiguration config);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ResetGame();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void UndoMove();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetBoardState([Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 64)] int[] boardState);

        #endregion // Game Management


        #region Moves & Input Generation

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void OnSquareSelected(int square);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void OnPawnPromotionChosen(int pieceType);

        // A complete move, wherever it comes from (a click, the network). The engine plays it only if it is legal.
        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SubmitMove(int from, int to, int promotion);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumLegalMoves();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetLegalMoveAtIndex(int index, out ushort move);

        #endregion // Moves & Input Generation


        #region Logging

        // The engine writes no log file: its lines come to the app. Works before Init; null stops them.
        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLogCallback(EngineLogDelegate callback, nint context);

        // Lines below this level are dropped by the engine (values of LogLevel)
        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLogLevel(int level);

        // The engine declares PFN_LOG_CALLBACK as __stdcall. The texts are UTF-8 and only valid during the call.
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void EngineLogDelegate(int level, nint file, int line, nint function, nint message, nint context);

        #endregion


        #endregion // DLL Defines


        #region Structures & Enums

        public enum PieceType
        {
            None = -1,
            WKing = 0,
            WQueen = 1,
            WPawn = 2,
            WKnight = 3,
            WBishop = 4,
            WRook = 5,
            BKing = 6,
            BQueen = 7,
            BPawn = 8,
            BKnight = 9,
            BBishop = 10,
            BRook = 11
        }

        public enum Side
        {
            None = -1,
            White = 0,
            Black = 1,
            Both = 2
        }

        public enum CPUDifficulty
        {
            None = 0,
            Easy = 1,
            Medium = 2,
            Hard = 3,
        }

        public enum GamePhase
        {
            Initializing = 0,
            PlayerTurn = 1,
            OpponentTurn = 2,
            PromotionDialog = 3,
            GameEnded = 4,
        }

        public enum EndGameState
        {
            OnGoing = 1,
            Checkmate = 2,
            StaleMate = 3,
            Draw = 4,
            Reset = 5
        }

        public enum GameModeSelection
        {
            None = 0,
            LocalCoop = 1,
            SinglePlayer = 2,
            MultiPlayer = 3
        }


        // Mirrors the engine's PlayerCapturedPieceEvent, where Side and bool are a single byte each
        [StructLayout(LayoutKind.Sequential)]
        public struct PlayerCapturedPiece
        {
            private sbyte _playerColor;
            public PieceType pieceType;
            private byte _captured;

            public readonly Side playerColor => (Side)_playerColor;
            public readonly bool captured => _captured != 0;
        };


        [StructLayout(LayoutKind.Sequential)]
        public struct EndGameStateEvent
        {
            public EndGameState State;
            public Side winner;
        };


        public struct GameConfiguration
        {
            public GameModeSelection Mode;
            public Side PlayerColor;        // SinglePlayer: the human player. Multiplayer: the local player.
            public CPUDifficulty CpuDifficulty;  // Only used for SinglePlayer mode

            /// <summary>
            /// Creates configuration for local cooperative mode.
            /// </summary>
            public static GameConfiguration CreateLocalCoop()
            {
                return new GameConfiguration
                {
                    Mode = GameModeSelection.LocalCoop,
                    PlayerColor = Side.White,
                    CpuDifficulty = CPUDifficulty.None
                };
            }

            /// <summary>
            /// Creates configuration for single-player mode against CPU.
            /// </summary>
            public static GameConfiguration CreateSinglePlayer(Side humanColor, CPUDifficulty difficulty)
            {
                return new GameConfiguration
                {
                    Mode = GameModeSelection.SinglePlayer,
                    PlayerColor = humanColor,
                    CpuDifficulty = difficulty
                };
            }

            /// <summary>
            /// Creates configuration for multiplayer mode.
            /// </summary>
            public static GameConfiguration CreateMultiplayer(Side localPlayerColor)
            {
                return new GameConfiguration
                {
                    Mode = GameModeSelection.MultiPlayer,
                    PlayerColor = localPlayerColor,
                    CpuDifficulty = CPUDifficulty.None
                };
            }

            /// <summary>
            /// Validates the configuration based on mode.
            /// </summary>
            public bool IsValid()
            {
                return Mode switch
                {
                    GameModeSelection.LocalCoop => true,
                    GameModeSelection.SinglePlayer => PlayerColor != Side.None && CpuDifficulty != CPUDifficulty.None,
                    GameModeSelection.MultiPlayer => PlayerColor != Side.None,
                    _ => false
                };
            }


        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct MoveEvent
        {
            public ushort data;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 250)]
            public string moveNotation;
        }


        #endregion  // Structures and Enums
    }
}
