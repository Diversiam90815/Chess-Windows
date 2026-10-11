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
        public static extern void Init([In, MarshalAs(UnmanagedType.Struct)] UserSettingsInit settings);

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

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetGameState();

        #endregion // Game Management


        #region Moves & Input Generation

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void OnSquareSelected(int square);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void OnPawnPromotionChosen(int pieceType);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumLegalMoves();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetLegalMoveAtIndex(int index, out ushort move);

        #endregion // Moves & Input Generation


        #region Multiplayer

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartMultiplayer();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void StopMultiplayer();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void FindOpponent();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetDiscoveredOpponentCount();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetDiscoveredOpponentAtIndex(int index, StringBuilder name, int maxLen);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ConnectToOpponent(int index);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void RespondToConnectionRequest([MarshalAs(UnmanagedType.I1)] bool accept);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLocalPlayer(int iLocalPlayer);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLocalPlayerReady([MarshalAs(UnmanagedType.I1)] bool ready);

        #endregion // Multiplayer


        #region Network

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNetworkAdapterCount();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetNetworkAdapterAtIndex(uint index, out NetworkAdapterInstance adapter);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ChangeCurrentAdapter(int ID);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCurrentNetworkAdapterID();

        #endregion


        #region Logging

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void LogInfoWithCaller(
            [MarshalAs(UnmanagedType.LPStr)] string message,
            [MarshalAs(UnmanagedType.LPStr)] string functionName,
            [MarshalAs(UnmanagedType.LPStr)] string className,
            int lineNumber);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void LogErrorWithCaller(
            [MarshalAs(UnmanagedType.LPStr)] string message,
            [MarshalAs(UnmanagedType.LPStr)] string functionName,
            [MarshalAs(UnmanagedType.LPStr)] string className,
            int lineNumber);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void LogWarningWithCaller(
            [MarshalAs(UnmanagedType.LPStr)] string message,
            [MarshalAs(UnmanagedType.LPStr)] string functionName,
            [MarshalAs(UnmanagedType.LPStr)] string className,
            int lineNumber);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void LogDebugWithCaller(
            [MarshalAs(UnmanagedType.LPStr)] string message,
            [MarshalAs(UnmanagedType.LPStr)] string functionName,
            [MarshalAs(UnmanagedType.LPStr)] string className,
            int lineNumber);

        #endregion


        #region Settings

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLocalPlayerName([MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetDiscoveryPort(int port);

        #endregion // Settings


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

        // State of the engine's state machine, as returned by GetGameState()
        public enum GameState
        {
            NotRunning = 0,
            Init = 1,
            WaitingForInput = 2,
            WaitingForTarget = 3,
            PawnPromotion = 4,
            WaitingForRemoteMove = 5,
            WaitingForCPUMove = 6,
            GameOver = 7,
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
        public struct MultiplayerStateEvent
        {
            public MultiplayerState State;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 250)]
            public string remoteName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 250)]
            public string errorMessage;
        };


        [StructLayout(LayoutKind.Sequential)]
        public struct EndGameStateEvent
        {
            public EndGameState State;
            public Side winner;
        };


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct NetworkAdapterInstance
        {
            public int ID;
            public int Priority;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 250)]
            private byte[] _adapterNameBytes;

            public string AdapterName
            {
                get
                {
                    int nullIndex = Array.IndexOf(_adapterNameBytes, (byte)0);
                    int length = (nullIndex >= 0) ? nullIndex : _adapterNameBytes.Length;
                    return Encoding.UTF8.GetString(_adapterNameBytes, 0, length);
                }
            }

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 250)]
            private byte[] _networkNameBytes;

            public string NetworkName
            {
                get
                {
                    int nullIndex = Array.IndexOf(_networkNameBytes, (byte)0);
                    int length = (nullIndex >= 0) ? nullIndex : _networkNameBytes.Length;
                    return Encoding.UTF8.GetString(_networkNameBytes, 0, length);
                }
            }
        }


        public struct GameConfiguration
        {
            public GameModeSelection Mode;
            public Side PlayerColor;        // Used for SinglePlayer and Multiplayer
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


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct UserSettingsInit
        {
            // Player
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 250)]
            public string PlayerName;

            // Network
            public int DiscoveryUDPPort;

            // Paths (the engine creates its "Logs" folder underneath)
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
            public string LogFolder;
        }


        #endregion  // Structures and Enums
    }
}
