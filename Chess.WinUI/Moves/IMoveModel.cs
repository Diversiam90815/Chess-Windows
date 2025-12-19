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
        event Action<PlayerColor> PlayerChanged;
        event Action GameStateInitSucceeded;
        event Action<EndGameState, PlayerColor> GameOverEvent;
        event Action NewBoardFromBackendEvent;
        event Action PawnPromotionEvent;

        void SetPromotionPieceType(PieceTypeInstance pieceType);
    }
}