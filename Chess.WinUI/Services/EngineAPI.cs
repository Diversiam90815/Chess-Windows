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


        #region Program

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Init", CharSet = CharSet.Unicode)]
        public static extern void Init();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "Deinit", CharSet = CharSet.Unicode)]
        public static extern void Deinit();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetDelegate", CharSet = CharSet.Unicode)]
        public static extern void SetDelegate(APIDelegate pDelegate);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetWindowScalingFactor", CharSet = CharSet.Unicode)]
        public static extern float GetWindowScalingFactor(nint hwnd);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetUnvirtualizedAppDataPath", CharSet = CharSet.Unicode)]
        public static extern void SetUnvirtualizedAppDataPath([In()][MarshalAs(UnmanagedType.LPStr)] string appDataPath);


        #endregion // Program


        #region Moves

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetNumPossibleMoves", CharSet = CharSet.Unicode)]
        public static extern int GetNumPossibleMoves();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetPossibleMoveAtIndex", CharSet = CharSet.Unicode)]
        public static extern bool GetPossibleMoveAtIndex(uint index, out PossibleMoveInstance move);

        #endregion // Moves


        #region Board

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetBoardState", CharSet = CharSet.Unicode)]
        public static extern bool GetBoardState([Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 64)] int[] boardState);

        #endregion // Board


        #region Game

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "StartGame", CharSet = CharSet.Unicode)]
        public static extern void StartGame([In, MarshalAs(UnmanagedType.Struct)] GameConfiguration config);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ResetGame", CharSet = CharSet.Unicode)]
        public static extern void ResetGame();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "UndoMove", CharSet = CharSet.Unicode)]
        public static extern void UndoMove();

        #endregion // Game


        #region State Machine

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "OnSquareSelected", CharSet = CharSet.Unicode)]
        public static extern void OnSquareSelected(PositionInstance position);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "OnPawnPromotionChosen", CharSet = CharSet.Unicode)]
        public static extern void OnPawnPromotionChosen(PieceTypeInstance promotion);


        #endregion  // State Machine


        #region Multiplayer

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "StartedMultiplayer", CharSet = CharSet.Unicode)]
        public static extern void StartedMultiplayer();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "StartMultiplayerGame", CharSet = CharSet.Unicode)]
        public static extern void StartMultiplayerGame();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "StartRemoteDiscovery", CharSet = CharSet.Unicode)]
        public static extern void StartRemoteDiscovery(bool isHost);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "DisconnectMultiplayerGame", CharSet = CharSet.Unicode)]
        public static extern void DisconnectMultiplayerGame();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "AnswerConnectionInvitation", CharSet = CharSet.Unicode)]
        public static extern void AnswerConnectionInvitation(bool accepted);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SendConnectionRequestToHost", CharSet = CharSet.Unicode)]
        public static extern void SendConnectionRequestToHost();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "StoppedMultiplayer", CharSet = CharSet.Unicode)]
        public static extern void StoppedMultiplayer();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetLocalPlayer", CharSet = CharSet.Unicode)]
        public static extern void SetLocalPlayer(int iLocalPlayer);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetLocalPlayerReady", CharSet = CharSet.Unicode)]
        public static extern void SetLocalPlayerReady(bool ready);

        #endregion // Multiplayer


        #region Logging

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "LogInfoWithCaller", CharSet = CharSet.Unicode)]
        public static extern void LogInfoWithCaller([In()][MarshalAs(UnmanagedType.LPStr)] string message, [In()][MarshalAs(UnmanagedType.LPStr)] string functionName, [In()][MarshalAs(UnmanagedType.LPStr)] string className, int lineNumber);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "LogErrorWithCaller", CharSet = CharSet.Unicode)]
        public static extern void LogErrorWithCaller([In()][MarshalAs(UnmanagedType.LPStr)] string message, [In()][MarshalAs(UnmanagedType.LPStr)] string functionName, [In()][MarshalAs(UnmanagedType.LPStr)] string className, int lineNumber);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "LogWarningWithCaller", CharSet = CharSet.Unicode)]
        public static extern void LogWarningWithCaller([In()][MarshalAs(UnmanagedType.LPStr)] string message, [In()][MarshalAs(UnmanagedType.LPStr)] string functionName, [In()][MarshalAs(UnmanagedType.LPStr)] string className, int lineNumber);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "LogDebugWithCaller", CharSet = CharSet.Unicode)]
        public static extern void LogDebugWithCaller([In()][MarshalAs(UnmanagedType.LPStr)] string message, [In()][MarshalAs(UnmanagedType.LPStr)] string functionName, [In()][MarshalAs(UnmanagedType.LPStr)] string className, int lineNumber);

        #endregion // Logging


        #region User Config

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetCurrentBoardTheme", CharSet = CharSet.Unicode)]
        public static extern void SetCurrentBoardTheme([In()][MarshalAs(UnmanagedType.LPStr)] string theme);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetCurrentBoardTheme", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetCurrentBoardTheme();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetCurrentPieceTheme", CharSet = CharSet.Unicode)]
        public static extern void SetCurrentPieceTheme([In()][MarshalAs(UnmanagedType.LPStr)] string theme);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetCurrentPieceTheme", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetCurrentPieceTheme();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetLocalPlayerName", CharSet = CharSet.Unicode)]
        public static extern void SetLocalPlayerName([In()][MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetLocalPlayerName", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetLocalPlayerName();



        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetSFXEnabled", CharSet = CharSet.Unicode)]
        public static extern bool GetSFXEnabled();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetSFXEnabled", CharSet = CharSet.Unicode)]
        public static extern void SetSFXEnabled(bool enabled);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetAtmosEnabled", CharSet = CharSet.Unicode)]
        public static extern bool GetAtmosEnabled();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetAtmosEnabled", CharSet = CharSet.Unicode)]
        public static extern void SetAtmosEnabled(bool enabled);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetSFXVolume", CharSet = CharSet.Unicode)]
        public static extern void SetSFXVolume(float volume);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetSFXVolume", CharSet = CharSet.Unicode)]
        public static extern float GetSFXVolume();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetAtmosVolume", CharSet = CharSet.Unicode)]
        public static extern void SetAtmosVolume(float volume);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetAtmosVolume", CharSet = CharSet.Unicode)]
        public static extern float GetAtmosVolume();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetMasterVolume", CharSet = CharSet.Unicode)]
        public static extern void SetMasterVolume(float volume);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetMasterVolume", CharSet = CharSet.Unicode)]
        public static extern float GetMasterVolume();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetAtmosScenario", CharSet = CharSet.Unicode)]
        public static extern void SetAtmosScenario([In()][MarshalAs(UnmanagedType.LPStr)] string scenario);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetAtmosScenario", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetAtmosScenario();

        #endregion // User Config


        #region Network

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetNetworkAdapterCount", CharSet = CharSet.Unicode)]
        public static extern int GetNetworkAdapterCount();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetNetworkAdapterAtIndex", CharSet = CharSet.Unicode)]
        public static extern bool GetNetworkAdapterAtIndex(uint index, out NetworkAdapterInstance adapter);

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetSavedAdapterID", CharSet = CharSet.Unicode)]
        public static extern int GetSavedAdapterID();

        [DllImport(LOGIC_API_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ChangeCurrentAdapter", CharSet = CharSet.Unicode)]
        public static extern void ChangeCurrentAdapter(int ID);

        #endregion // Network


        #endregion // DLL Defines


        #region Structures & Enums

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct PositionInstance
        {
            public PositionInstance(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public int x { get; set; }
            public int y { get; set; }


            public override readonly bool Equals(object obj)
            {
                if (obj is PositionInstance)
                {
                    var other = (PositionInstance)obj;
                    return (this.x == other.x) && (this.y == other.y);
                }
                return false;
            }


            public static bool operator ==(PositionInstance left, PositionInstance right)
            {
                if (Equals(left, right))
                {
                    return true;
                }

                return left.Equals(right);
            }


            public static bool operator !=(PositionInstance left, PositionInstance right)
            {
                return !(left == right);
            }


            public override int GetHashCode()
            {
                int hashcode = 17;
                hashcode = hashcode * 23 + x.GetHashCode();
                hashcode = hashcode * 23 + y.GetHashCode();
                return hashcode;
            }
        }


        public enum PieceTypeInstance
        {
            DefaultType,
            Pawn,
            Knight,
            Bishop,
            Rook,
            Queen,
            King
        }


        public enum PlayerColor
        {
            NoColor,
            White,
            Black
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


        public enum GameState
        {
            Undefined = 0,
            Init = 1,
            InitSucceeded = 2,
            WaitingForInput = 3,
            MoveInitiated = 4,
            WaitingForTarget = 5,
            ValidatingMove = 6,
            ExecutingMove = 7,
            PawnPromotion = 8,
            WaitingForRemoteMove = 9,
            WaitingForCPUMove = 10,
            GameOver = 11,
        }


        public enum EndGameState
        {
            OnGoing = 1,
            Checkmate = 2,
            StaleMate = 3,
            Reset = 4
        }


        public enum GameModeSelection
        {
            None,
            LocalCoop,
            VsCPU,
        }


        [Flags]
        public enum MoveTypeInstance : int
        {
            MoveType_None = 0,
            MoveType_Normal = 1 << 0, // 1
            MoveType_DoublePawnPush = 1 << 1, // 2
            MoveType_PawnPromotion = 1 << 2, // 4
            MoveType_Capture = 1 << 3, // 8
            MoveType_EnPassant = 1 << 4, // 16
            MoveType_CastlingKingside = 1 << 5, // 32
            MoveType_CastlingQueenside = 1 << 6, // 64
            MoveType_Check = 1 << 7, // 128
            MoveType_Checkmate = 1 << 8, // 256
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct PossibleMoveInstance
        {
            public PositionInstance start;
            public PositionInstance end;
            public MoveTypeInstance type;
            public PieceTypeInstance promotionPiece;

            public override readonly bool Equals(object obj)
            {
                if (obj is PossibleMoveInstance)
                {
                    var other = (PossibleMoveInstance)obj;
                    return (start == other.start) && (end == other.end);
                }
                return false;
            }


            public static bool operator ==(PossibleMoveInstance left, PossibleMoveInstance right)
            {
                if (Equals(left, right))
                {
                    return true;
                }

                return left.Equals(right);
            }


            public static bool operator !=(PossibleMoveInstance left, PossibleMoveInstance right)
            {
                return !left.Equals(right);
            }


            public override int GetHashCode()
            {
                return start.GetHashCode() ^ end.GetHashCode();
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct Score
        {
            public PlayerColor player;
            public int score;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct PlayerCapturedPiece
        {
            public PlayerColor playerColor;
            public PieceTypeInstance pieceType;
            public bool captured;
        };


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct MoveHistoryEvent
        {
            public bool added;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 250)]
            public string moveNotation;
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
            public PlayerColor winner;
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


        [StructLayout(LayoutKind.Sequential)]
        public struct GameConfiguration
        {
            public GameModeSelection Mode;
            public PlayerColor PlayerColor;
            public CPUDifficulty CpuDifficulty;
        }



        #endregion  // Structures and Enums
    }
}
