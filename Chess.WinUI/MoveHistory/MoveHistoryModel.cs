using Chess.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using static Chess.UI.Services.EngineAPI;

namespace Chess.UI.MoveHistory
{
    public class MoveHistoryModel : IMoveHistoryModel
    {
        public List<string> MoveHistory { get; } = new();


        public MoveHistoryModel()
        {
            var logicCommunication = App.Current.ChessLogicCommunication as CommunicationLayer;
            logicCommunication.MoveHistoryUpdated += UpdateMoveHistory;
        }


        private void UpdateMoveHistory(MoveHistoryEvent moveHistoryEvent)
        {
            if (moveHistoryEvent.added)
            {
                string moveNotation = moveHistoryEvent.moveNotation;
                MoveHistory.Add(moveNotation);
            }
            else
            {
                MoveHistory.Clear();
            }

            MoveHistoryUpdated?.Invoke();
        }


        public void RemoveLastMove()
        {
            MoveHistory.Remove(MoveHistory.LastOrDefault());
        }


        public event Action MoveHistoryUpdated;
    }
}
