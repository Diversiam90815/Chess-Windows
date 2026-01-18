using Chess.UI.Board;
using Chess.UI.Moves;
using Chess.UI.Services;
using System;
using System.Collections.Generic;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Models
{
    /// <summary>
    /// Move flags matching the C++ MoveFlag enum
    /// </summary>
    public enum MoveFlag : byte
    {
        Quiet = 0b0000,
        DoublePawnPush = 0b0001,
        KingCastle = 0b0010,
        QueenCastle = 0b0011,
        Capture = 0b0100,
        EnPassant = 0b0101,
        KnightPromotion = 0b1000,
        BishopPromotion = 0b1001,
        RookPromotion = 0b1010,
        QueenPromotion = 0b1011,
        KnightPromoCapture = 0b1100,
        BishopPromoCapture = 0b1101,
        RookPromoCapture = 0b1110,
        QueenPromoCapture = 0b1111
    }


    /// <summary>
    /// Represents a chess move encoded in 16 bits
    /// Encoding: 0-5: source square, 6-11: target square, 12-15: flags
    /// </summary>
    public readonly struct Move
    {
        private readonly ushort _data;

        public Move(ushort data)
        {
            _data = data;
        }

        public Move(Square from, Square to, MoveFlag flags = MoveFlag.Quiet)
        {
            _data = (ushort)(((int)from) | ((int)to << 6) | ((int)flags << 12));
        }

        // Accessors
        public Square From => (Square)(_data & 0x3F);
        public Square To => (Square)((_data >> 6) & 0x3F);
        public MoveFlag Flags => (MoveFlag)((_data >> 12) & 0x0F);
        public ushort Raw => _data;


        // Move type checks
        public bool IsQuiet => Flags == MoveFlag.Quiet;
        public bool IsCapture => ((_data >> 12) & 0b0100) != 0;
        public bool IsPromotion => ((_data >> 12) & 0b1000) != 0;
        public bool IsCastle => Flags == MoveFlag.KingCastle || Flags == MoveFlag.QueenCastle;
        public bool IsEnPassant => Flags == MoveFlag.EnPassant;
        public bool IsDoublePush => Flags == MoveFlag.DoublePawnPush;
        public bool IsValid => _data != 0;


        // Get promotion piece type (only valid if IsPromotion is true)
        public PieceType PromotionPieceOffset => ((PieceType)((int)Flags & 0b0011));

        public static Move None => new Move(0);

        public override string ToString()
        {
            if (!IsValid) return "None";
            return $"{From}->{To} ({Flags})";
        }
    }


    public interface IMoveModel
    {
        List<Move> LegalMoves { get; }

        public event Action ChesspieceSelected;
        event Action LegalMovesCalculated;
        event Action<Side> PlayerChanged;
        event Action GameStateInitSucceeded;
        event Action<EndGameState, Side> GameOverEvent;
        event Action NewBoardFromBackendEvent;
        event Action PawnPromotionEvent;

        void SetPromotionPieceType(PieceType pieceType);
    }


    public class MoveModel : IMoveModel
    {
        private List<Move> legalMoves = [];

        public List<Move> LegalMoves => legalMoves;


        public MoveModel()
        {
            var logicCommunication = App.Current.ChessLogicCommunication as CommunicationLayer;
            logicCommunication.GameStateChanged += HandleGameStateChanged;
            logicCommunication.PlayerChanged += HandlePlayerChanged;
            logicCommunication.EndGameStateEvent += HandleEndGameState;
        }


        private void HandleGameStateChanged(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.Initializing:
                    {
                        //GameStateInitSucceeded?.Invoke();
                        break;
                    }
                case GamePhase.PlayerTurn:
                    {
                        break;
                    }
                case GamePhase.OpponentTurn:
                    {
                        break;
                    }
                case GamePhase.PromotionDialog:
                    {
                        break;
                    }
                case GamePhase.GameEnded:
                    {
                        break;
                    }


                //case GameState.WaitingForInput:
                //    {
                //        //NewBoardFromBackendEvent?.Invoke();
                //        break;
                //    }
                //case GameState.WaitingForTarget:
                //    {
                //        //HandleWaitingForTarget();
                //        break;
                //    }
                //case GameState.ValidatingMove:
                //    {
                //        break;
                //    }
                //case GameState.ExecutingMove:
                //    {
                //        //NewBoardFromBackendEvent?.Invoke();
                //        break;
                //    }
                //case GameState.WaitingForCPUMove:
                //    {
                //        //NewBoardFromBackendEvent?.Invoke();
                //        break;
                //    }
                //case GameState.GameOver:
                //    {
                //        break;
                //    }
                //case GameState.PawnPromotion:
                //    {
                //        //PawnPromotionEvent?.Invoke();
                //        break;
                //    }
                //case GameState.WaitingForRemoteMove:
                //    {
                //        //NewBoardFromBackendEvent?.Invoke();
                //        //RemotePlayersTurn?.Invoke();
                //        break;
                //    }
                default:
                    break;
            }
        }


        public void HandleEndGameState(EndGameStateEvent endgame)
        {
            EndGameState state = endgame.State;
            Side winner = endgame.winner;

            GameOverEvent?.Invoke(state, winner);
        }


        private List<Move> GetLegalMoves()
        {
            LegalMoves.Clear();

            int numMoves = EngineAPI.GetNumLegalMoves();

            for (int i = 0; i < numMoves; ++i)
            {
                if (EngineAPI.GetLegalMoveAtIndex(i, out ushort moveData))
                {
                    var move = new Move(moveData);
                    LegalMoves.Add(move);
                }
            }
            return LegalMoves;
        }


        private void HandleWaitingForTarget()
        {
            Logger.LogInfo("Due to delegate message WaitingForTarget we start getting the moves!");

            // We have selected a chesspiece and started the move cycle
            ChesspieceSelected?.Invoke();

            GetLegalMoves();

            LegalMovesCalculated?.Invoke();
        }


        public void SetPromotionPieceType(PieceType pieceType)
        {
            Logger.LogInfo("Promoting to " + pieceType.ToString());
            OnPawnPromotionChosen((int)pieceType);
        }


        public void HandlePlayerChanged(Side player)
        {
            PlayerChanged?.Invoke(player);
        }


        public event Action ChesspieceSelected;
        public event Action LegalMovesCalculated;
        public event Action<Side> PlayerChanged;
        public event Action RemotePlayersTurn;
        public event Action GameStateInitSucceeded;
        public event Action<EndGameState, Side> GameOverEvent;
        public event Action NewBoardFromBackendEvent;
        public event Action PawnPromotionEvent;
    }
}
