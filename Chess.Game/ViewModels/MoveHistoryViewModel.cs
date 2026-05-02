using Chess.UI.Models;
using Chess.UI.Wrappers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;


namespace Chess.UI.ViewModels
{
    public class MoveHistoryEntry : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int MoveNumber { get; set; }

        private string _whiteMove = string.Empty;
        public string WhiteMove
        {
            get => _whiteMove;
            set
            {
                if (_whiteMove != value)
                {
                    _whiteMove = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WhiteMove)));
                }
            }
        }

        private string _blackMove = string.Empty;
        public string BlackMove
        {
            get => _blackMove;
            set
            {
                if (_blackMove != value)
                {
                    _blackMove = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BlackMove)));
                }
            }
        }
    }


    public class MoveHistoryViewModel
    {
        public ObservableCollection<MoveHistoryEntry> MoveEntries { get; } = [];

        private readonly IMoveHistoryModel _model;
        private readonly IDispatcherQueueWrapper _dispatcherQueue;


        public MoveHistoryViewModel(IDispatcherQueueWrapper dispatcher, IMoveHistoryModel model)
        {
            _dispatcherQueue = dispatcher;
            _model = model;

            _model.MoveHistoryUpdated += OnHandleMoveHistoryUpdated;
        }


        public void OnReset()
        {
            _model.ClearHistory();
            MoveEntries.Clear();
        }


        private void RebuildFromModel()
        {
            MoveEntries.Clear();

            var moves = _model.MoveHistory;

            for (int i = 0; i < moves.Count; i += 2)
            {
                MoveEntries.Add(new MoveHistoryEntry
                {
                    MoveNumber = (i / 2) + 1,
                    WhiteMove = moves[i],
                    BlackMove = (i + 1 < moves.Count) ? moves[i + 1] : string.Empty
                });
            }
        }


        private void OnHandleMoveHistoryUpdated()
        {
            _dispatcherQueue.TryEnqueue(RebuildFromModel);
        }
    }
}
