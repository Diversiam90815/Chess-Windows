using Chess.UI.Services;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Chess.UI.Services.EngineAPI;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;
using Chess.UI.Styles;
using Chess.UI.Board;
using Chess.UI.Images;
using Chess.UI.Wrappers;
using Chess.UI.Coordinates;
using Microsoft.Extensions.DependencyInjection;
using Chess.UI.Moves;


namespace Chess.UI.ViewModels
{
    public partial class ChessBoardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IDispatcherQueueWrapper _dispatcherQueue;

        public event Action ButtonClicked;
        public event Action SquareClicked;

        public ObservableCollection<BoardSquare> Board { get; set; }

        public event Func<EndGameState, Side, Task> ShowEndGameDialog;

        public event Func<Task<PieceType?>> ShowPawnPromotionDialogRequested;

        public ScoreViewModel ScoreViewModel { get; }

        public MoveHistoryViewModel MoveHistoryViewModel { get; set; }

        public MultiplayerViewModel MultiplayerViewModel { get; }

        private readonly IStyleManager _styleManager;

        private readonly IMoveModel _moveModel;

        private readonly IBoardModel _boardModel;

        private readonly IChessCoordinate _coordinate;

        private readonly IImageService _imageServices;

        public BoardStyle CurrentBoardStyle;


        public ChessBoardViewModel(IDispatcherQueueWrapper dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;

            MoveHistoryViewModel = App.Current.Services.GetService<MoveHistoryViewModel>();
            ScoreViewModel = App.Current.Services.GetService<ScoreViewModel>();
            _moveModel = App.Current.Services.GetService<IMoveModel>();
            _boardModel = App.Current.Services.GetService<IBoardModel>();
            _coordinate = App.Current.Services.GetService<IChessCoordinate>();
            _imageServices = App.Current.Services.GetService<IImageService>();
            _styleManager = App.Current.Services.GetService<IStyleManager>();
            MultiplayerViewModel = App.Current.Services.GetService<MultiplayerViewModel>();

            _moveModel.PossibleMovesCalculated += OnHighlightPossibleMoves;
            _moveModel.PlayerChanged += OnHandlePlayerChanged;
            _moveModel.GameStateInitSucceeded += OnGameStateInitSucceeded;
            _moveModel.GameOverEvent += OnEndGameState;
            _moveModel.NewBoardFromBackendEvent += OnBoardFromBackendUpdated;

            _styleManager.PropertyChanged += OnThemeManagerPropertyChanged;

            _moveModel.PawnPromotionEvent += OnPromotionPiece;

            this.CurrentBoardStyle = _styleManager.CurrentBoardStyle;

            Board = [];

            for (int i = 0; i < _coordinate.GetNumBoardSquares(); i++)
            {
                Board.Add(new());
            }
        }


        public void InitializeBoardFromNative()
        {
            var boardState = _boardModel.GetBoardStateFromNative();

            for (int i = 0; i < _coordinate.GetNumBoardSquares(); i++)
            {
                BoardSquare square = _boardModel.DecodeBoardState(i, boardState);

                // Calculate the index in the UI board array
                int displayIndex = _coordinate.ToIndex(square.pos, true);

                _dispatcherQueue.TryEnqueue(() =>
                {
                    Board[displayIndex] = square;
                    OnPropertyChanged(nameof(Board));
                });
            }
        }


        public void UpdateBoardFromNative()
        {
            ResetHighlightsOnBoard();

            var (changedSquares, newBoardState) = _boardModel.UpdateBoardState();

            foreach (var kvp in changedSquares)
            {
                int boardIndex = kvp.Key;      // The index on the chess board
                int encodedState = kvp.Value;  // The encoded state value

                BoardSquare square = _boardModel.DecodeBoardState(boardIndex, newBoardState);

                // Calculate the index in the UI board array
                int displayIndex = _coordinate.ToIndex(square.pos, true);

                _dispatcherQueue.TryEnqueue(() =>
                {
                    Board[displayIndex] = square;
                    OnPropertyChanged(nameof(Board));
                });
            }
        }


