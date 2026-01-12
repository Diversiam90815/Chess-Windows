using Chess.UI.Services;
using System;
using System.Collections.Generic;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Moves
{
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
