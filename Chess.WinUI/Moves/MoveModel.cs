using Chess.UI.Services;
using System;
using System.Collections.Generic;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Moves
{
    public interface IMoveModel
    {
        List<PossibleMoveInstance> PossibleMoves { get; }

        public event Action ChesspieceSelected;
        event Action PossibleMovesCalculated;
        event Action<Side> PlayerChanged;
        event Action GameStateInitSucceeded;
        event Action<EndGameState, Side> GameOverEvent;
        event Action NewBoardFromBackendEvent;
        event Action PawnPromotionEvent;

        void SetPromotionPieceType(PieceType pieceType);
    }


    public class MoveModel : IMoveModel
    {
        private List<PossibleMoveInstance> possibleMoves = [];

        public List<PossibleMoveInstance> PossibleMoves => possibleMoves;


        public MoveModel()
        {
            var logicCommunication = App.Current.ChessLogicCommunication as CommunicationLayer;
            logicCommunication.GameStateChanged += HandleGameStateChanged;
            logicCommunication.PlayerChanged += HandlePlayerChanged;
            logicCommunication.EndGameStateEvent += HandleEndGameState;
        }


        private void HandleGameStateChanged(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.InitSucceeded:
                    {
                        GameStateInitSucceeded?.Invoke();
                        break;
                    }
                case GameState.WaitingForInput:
                    {
                        NewBoardFromBackendEvent?.Invoke();
                        break;
                    }
                case GameState.WaitingForTarget:
                    {
                        HandleWaitingForTarget();
                        break;
                    }
                case GameState.ValidatingMove:
                    {
                        break;
                    }
                case GameState.ExecutingMove:
                    {
                        NewBoardFromBackendEvent?.Invoke();
                        break;
                    }
                case GameState.WaitingForCPUMove:
                    {
                        NewBoardFromBackendEvent?.Invoke();
                        break;
                    }
                case GameState.GameOver:
                    {
                        break;
                    }
                case GameState.PawnPromotion:
                    {
                        PawnPromotionEvent?.Invoke();
                        break;
                    }
                case GameState.WaitingForRemoteMove:
                    {
                        NewBoardFromBackendEvent?.Invoke();
                        RemotePlayersTurn?.Invoke();
                        break;
                    }
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


        private void HandleWaitingForTarget()
        {
            Logger.LogInfo("Due to delegate message WaitingForTarget we start getting the moves!");

            // We have selected a chesspiece and started the move cycle
            ChesspieceSelected?.Invoke();

            PossibleMoves.Clear();

            int numMoves = GetNumPossibleMoves();
            for (int i = 0; i < numMoves; i++)
            {
                if (GetPossibleMoveAtIndex((uint)i, out var move))
                {
                    PossibleMoves.Add(move);
                }
            }
            PossibleMovesCalculated?.Invoke();
        }


        public void SetPromotionPieceType(PieceType pieceType)
        {
            Logger.LogInfo("Promoting to " + pieceType.ToString());
            OnPawnPromotionChosen(pieceType);
        }


        public void HandlePlayerChanged(Side player)
        {
            PlayerChanged?.Invoke(player);
        }


        public event Action ChesspieceSelected;
        public event Action PossibleMovesCalculated;
        public event Action<Side> PlayerChanged;
        public event Action RemotePlayersTurn;
        public event Action GameStateInitSucceeded;
        public event Action<EndGameState, Side> GameOverEvent;
        public event Action NewBoardFromBackendEvent;
        public event Action PawnPromotionEvent;
    }
}