        private void OnBoardFromBackendUpdated()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                UpdateBoardFromNative();
            });
        }


        public void ResetGame()
        {
            EngineAPI.ResetGame();

            Board.Clear();
            for (int i = 0; i < _coordinate.GetNumBoardSquares(); i++)
            {
                Board.Add(new BoardSquare());
            }
            ScoreViewModel.ReinitScoreValues();
        }


        public void StartGame(GameConfiguration config)
        {
            EngineAPI.StartGame(config);  // Start the game and thus the StateMachine
        }


        public void OnGameStateInitSucceeded()
        {
            // Once the board is ready calculated, we load it from native
            InitializeBoardFromNative();
        }


        private void OnThemeManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StyleManager.CurrentBoardStyle))
            {
                UpdateBoardTheme(_styleManager.CurrentBoardStyle);
            }
        }


        private void UpdateBoardTheme(BoardStyle boardTheme)
        {
            CurrentBoardStyle = boardTheme;
        }


        private async void OnPromotionPiece()
        {
            var promotionPiece = await RequestPawnPromotionAsync();
            if (promotionPiece.HasValue)
            {
                _moveModel.SetPromotionPieceType(promotionPiece.Value);
            }
            else
            {
                // Pawn Promotion has been cancelled
                ResetHighlightsOnBoard();
            }
        }


        public void HandleSquareClick(BoardSquare square)
        {
            PositionInstance enginePos = _coordinate.FromDisplayCoordinates(square.pos);
            Logger.LogInfo($"Square (UI) X{square.pos.x}-Y{square.pos.y} clicked => (Engine) X{enginePos.x}-Y{enginePos.y}!");

            EngineAPI.OnSquareSelected(enginePos);
        }


        public void OnHighlightPossibleMoves()
        {
            ResetHighlightsOnBoard();

            foreach (var pm in _moveModel.PossibleMoves)
            {
                PositionInstance displayPos = _coordinate.ToDisplayCoordinates(pm.end);

                int index = _coordinate.ToIndex(displayPos, true);
                BoardSquare square = Board[index];

                if (square != null)
                {
                    square.IsHighlighted = true;
                }
            }
        }


        public void OnButtonClicked()
        {
            ButtonClicked?.Invoke();
        }


        public void OnSquareClicked()
        {
            SquareClicked?.Invoke();
        }


        public void ResetHighlightsOnBoard()
        {
            foreach (var square in Board)
            {
                square.IsHighlighted = false;
            }
        }


        public void UndoLastMove()
        {
            EngineAPI.UndoMove();
            UpdateBoardFromNative();
            MoveHistoryViewModel.RemoveLastMove();
        }


        private Task<PieceTypeInstance?> RequestPawnPromotionAsync()
        {
            if (ShowPawnPromotionDialogRequested != null)
            {
                return ShowPawnPromotionDialogRequested.Invoke();
            }
            return Task.FromResult<PieceTypeInstance?>(null);
        }


        private void OnEndGameState(EndGameState state, PlayerColor winner)
        {
            ShowEndGameDialog?.Invoke(state, winner);
        }


        private void OnHandlePlayerChanged(PlayerColor player)
        {
            CurrentPlayer = player;
        }


        private PlayerColor _currentPlayer;
        public PlayerColor CurrentPlayer
        {
            get => _currentPlayer;
            set
            {
                if (value != _currentPlayer)
                {
                    _currentPlayer = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isMultiplayerGame;
        public bool IsMultiplayerGame
        {
            get => _isMultiplayerGame;
            set
            {
                if (value != _isMultiplayerGame)
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
                if (value != _isKoopGame)
                {
                    _isKoopGame = value;
                    OnPropertyChanged();
                }
            }
        }


        public ImageSource BoardBackgroundImage
        {
            get
            {
                return _imageServices.GetImage(CurrentBoardStyle);
            }
        }


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }
}
