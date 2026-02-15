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

        private readonly IMoveHistoryModel _model;
        private readonly IDispatcherQueueWrapper _dispatcherQueue;


        public MoveHistoryViewModel(IDispatcherQueueWrapper dispatcher, IMoveHistoryModel model)
        {
            _dispatcherQueue = dispatcher;
            _model = model;

            for (int i = 0; i < MovesMaxColumns; i++)
            {
                MoveHistoryColumns.Add(new ObservableCollection<string>());
            }
            
            _model.MoveHistoryUpdated += OnHandleMoveHistoryUpdated;
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


        private void OnHandleMoveHistoryUpdated()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                ClearMoveHistory();

                foreach (var moveNotation in _model.MoveHistory)
                {
                    AddMove(moveNotation);
                }
            });
        }
    }
}
