using Chess.UI.Board;
using Chess.UI.Services;
using Chess.UI.Styles;
using Chess.UI.ViewModels;
using Chess.UI.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Views
{
    public sealed partial class GamePage : Page
    {
        private readonly GameWindowViewModel _viewModel;
        private readonly IImageService _images;
        private readonly IDispatcherQueueWrapper _dispatcher;
        private readonly IStyleManager _styleManager;
        private readonly INavigationService _navigationService;
        private readonly IChessGameService _gameService;

        private PieceType? _selectedPromotionPiece { get; set; }


        public GamePage()
        {
            this.InitializeComponent();

            _dispatcher = App.Current.Services.GetService<IDispatcherQueueWrapper>();
            _images = App.Current.Services.GetService<IImageService>();
            _viewModel = App.Current.Services.GetService<GameWindowViewModel>();
            _styleManager = App.Current.Services.GetService<IStyleManager>();
            _navigationService = App.Current.Services.GetService<INavigationService>();
            _gameService = App.Current.Services.GetService<IChessGameService>();

            RootGrid.DataContext = _viewModel;

            _viewModel.ShowPawnPromotionDialogRequested += OnShowPawnPromotionDialog;
            _viewModel.ShowEndGameDialog += OnGameOverState;
        }


        #region Button Click handlers


        private void UndoMove_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            _viewModel.UndoLastMove();
        }


        private async void ResetGame_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            await _viewModel.ResetCurrentGameAsync();
        }


        private async void EndGame_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.EndGame();
            _viewModel.OnButtonClicked();
            await _gameService.EndGameAsync();
            _navigationService.NavigateToHome();
        }


        private void ChessPiece_Clicked(object sender, TappedRoutedEventArgs e)
        {
            var grid = sender as FrameworkElement;
            var square = grid?.DataContext as BoardSquare;

            if (square != null)
            {
                _viewModel.ChessBoardViewModel.HandleSquareClick(square);
            }
        }


        #endregion


        #region Dialog Handlers

        private async Task OnGameOverState(EndGameState endGameState, Side winner)
        {
            var tcs = new TaskCompletionSource<bool>();

            _dispatcher.TryEnqueue(async () =>
            {
                try
                {
                    switch (endGameState)
                    {
                        case EndGameState.Checkmate:
                            await ShowEnhancedEndGameDialog("Checkmate", winner, endGameState);
                            break;

                        case EndGameState.StaleMate:
                            await ShowEnhancedEndGameDialog("Stalemate", winner, endGameState);
                            break;

                        default:
                            break;
                    }
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            await tcs.Task;
        }


        private async Task ShowEnhancedEndGameDialog(string gameResult, Side winner, EndGameState endGameState)
        {
            var stackPanel = new StackPanel
            {
                Spacing = 16,
                Margin = new Thickness(16)
            };

            // Title
            var titleBlock = new TextBlock
            {
                Text = gameResult,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };
            stackPanel.Children.Add(titleBlock);

            // Winner/Draw message
            if (endGameState == EndGameState.Checkmate && winner != Side.None)
            {
                var winnerBlock = new TextBlock
                {
                    Text = $"Winner: {winner}",
                    FontSize = 18,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = winner == Side.White ?
                        new SolidColorBrush(Colors.DarkBlue) :
                        new SolidColorBrush(Colors.DarkRed),
                    Margin = new Thickness(0, 0, 0, 12)
                };
                stackPanel.Children.Add(winnerBlock);
            }
            else
            {
                var drawBlock = new TextBlock
                {
                    Text = "Game ends in a draw",
                    FontSize = 18,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    Margin = new Thickness(0, 0, 0, 12)
                };
                stackPanel.Children.Add(drawBlock);
            }

            // Game statistics
            AddGameStatistics(stackPanel);

            // Create dialog
            var dialog = new ContentDialog
            {
                Title = $"Game Over - {gameResult}",
                Content = new ScrollViewer
                {
                    Content = stackPanel,
                    MaxHeight = 500,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                },
                PrimaryButtonText = "New Game",
                SecondaryButtonText = "View Board",
                CloseButtonText = "Main Menu",
                XamlRoot = this.XamlRoot,
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();

            await HandleEndGameDialogResult(result);
        }


        private void AddGameStatistics(StackPanel stackPanel)
        {
            var statsHeader = new TextBlock
            {
                Text = "Game Statistics",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 8)
            };
            stackPanel.Children.Add(statsHeader);

            var statsGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                RowSpacing = 8,
                ColumnSpacing = 16
            };

            var totalMoves = _viewModel.MoveHistoryViewModel.MoveEntries
                .Sum(e => (!string.IsNullOrEmpty(e.WhiteMove) ? 1 : 0) +
                          (!string.IsNullOrEmpty(e.BlackMove) ? 1 : 0));

            AddStatRow(statsGrid, 0, "Total Moves:", totalMoves.ToString());

            var whiteCaptured = _viewModel.CapturedPiecesViewModel.WhiteCapturedPawn +
                               _viewModel.CapturedPiecesViewModel.WhiteCapturedBishop +
                               _viewModel.CapturedPiecesViewModel.WhiteCapturedKnight +
                               _viewModel.CapturedPiecesViewModel.WhiteCapturedRook +
                               _viewModel.CapturedPiecesViewModel.WhiteCapturedQueen;

            var blackCaptured = _viewModel.CapturedPiecesViewModel.BlackCapturedPawn +
                               _viewModel.CapturedPiecesViewModel.BlackCapturedBishop +
                               _viewModel.CapturedPiecesViewModel.BlackCapturedKnight +
                               _viewModel.CapturedPiecesViewModel.BlackCapturedRook +
                               _viewModel.CapturedPiecesViewModel.BlackCapturedQueen;

            AddStatRow(statsGrid, 3, "White Captured:", $"{whiteCaptured} pieces");
            AddStatRow(statsGrid, 4, "Black Captured:", $"{blackCaptured} pieces");

            var gameType = _viewModel.IsMultiplayerGame ? "Multiplayer" :
                          _viewModel.IsKoopGame ? "Local Co-op" : "Single Player";
            AddStatRow(statsGrid, 5, "Game Type:", gameType);

            stackPanel.Children.Add(statsGrid);
        }


        private void AddStatRow(Grid grid, int row, string label, string value)
        {
            while (grid.RowDefinitions.Count <= row)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            var labelBlock = new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(labelBlock, row);
            Grid.SetColumn(labelBlock, 0);
            grid.Children.Add(labelBlock);

            var valueBlock = new TextBlock
            {
                Text = value,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(valueBlock, row);
            Grid.SetColumn(valueBlock, 1);
            grid.Children.Add(valueBlock);
        }


        private void AddPromotionButton(Panel panel, PieceType pieceType, Side playerSide, ContentDialog dialog)
        {
            var button = new Button
            {
                Tag = pieceType,
                Width = 80,
                Height = 80,
                Padding = new Thickness(0)
            };

            var image = new Image
            {
                Source = _images.GetPieceImage(_styleManager.CurrentPieceStyle, pieceType),
                Stretch = Stretch.Uniform
            };

            button.Content = image;

            button.Click += (s, e) =>
            {
                _selectedPromotionPiece = pieceType;
                dialog.Hide();
            };

            panel.Children.Add(button);
        }


        private async Task<PieceType?> OnShowPawnPromotionDialog()
        {
            var tcs = new TaskCompletionSource<PieceType?>();

            _dispatcher.TryEnqueue(async () =>
            {
                try
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Pawn Promotion",
                        XamlRoot = this.XamlRoot
                    };

                    var stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Spacing = 10
                    };

                    Side currentPlayer = _viewModel.ChessBoardViewModel.CurrentPlayer;
                    _selectedPromotionPiece = null;

                    AddPromotionButton(stackPanel, PieceType.WQueen, currentPlayer, dialog);
                    AddPromotionButton(stackPanel, PieceType.WRook, currentPlayer, dialog);
                    AddPromotionButton(stackPanel, PieceType.WBishop, currentPlayer, dialog);
                    AddPromotionButton(stackPanel, PieceType.WKnight, currentPlayer, dialog);

                    dialog.Content = stackPanel;

                    await dialog.ShowAsync();

                    tcs.SetResult(_selectedPromotionPiece);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error showing pawn promotion dialog: {ex.Message}");
                    tcs.SetException(ex);
                }
            });

            return await tcs.Task;
        }


        private async Task HandleEndGameDialogResult(ContentDialogResult result)
        {
            switch (result)
            {
                case ContentDialogResult.Primary: // New Game
                    await _viewModel.ResetCurrentGameAsync();
                    break;

                case ContentDialogResult.Secondary: // View Board
                    // Do nothing - just close dialog
                    break;

                case ContentDialogResult.None: // Main Menu
                default:
                    await _gameService.EndGameAsync();
                    _navigationService.NavigateToHome();
                    break;
            }
        }


        #endregion
    }
}
