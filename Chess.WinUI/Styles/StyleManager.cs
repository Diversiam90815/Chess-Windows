using Chess.UI.Wrappers;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace Chess.UI.Styles
{
    public interface IStyleManager : INotifyPropertyChanged
    {
        BoardStyle CurrentBoardStyle { get; set; }
        PieceStyle CurrentPieceStyle { get; set; }

        void SaveStyles();
        void LoadStyles();
    }


    public class StyleManager : IStyleManager
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IDispatcherQueueWrapper _dispatcherQueue;


        public StyleManager(IDispatcherQueueWrapper dispatcher)
        {
            _dispatcherQueue = dispatcher;

            LoadStyles();
        }


        private BoardStyle _currentBoardStyle;
        public BoardStyle CurrentBoardStyle
        {
            get => _currentBoardStyle;
            set
            {
                if (_currentBoardStyle != value)
                {
                    _currentBoardStyle = value;
                    OnPropertyChanged();
                    SaveStyles();
                }
            }
        }


        private PieceStyle _currentPieceStyle;
        public PieceStyle CurrentPieceStyle
        {
            get => _currentPieceStyle;
            set
            {
                if (_currentPieceStyle != value)
                {
                    _currentPieceStyle = value;
                    OnPropertyChanged();
                    SaveStyles();
                }
            }
        }


        public void LoadStyles()
        {
            // Load from settings without triggering save
            string configPieceStyle = Settings.Settings.CurrentPieceTheme;
            string configBoardStyle = Settings.Settings.CurrentBoardTheme;

            if (!Enum.TryParse<PieceStyle>(configPieceStyle, out var parsedPieceStyle))
            {
                parsedPieceStyle = PieceStyle.Standard;
            }
            _currentPieceStyle = parsedPieceStyle;

            if (!Enum.TryParse<BoardStyle>(configBoardStyle, out var parsedBoardStyle))
            {
                parsedBoardStyle = BoardStyle.Wood;
            }
            _currentBoardStyle = parsedBoardStyle;
        }


        public void SaveStyles()
        {
            Settings.Settings.CurrentPieceTheme = _currentPieceStyle.ToString();
            Settings.Settings.CurrentBoardTheme = _currentBoardStyle.ToString();
        }


        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }
}
