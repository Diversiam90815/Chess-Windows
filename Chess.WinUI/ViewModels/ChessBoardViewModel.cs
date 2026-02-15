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
using Chess.UI.Models;


namespace Chess.UI.ViewModels
{
    public partial class ChessBoardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IDispatcherQueueWrapper _dispatcherQueue;
        private readonly IStyleManager _styleManager;
        private readonly IMoveModel _moveModel;
        private readonly IBoardModel _boardModel;
        private readonly IImageService _imageServices;
        private readonly IChessGameService _gameService;
        private readonly ICommunicationLayer _commLayer;

        public event Action ButtonClicked;
        public event Action SquareClicked;
        public event Func<EndGameState, Side, Task> ShowEndGameDialog;
        public event Func<Task<PieceType?>> ShowPawnPromotionDialogRequested;

        public bool IsMultiplayerGame => _gameService.IsMultiplayerGame;

        public ObservableCollection<BoardSquare> Board { get; set; }

        public BoardStyle CurrentBoardStyle;


        public ChessBoardViewModel(IDispatcherQueueWrapper dispatcherQueue, IChessGameService gameService, IStyleManager styleManager, IMoveModel moveModel, IBoardModel boardModel, IImageService imageServices, ICommunicationLayer commLayer)
        {
            _dispatcherQueue = dispatcherQueue;
            _gameService = gameService;
            _styleManager = styleManager;
            _moveModel = moveModel;
            _boardModel = boardModel;
            _imageServices = imageServices;
            _commLayer = commLayer;

            // Subscribe to game service events
            _gameService.GameStarted += OnGameStarted;
            _gameService.GameEnded += OnGameEnded;
            _gameService.GameReset += OnGameReset;

            // Subscribe to move model events
            _moveModel.LegalMovesCalculated += OnHighlightLegalMoves;
            _moveModel.PlayerChanged += OnHandlePlayerChanged;
            _moveModel.GameStateInitSucceeded += OnGameStateInitSucceeded;
            _moveModel.GameOverEvent += OnEndGameState;
            _moveModel.NewBoardFromBackendEvent += OnBoardFromBackendUpdated;
            _moveModel.PawnPromotionEvent += OnPromotionPiece;

            _commLayer.PlayerChanged += OnCurrentSideChanged;

            // Subscribe to style manager events
            _styleManager.PropertyChanged += OnThemeManagerPropertyChanged;

            CurrentBoardStyle = _styleManager.CurrentBoardStyle;

            Board = new ObservableCollection<BoardSquare>();
            InitializeEmptyBoard();
        }


        #region Board initialization and updates

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
            _boardModel.SyncBoardStateFromNative();

            _dispatcherQueue.TryEnqueue(() =>
            {
                // Update each square in the board
                for (int i = 0; i < 64; i++)
                {
                    Square square = (Square)i;
                    int displayIndex = GetDisplayIndex(square);

                    if (displayIndex >= 0 && displayIndex < Board.Count)
                    {
                        Board[displayIndex] = _boardModel.GetSquare(square);
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
            int displayRank = square.Rank();

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


        #endregion


        #region Event Handlers

        private void OnGameStarted()
        {
            Logger.LogInfo("ChessBoardViewModel: Game started");
            InitializeBoardFromNative();
        }


        private void OnGameEnded()
        {
            Logger.LogInfo("ChessBoardViewModel: Game ended");
            ResetHighlightsOnBoard();
        }


        private void OnGameReset()
        {
            Logger.LogInfo("ChessBoardViewModel: Game reset");
            InitializeEmptyBoard();
        }


        private void OnGameStateInitSucceeded()
        {
            Logger.LogInfo("ChessBoardViewModel: Game state initialized");
            InitializeBoardFromNative();
        }


        private void OnBoardFromBackendUpdated()
        {
            Logger.LogInfo("ChessBoardViewModel: Board updated from backend");
            UpdateBoardFromNative();
        }


        private void OnCurrentSideChanged(Side currentSide)
        {
            Logger.LogInfo($"Current side changed to {currentSide}");
            CurrentPlayer = currentSide;
        }


        private void OnHighlightLegalMoves()
        {
            Logger.LogInfo($"ChessBoardViewModel: Highlighting {_moveModel.LegalMoves.Count} legal moves");

            ResetHighlightsOnBoard();

            foreach (var move in _moveModel.LegalMoves)
            {
                Square targetSquare = move.To;
                int displayIndex = GetDisplayIndex(targetSquare);

                if (displayIndex >= 0 && displayIndex < Board.Count)
                {
                    Board[displayIndex].IsHighlighted = true;
                }
            }
        }


        private async void OnPromotionPiece()
        {
            Logger.LogInfo("ChessBoardViewModel: Pawn promotion required");

            var promotionPiece = await RequestPawnPromotionAsync();

            if (promotionPiece.HasValue)
            {
                _moveModel.SetPromotionPieceType(promotionPiece.Value);
            }
            else
            {
                Logger.LogWarning("Pawn promotion cancelled");
                ResetHighlightsOnBoard();
            }
        }


        private void OnHandlePlayerChanged(Side player)
        {
            Logger.LogInfo($"ChessBoardViewModel: Player changed to {player}");
            CurrentPlayer = player;
        }


        private void OnEndGameState(EndGameState state, Side winner)
        {
            Logger.LogInfo($"ChessBoardViewModel: Game ended - State={state}, Winner={winner}");
            ShowEndGameDialog?.Invoke(state, winner);
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
            OnPropertyChanged(nameof(BoardBackgroundImage));
        }


        #endregion


        #region User Actions

        public void HandleSquareClick(BoardSquare square)
        {
            Square engineSquare = square.Position;
            Logger.LogInfo($"Square {engineSquare.ToAlgebraic()} clicked");

            EngineAPI.OnSquareSelected((int)engineSquare);
        }


        public void UndoLastMove()
        {
            Logger.LogInfo("ChessBoardViewModel: Undo last move");

            EngineAPI.UndoMove();
            UpdateBoardFromNative();
        }


        public void ResetHighlightsOnBoard()
        {
            foreach (var square in Board)
            {
                square.IsHighlighted = false;
            }
        }


        private Task<PieceType?> RequestPawnPromotionAsync()
        {
            if (ShowPawnPromotionDialogRequested != null)
            {
                return ShowPawnPromotionDialogRequested.Invoke();
            }
            return Task.FromResult<PieceType?>(null);
        }


        public void OnButtonClicked() => ButtonClicked?.Invoke();
        public void OnSquareClicked() => SquareClicked?.Invoke();


        #endregion


        #region Properties

        private Side _currentPlayer;
        public Side CurrentPlayer
        {
            get => _currentPlayer;
            set
            {
                if (_currentPlayer != value)
                {
                    _currentPlayer = value;
                    OnPropertyChanged();
                }
            }
        }


        public ImageSource BoardBackgroundImage => _imageServices.GetImage(CurrentBoardStyle);


        #endregion


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }
}
