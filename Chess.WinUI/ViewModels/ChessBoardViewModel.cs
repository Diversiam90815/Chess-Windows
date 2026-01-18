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
using Chess.UI.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Chess.UI.Models;


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

        private readonly IImageService _imageServices;

        public BoardStyle CurrentBoardStyle;


        public ChessBoardViewModel(IDispatcherQueueWrapper dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;

            MoveHistoryViewModel = App.Current.Services.GetService<MoveHistoryViewModel>();
            ScoreViewModel = App.Current.Services.GetService<ScoreViewModel>();
            _moveModel = App.Current.Services.GetService<IMoveModel>();
            _boardModel = App.Current.Services.GetService<IBoardModel>();
            _imageServices = App.Current.Services.GetService<IImageService>();
            _styleManager = App.Current.Services.GetService<IStyleManager>();
            MultiplayerViewModel = App.Current.Services.GetService<MultiplayerViewModel>();

            _moveModel.LegalMovesCalculated += OnHighlightLegalMoves;
            _moveModel.PlayerChanged += OnHandlePlayerChanged;
            _moveModel.GameStateInitSucceeded += OnGameStateInitSucceeded;
            _moveModel.GameOverEvent += OnEndGameState;
            _moveModel.NewBoardFromBackendEvent += OnBoardFromBackendUpdated;

            _styleManager.PropertyChanged += OnThemeManagerPropertyChanged;

            _moveModel.PawnPromotionEvent += OnPromotionPiece;

            this.CurrentBoardStyle = _styleManager.CurrentBoardStyle;

            Board = [];

            InitializeEmptyBoard();
        }


        /// <summary>
        /// Initializes an empty 64-square board in display order
        /// </summary>
        private void InitializeEmptyBoard()
        {
            Board.Clear();

            // Create squares in display order (rank 8 to rank 1, files a-h)
            for (int displayRank = 0; displayRank < 8; displayRank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    Square square = SquareExtension.FromDisplayCoordinates(file, displayRank);
                    Board.Add(new BoardSquare(square, PieceType.None));
                }
            }
        }


        public void InitializeBoardFromNative()
        {
            var boardState = _boardModel.GetBoardStateFromNative();
            var allSquares = _boardModel.GetAllSquares();

            _dispatcherQueue.TryEnqueue(() =>
            {
                // Update each square in the board
                for (int i = 0; i < 64; i++)
                {
                    Square square = (Square)i;
                    int displayIndex = GetDisplayIndex(square);

                    if (displayIndex >= 0 && displayIndex < Board.Count)
                    {
                        Board[displayIndex] = allSquares[i];
                    }
                }
                OnPropertyChanged(nameof(Board));
            });
        }


        /// <summary>
        /// Converts a Square to its display index in the Board collection
        /// </summary>
        /// <param name="square">The chess square</param>
        /// <returns>Index in the Board collection (0-63)</returns>
        private int GetDisplayIndex(Square square)
        {
            if (square == Square.None || (int)square < 0 || (int)square >= 64)
                return -1;

            int file = square.File();
            int displayRank = square.DisplayRank();

            // Board is organized as rank 8->1, files a->h
            return displayRank * 8 + file;
        }


        public void UpdateBoardFromNative()
        {
            ResetHighlightsOnBoard();

            var changedSquares = _boardModel.UpdateBoardState();

            _dispatcherQueue.TryEnqueue(() =>
            {
                foreach (var kvp in changedSquares)
                {
                    Square square = kvp.Key;
                    int displayIndex = GetDisplayIndex(square);

                    if (displayIndex >= 0 && displayIndex < Board.Count)
                    {
                        var updatedSquare = _boardModel.GetSquare(square);
                        if (updatedSquare != null)
                        {
                            Board[displayIndex] = updatedSquare;
                        }
                    }
                }
                OnPropertyChanged(nameof(Board));
            });
        }


        /// <summary>
        /// Converts display coordinates (x, y) to Square
        /// </summary>
        private Square GetSquareFromDisplayCoordinates(int x, int y)
        {
            return SquareExtension.FromDisplayCoordinates(x, y);
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

            InitializeEmptyBoard();
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
            Square engineSquare = square.Position;

            Logger.LogInfo($"Square {engineSquare.ToAlgebraic()} (index {(int)engineSquare}) clicked!");

            // Send the square index directly to the engine
            EngineAPI.OnSquareSelected((int)engineSquare);
        }


        public void OnHighlightLegalMoves()
        {
            ResetHighlightsOnBoard();

            foreach (var move in _moveModel.LegalMoves)
            {
                // Assuming LegalMoves now contains Move objects with Square properties
                Square targetSquare = move.To;
                int displayIndex = GetDisplayIndex(targetSquare);

                if (displayIndex >= 0 && displayIndex < Board.Count)
                {
                    Board[displayIndex].IsHighlighted = true;
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


        private Task<PieceType?> RequestPawnPromotionAsync()
        {
            if (ShowPawnPromotionDialogRequested != null)
            {
                return ShowPawnPromotionDialogRequested.Invoke();
            }
            return Task.FromResult<PieceType?>(null);
        }


        private void OnEndGameState(EndGameState state, Side winner)
        {
            ShowEndGameDialog?.Invoke(state, winner);
        }


        private void OnHandlePlayerChanged(Side player)
        {
            CurrentPlayer = player;
        }


        private Side _currentPlayer;
        public Side CurrentPlayer
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
