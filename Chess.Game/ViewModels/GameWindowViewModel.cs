using Chess.UI.Services;
using Chess.UI.Wrappers;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.ViewModels
{
    public class GameWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IDispatcherQueueWrapper _dispatcherQueue;
        private readonly IChessGameService _gameService;

        public ChessBoardViewModel ChessBoardViewModel { get; }
        public MoveHistoryViewModel MoveHistoryViewModel { get; }
        public CapturedPiecesViewModel CapturedPiecesViewModel { get; }
        public MultiplayerInfoViewModel MultiplayerInfoViewModel { get; }


        public event Func<EndGameState, Side, Task> ShowEndGameDialog;
        public event Func<Task<PieceType?>> ShowPawnPromotionDialogRequested;
        public event Action ButtonClicked;


        public GameWindowViewModel(
            IDispatcherQueueWrapper dispatcherQueue,
            IChessGameService gameService,
            ChessBoardViewModel chessBoardViewModel,
            CapturedPiecesViewModel capturedPiecesViewModel,
            MoveHistoryViewModel moveHistoryViewModel,
            MultiplayerInfoViewModel multiplayerInfoViewModel)
        {
            _dispatcherQueue = dispatcherQueue;
            _gameService = gameService;

            ChessBoardViewModel = chessBoardViewModel;
            CapturedPiecesViewModel = capturedPiecesViewModel;
            MoveHistoryViewModel = moveHistoryViewModel;
            MultiplayerInfoViewModel = multiplayerInfoViewModel;

            // Subscribe to ChessBoard events that require window-level handling
            ChessBoardViewModel.ShowEndGameDialog += OnShowEndGameDialog;
            ChessBoardViewModel.ShowPawnPromotionDialogRequested += OnShowPawnPromotionDialog;
            ChessBoardViewModel.ButtonClicked += OnButtonClicked;

            // Subscribe to game service events
            _gameService.GameReset += OnGameReset;
            _gameService.ConfigurationChanged += OnConfigurationChanged;

            // Apply current configuration if the game was already started before this VM was created
            if (_gameService.CurrentConfiguration.HasValue)
                OnConfigurationChanged(_gameService.CurrentConfiguration.Value);
        }



        #region Game Service Event Handlers

        private void OnGameReset()
        {
            Logger.LogInfo("Resetting all components");

            MoveHistoryViewModel.OnReset();
            CapturedPiecesViewModel.ClearCaptures();
        }


        private void OnConfigurationChanged(GameConfiguration config)
        {
            Logger.LogInfo($"Configuration changed - Mode={config.Mode}");

            // Update game mode flags
            IsMultiplayerGame = config.Mode == GameModeSelection.MultiPlayer;
            IsKoopGame = config.Mode == GameModeSelection.LocalCoop;
        }


        #endregion


        #region Child ViewModel Event Handlers

        private async Task OnShowEndGameDialog(EndGameState state, Side winner)
        {
            Logger.LogInfo($"Showing end game dialog - State={state}, Winner={winner}");

            if (ShowEndGameDialog != null)
            {
                await ShowEndGameDialog.Invoke(state, winner);
            }
        }


        private Task<PieceType?> OnShowPawnPromotionDialog()
        {
            Logger.LogInfo("Showing pawn promotion dialog");

            if (ShowPawnPromotionDialogRequested != null)
            {
                return ShowPawnPromotionDialogRequested.Invoke();
            }
            return Task.FromResult<PieceType?>(null);
        }


        public void OnButtonClicked()
        {
            ButtonClicked?.Invoke();
        }


        #endregion


        #region Game Actions (delegated to appropriate services/ViewModels)

        public void UndoLastMove()
        {
            Logger.LogInfo("Undo move requested");
            ChessBoardViewModel.UndoLastMove();
        }


        public async Task ResetCurrentGameAsync()
        {
            Logger.LogInfo("Reset game requested");

            // Restart with current configuration, the game service resets the running game first
            if (_gameService.CurrentConfiguration.HasValue)
            {
                await _gameService.StartGameAsync(_gameService.CurrentConfiguration.Value);
            }
        }


        #endregion


        #region Properties

        private bool _isMultiplayerGame;
        public bool IsMultiplayerGame
        {
            get => _isMultiplayerGame;
            set
            {
                if (_isMultiplayerGame != value)
                {
                    _isMultiplayerGame = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isKoopGame;
        public bool IsKoopGame
        {
            get => _isKoopGame;
            set
            {
                if (_isKoopGame != value)
                {
                    _isKoopGame = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(GameModeText));
                }
            }
        }

        public string GameModeText =>
            IsMultiplayerGame ? "Multiplayer" :
            IsKoopGame ? "Local Co-op" : "vs Computer";


        #endregion


        #region INotifyPropertyChanged

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }


        #endregion
    }
}
