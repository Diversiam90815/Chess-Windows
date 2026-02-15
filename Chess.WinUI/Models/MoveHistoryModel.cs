using Chess.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Chess.UI.Models
{
    public interface IMoveHistoryModel
    {
        // Properties
        List<string> MoveHistory { get; }

        // Methods
        void OnMoveExecuted(Move move, string notation);
        void OnMoveUndone();

        // Events
        event Action MoveHistoryUpdated;
    }


    public class MoveHistoryModel : IMoveHistoryModel
    {
        private readonly ICommunicationLayer _backendCommunication;

        public event Action MoveHistoryUpdated;

        private static readonly List<string> list = [];

        public List<string> MoveHistory { get; } = list;


        public MoveHistoryModel(ICommunicationLayer communicationLayer)
        {
            _backendCommunication = communicationLayer;

            _backendCommunication.MoveExecuted += OnMoveExecuted;
            _backendCommunication.MoveUndone += OnMoveUndone;
        }


        public void OnMoveExecuted(Move move, string notation)
        {
            MoveHistory.Add(notation);
            MoveHistoryUpdated?.Invoke();
        }


        public void OnMoveUndone()
        {
            if (MoveHistory.Count <= 0)
                return;

            MoveHistory.Remove(MoveHistory.LastOrDefault());
            MoveHistoryUpdated?.Invoke();
        }
    }
}
