using Chess.UI.Models;
using Chess.UI.Wrappers;
using System.Collections.ObjectModel;
using System.Linq;


namespace Chess.UI.ViewModels
{
    public class MoveHistoryViewModel
    {
        private const int MovesMaxColumns = 3;

        public ObservableCollection<ObservableCollection<string>> MoveHistoryColumns { get; } = [];

        private IMoveHistoryModel Model { get; }

        private readonly IDispatcherQueueWrapper _dispatcherQueue;


        public MoveHistoryViewModel(IDispatcherQueueWrapper dispatcher, IMoveHistoryModel model)
        {
            _dispatcherQueue = dispatcher;
            Model = model;

            for (int i = 0; i < MovesMaxColumns; i++)
            {
                MoveHistoryColumns.Add(new ObservableCollection<string>());
            }
            Model.MoveHistoryUpdated += OnHandleMoveHistoryUpdated;
        }


        public void AddMove(string move)
        {
            // Find the column with the least number of moves
            var minColumn = MoveHistoryColumns.OrderBy(col => col.Count).First();

            minColumn.Add(move);
        }


        public void ClearMoveHistory()
        {
            foreach (var column in MoveHistoryColumns)
            {
                column.Clear();
            }
        }


        public void RemoveLastMove()
        {
            OnHandleMoveHistoryUpdated();
        }


        private void OnHandleMoveHistoryUpdated()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                ClearMoveHistory();

                foreach (var moveNotation in Model.MoveHistory)
                {
                    AddMove(moveNotation);
                }
            });
        }
    }
}
