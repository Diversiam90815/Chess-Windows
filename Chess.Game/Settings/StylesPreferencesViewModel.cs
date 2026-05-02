using Chess.UI.Services;
using Chess.UI.Styles;
using Chess.UI.Wrappers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace Chess.UI.Settings
{
    public class StylesPreferencesViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IDispatcherQueueWrapper _dispatcherQueueWrapper;

        private readonly StyleLoader _styleLoader;

        private readonly IStyleManager _styleManager;

        public event Action ItemSelected;

        private bool _isInitialized = false;
        private bool _userInteractionEnabled = false;


        public ObservableCollection<BoardStyleInfo> BoardThemes { get; }

        public ObservableCollection<PieceStyleInfo> PieceThemes { get; }


        public StylesPreferencesViewModel(IDispatcherQueueWrapper dispatcherQueue, IStyleManager themeManager)
        {
            _dispatcherQueueWrapper = dispatcherQueue;
            _styleLoader = new();
            _styleManager = themeManager;

            BoardThemes = [];
            PieceThemes = [];

            _ = InitializeAsync();
        }


        private async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                var (boardThemes, pieceThemes) = await Task.Run(() =>
                {
                    var boards = _styleLoader.LoadBoardStyles();
                    var pieces = _styleLoader.LoadPieceStyles();
                    return (boards, pieces);
                });

                _dispatcherQueueWrapper.TryEnqueue(() =>
                {
                    foreach (var board in boardThemes)
                        BoardThemes.Add(board);

                    foreach (var piece in pieceThemes)
                        PieceThemes.Add(piece);

                    SelectedBoardTheme = GetCurrentSelectedBoardTheme();
                    SelectedPieceTheme = GetCurrentSelectedPieceTheme();

                    _isInitialized = true;
                    _userInteractionEnabled = true;
                });
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to initialize themes: {ex.Message}");
            }

        }


        public BoardStyleInfo GetCurrentSelectedBoardTheme()
        {
            string currentThemeName = Settings.CurrentBoardTheme;
            BoardStyleInfo theme = BoardThemes.FirstOrDefault(b => string.Equals(b.Name, currentThemeName, StringComparison.OrdinalIgnoreCase));
            return theme;
        }


        public PieceStyleInfo GetCurrentSelectedPieceTheme()
        {
            string currentThemeName = Settings.CurrentPieceTheme;
            PieceStyleInfo theme = PieceThemes.FirstOrDefault(p => string.Equals(p.Name, currentThemeName, StringComparison.OrdinalIgnoreCase));
            return theme;
        }


        private BoardStyleInfo _selectedBoardTheme;
        public BoardStyleInfo SelectedBoardTheme
        {
            get => _selectedBoardTheme;
            set
            {
                if (_selectedBoardTheme != value)
                {
                    _selectedBoardTheme = value;

                    if (value != null)
                        Settings.CurrentBoardTheme = value.Name;

                    OnPropertyChanged();

                    // Update ThemeManager’s board theme
                    // This triggers property change events in the manager
                    _styleManager.CurrentBoardStyle = value.Style;

                    if (_userInteractionEnabled)
                        ItemSelected?.Invoke();
                }
            }
        }


        private PieceStyleInfo _selectedPieceTheme;
        public PieceStyleInfo SelectedPieceTheme
        {
            get => _selectedPieceTheme;
            set
            {
                if (_selectedPieceTheme != value)
                {
                    _selectedPieceTheme = value;
                    if (value != null)
                        Settings.CurrentPieceTheme = value.Name;

                    OnPropertyChanged();

                    // Update ThemeManager’s piece theme
                    _styleManager.CurrentPieceStyle = value.Style;

                    if (_userInteractionEnabled)
                        ItemSelected?.Invoke();
                }
            }
        }


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            _dispatcherQueueWrapper.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }
}
