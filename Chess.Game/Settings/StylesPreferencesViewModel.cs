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
    public class StylesPreferencesViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IDispatcherQueueWrapper _dispatcherQueueWrapper;

        private readonly IStyleLoader _styleLoader;

        private readonly IStyleManager _styleManager;

        private readonly ISettingsService _settingsService;

        public event Action ItemSelected;

        private bool _isInitialized = false;
        private bool _userInteractionEnabled = false;


        public ObservableCollection<BoardStyleInfo> BoardThemes { get; }

        public ObservableCollection<PieceStyleInfo> PieceThemes { get; }


        // Completes once the available styles are loaded and the saved ones are selected
        public Task Initialization { get; }


        public StylesPreferencesViewModel(IDispatcherQueueWrapper dispatcherQueue, IStyleManager themeManager, ISettingsService settingsService, IStyleLoader styleLoader)
        {
            _dispatcherQueueWrapper = dispatcherQueue;
            _styleLoader = styleLoader;
            _styleManager = themeManager;
            _settingsService = settingsService;

            BoardThemes = [];
            PieceThemes = [];

            Initialization = InitializeAsync();
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

                await _dispatcherQueueWrapper.EnqueueAsync(() =>
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
            string currentThemeName = _settingsService.BoardStyle;
            BoardStyleInfo theme = BoardThemes.FirstOrDefault(b => string.Equals(b.Name, currentThemeName, StringComparison.OrdinalIgnoreCase));
            return theme;
        }


        public PieceStyleInfo GetCurrentSelectedPieceTheme()
        {
            string currentThemeName = _settingsService.ChessPieceStyle;
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
                        _settingsService.BoardStyle = value.Name;

                    OnPropertyChanged();

                    // Update ThemeManager’s board theme
                    // This triggers property change events in the manager
                    if (value != null)
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
                        _settingsService.ChessPieceStyle = value.Name;

                    OnPropertyChanged();

                    // Update ThemeManager’s piece theme
                    if (value != null)
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
