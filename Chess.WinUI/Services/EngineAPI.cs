using System;
using System.Runtime.InteropServices;
using System.Text;
using static Chess.UI.Services.CommunicationLayer;


namespace Chess.UI.Services
{
    public class EngineAPI
    {
        #region DLL Defines

        #region Defines

        private const string LOGIC_API_PATH = @"Chess.Engine.API.dll";

        #endregion // Defines


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
        public static extern bool GetBoardState([Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 64)] int[] boardState);

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
        public static extern void StartedMultiplayer();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartRemoteDiscovery([MarshalAs(UnmanagedType.I1)] bool isHost);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void AnswerConnectionInvitation([MarshalAs(UnmanagedType.I1)] bool accept);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SendConnectionRequestToHost();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void StoppedMultiplayer();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLocalPlayer(int iLocalPlayer);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLocalPlayerReady([MarshalAs(UnmanagedType.I1)] bool ready);

        #endregion // Multiplayer


        #region Settings

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCurrentBoardTheme([MarshalAs(UnmanagedType.LPStr)] string theme);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetCurrentBoardTheme();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCurrentPieceTheme([MarshalAs(UnmanagedType.LPStr)] string theme);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetCurrentPieceTheme();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLocalPlayerName([MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetLocalPlayerName();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetSFXEnabled();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetSFXEnabled([MarshalAs(UnmanagedType.I1)] bool enabled);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetAtmosEnabled();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAtmosEnabled([MarshalAs(UnmanagedType.I1)] bool enabled);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetSFXVolume(float volume);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern float GetSFXVolume();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAtmosVolume(float volume);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern float GetAtmosVolume();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAtmosScenario([MarshalAs(UnmanagedType.LPStr)] string scenario);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetAtmosScenario();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetMasterVolume(float volume);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern float GetMasterVolume();

        #endregion


        #region Network

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNetworkAdapterCount();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool GetNetworkAdapterAtIndex(uint index, out NetworkAdapterInstance adapter);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ChangeCurrentAdapter(int ID);

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


        #region Utilities

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetWindowScalingFactor", CharSet = CharSet.Unicode)]
        public static extern float GetWindowScalingFactor(nint hwnd);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetUnvirtualizedAppDataPath", CharSet = CharSet.Unicode)]
        public static extern void SetUnvirtualizedAppDataPath([In()][MarshalAs(UnmanagedType.LPStr)] string appDataPath);

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

        public enum ConnectionState
        {
            None = 0,
            HostingSession = 1,
            WaitingForARemote = 2,
            Connected = 3,
            Disconnected = 4,
            Error = 5,
            ConnectionRequested = 6, // Client has requested a connection to the host
            PendingHostApproval = 7, // Waiting for the host to approve the connection
            ClientFoundHost = 9, // Client found a host
            SetPlayerColor = 10,
            GameStarted = 11,
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


        [StructLayout(LayoutKind.Sequential)]
        public struct PlayerCapturedPiece
        {
            public Side playerColor;
            public PieceType pieceType;
            public bool captured;
        };


        [StructLayout(LayoutKind.Sequential)]
        public struct ConnectionStatusEvent
        {
            public ConnectionState ConnectionState;

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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 250)]
            private byte[] _adapterNameBytes;

            public string AdapterName
            {
                get
                {
                    // Find null terminator in the byte array
                    int nullIndex = Array.IndexOf(_adapterNameBytes, (byte)0);
                    int length = (nullIndex >= 0) ? nullIndex : _adapterNameBytes.Length;

                    // Convert bytes to a string using UTF-8 encoding
                    return Encoding.UTF8.GetString(_adapterNameBytes, 0, length);
                }
            }

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 250)]
            private byte[] _networkNameBytes;

            public string NetworkName
            {
                get
                {
                    // Find null terminator in the byte array
                    int nullIndex = Array.IndexOf(_networkNameBytes, (byte)0);
                    int length = (nullIndex >= 0) ? nullIndex : _networkNameBytes.Length;

                    // Convert bytes to a string using UTF-8 encoding
                    return Encoding.UTF8.GetString(_networkNameBytes, 0, length);
                }
            }

            public int ID;
            public int Visibility;
            public int Type;
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


            #endregion  // Structures and Enums
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct MoveEvent
        {
            public ushort data;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 250)]
            public string moveNotation;
        }

    }
}
