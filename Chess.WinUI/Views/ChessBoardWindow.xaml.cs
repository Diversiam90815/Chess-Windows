using Chess.UI.Board;
using Chess.UI.Services;
using Chess.UI.Styles;
using Chess.UI.ViewModels;
using Chess.UI.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Text;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Views
{
    public sealed partial class ChessBoardWindow : Window
    {
        private readonly ChessBoardViewModel _viewModel;

        private PieceTypeInstance? ViewModelSelectedPiece { get; set; }

        private readonly IImageService _images;

        private readonly IDispatcherQueueWrapper _dispatcher;

        private readonly IWindowSizeService _windowSizeService;


        public ChessBoardWindow()
        {
            this.InitializeComponent();
            AppWindow.SetIcon(Project.IconPath);

            _dispatcher = App.Current.Services.GetService<IDispatcherQueueWrapper>();
            _images = App.Current.Services.GetService<IImageService>();
            _viewModel = App.Current.Services.GetService<ChessBoardViewModel>();
            _windowSizeService = App.Current.Services.GetService<IWindowSizeService>();

            RootPanel.DataContext = _viewModel;

            _viewModel.ShowPawnPromotionDialogRequested += OnShowPawnPromotionPieces;
            _viewModel.ShowEndGameDialog += OnGameOverState;

            _windowSizeService.SetWindowSize(this, 1100, 800);
            _windowSizeService.SetWindowNonResizable(this);
        }


        private void UndoMove_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();

            _viewModel.UndoLastMove();
        }


        private void ResetGame_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();

            _viewModel.ResetGame();
            _viewModel.StartGame(new GameConfiguration());
        }


        private void EndGame_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            this.Close();
        }


        private void ChessPiece_Clicked(object sender, TappedRoutedEventArgs e)
        {
            _viewModel.OnSquareClicked();

            var grid = sender as FrameworkElement;
            var square = grid.DataContext as BoardSquare;

            // Handle the move
            _viewModel.HandleSquareClick(square);
        }


        private async Task OnGameOverState(EndGameState endGameState, PlayerColor winner)
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


        private async Task ShowEnhancedEndGameDialog(string gameResult, PlayerColor winner, EndGameState endGameState)
        {
            // Create enhanced dialog content
            var stackPanel = new StackPanel
            {
                Spacing = 16,
                Margin = new Thickness(16)
            };

            // Title section
            var titleBlock = new TextBlock
            {
                Text = gameResult,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };
            stackPanel.Children.Add(titleBlock);

            // Winner section (only for checkmate)
            if (endGameState == EndGameState.Checkmate && winner != PlayerColor.NoColor)
            {
                var winnerBlock = new TextBlock
                {
                    Text = $"🏆 Winner: {winner}",
                    FontSize = 18,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = winner == PlayerColor.White ?
                        new SolidColorBrush(Colors.DarkBlue) :
                        new SolidColorBrush(Colors.DarkRed),
                    Margin = new Thickness(0, 0, 0, 12)
                };
                stackPanel.Children.Add(winnerBlock);
            }
            else if (endGameState == EndGameState.StaleMate)
            {
                var drawBlock = new TextBlock
                {
                    Text = "🤝 Game ends in a draw",
                    FontSize = 18,
                    FontWeight = FontWeights.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    Margin = new Thickness(0, 0, 0, 12)
                };
                stackPanel.Children.Add(drawBlock);
            }

            // Game statistics section
            var statsHeader = new TextBlock
            {
                Text = "📊 Game Statistics",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 8)
            };
            stackPanel.Children.Add(statsHeader);

            // Statistics grid
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

            // Move count
            var totalMoves = _viewModel.MoveHistoryViewModel.MoveHistoryColumns
                .SelectMany(col => col)
                .Count();

            AddStatRow(statsGrid, 0, "Total Moves:", totalMoves.ToString());

            // Score information
            AddStatRow(statsGrid, 1, "White Score:", _viewModel.ScoreViewModel.WhiteScoreValue.ToString());
            AddStatRow(statsGrid, 2, "Black Score:", _viewModel.ScoreViewModel.BlackScoreValue.ToString());

            // Captured pieces summary
            var whiteCaptured = _viewModel.ScoreViewModel.WhiteCapturedPawn +
                               _viewModel.ScoreViewModel.WhiteCapturedBishop +
                               _viewModel.ScoreViewModel.WhiteCapturedKnight +
                               _viewModel.ScoreViewModel.WhiteCapturedRook +
                               _viewModel.ScoreViewModel.WhiteCapturedQueen;

            var blackCaptured = _viewModel.ScoreViewModel.BlackCapturedPawn +
                               _viewModel.ScoreViewModel.BlackCapturedBishop +
                               _viewModel.ScoreViewModel.BlackCapturedKnight +
                               _viewModel.ScoreViewModel.BlackCapturedRook +
                               _viewModel.ScoreViewModel.BlackCapturedQueen;

            AddStatRow(statsGrid, 3, "White Captured:", whiteCaptured.ToString() + " pieces");
            AddStatRow(statsGrid, 4, "Black Captured:", blackCaptured.ToString() + " pieces");

            // Game type
            var gameType = _viewModel.IsMultiplayerGame ? "Multiplayer" : "Single Player";
            AddStatRow(statsGrid, 5, "Game Type:", gameType);

            stackPanel.Children.Add(statsGrid);

            // Captured pieces details (if any pieces were captured)
            if (whiteCaptured > 0 || blackCaptured > 0)
            {
                var capturedHeader = new TextBlock
                {
                    Text = "🎯 Captured Pieces",
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 16, 0, 8)
                };
                stackPanel.Children.Add(capturedHeader);

                var capturedGrid = CreateCapturedPieceDisplay();
                stackPanel.Children.Add(capturedGrid);
            }

            // Create the dialog
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
                XamlRoot = this.Content.XamlRoot,
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();

            switch (result)
            {
                case ContentDialogResult.Primary: // New Game
                    _viewModel.ResetGame();
                    _viewModel.StartGame(new GameConfiguration());
                    break;
                case ContentDialogResult.Secondary: // View Board
                    // Do nothing - just close dialog and let user examine the board
                    break;
                case ContentDialogResult.None: // Main Menu
                default:
                    _viewModel.ResetGame();
                    this.Close();
                    break;
            }
        }


        private void AddStatRow(Grid grid, int row, string label, string value)
            
        {
            // Ensure we have enough row definitions
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


        private Grid CreateCapturedPieceDisplay()
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                RowSpacing = 12,
                ColumnSpacing = 16
            };

            // White captured pieces
            var whitePanel = CreatePlayerCapturedPanel("White Captured:", PlayerColor.White);
            Grid.SetRow(whitePanel, 0);
            Grid.SetColumn(whitePanel, 0);
            grid.Children.Add(whitePanel);

            // Black captured pieces
            var blackPanel = CreatePlayerCapturedPanel("Black Captured:", PlayerColor.Black);
            Grid.SetRow(blackPanel, 0);
            Grid.SetColumn(blackPanel, 1);
            grid.Children.Add(blackPanel);

            return grid;
        }


        private StackPanel CreatePlayerCapturedPanel(string title, PlayerColor player)
        {
            var panel = new StackPanel
            {
                Spacing = 4
            };

            var titleBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14
            };
            panel.Children.Add(titleBlock);

            var piecesPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8
            };

            // Add captured piece counts
            var pieces = new[]
            {
                (PieceTypeInstance.Pawn, player == PlayerColor.White ? _viewModel.ScoreViewModel.WhiteCapturedPawn : _viewModel.ScoreViewModel.BlackCapturedPawn, "♟"),
                (PieceTypeInstance.Knight, player == PlayerColor.White ? _viewModel.ScoreViewModel.WhiteCapturedKnight : _viewModel.ScoreViewModel.BlackCapturedKnight, "♞"),
                (PieceTypeInstance.Bishop, player == PlayerColor.White ? _viewModel.ScoreViewModel.WhiteCapturedBishop : _viewModel.ScoreViewModel.BlackCapturedBishop, "♝"),
                (PieceTypeInstance.Rook, player == PlayerColor.White ? _viewModel.ScoreViewModel.WhiteCapturedRook : _viewModel.ScoreViewModel.BlackCapturedRook, "♜"),
                (PieceTypeInstance.Queen, player == PlayerColor.White ? _viewModel.ScoreViewModel.WhiteCapturedQueen : _viewModel.ScoreViewModel.BlackCapturedQueen, "♛")
            };

            foreach (var (pieceType, count, symbol) in pieces)
            {
                if (count > 0)
                {
                    var pieceBlock = new TextBlock
                    {
                        Text = $"{symbol}×{count}",
                        FontSize = 12,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    piecesPanel.Children.Add(pieceBlock);
                }
            }

            if (piecesPanel.Children.Count == 0)
            {
                var noneBlock = new TextBlock
                {
                    Text = "None",
                    FontStyle = FontStyle.Italic,
                    Foreground = new SolidColorBrush(Colors.Gray),
                    FontSize = 12
                };
                piecesPanel.Children.Add(noneBlock);
            }

            panel.Children.Add(piecesPanel);
            return panel;
        }


        private async Task<PieceTypeInstance?> OnShowPawnPromotionPieces()
        {
            TaskCompletionSource<PieceTypeInstance?> tcs = new();

            DispatcherQueue.TryEnqueue(() =>
            {
                var dialog = new ContentDialog
                {
                    Title = "Pawn Promotion",
                    XamlRoot = this.Content.XamlRoot
                };

                // Customize the dialog to include all four options
                // Since ContentDialog has limited buttons, we'll use a custom Content
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                PlayerColor currentPlayer = _viewModel.CurrentPlayer;

                // Helper method to create a button with an image
                Button CreatePieceButton(PieceTypeInstance pieceType)
                {
                    var button = new Button
                    {
                        Tag = pieceType,
                        Margin = new Thickness(5),
                        Padding = new Thickness(0),
                        Width = 80,
                        Height = 80
                    };

                    var image = new Image
                    {
                        Source = _images.GetPieceImage(PieceStyle.Basic, currentPlayer, pieceType),       // Need to adapt to current theme!
                        Stretch = Stretch.Uniform
                    };

                    button.Content = image;

                    button.Click += (s, e) =>
                    {
                        dialog.Hide();
                        ViewModelSelectedPiece = pieceType;
                    };

                    return button;
                }

                // Create buttons for each promotion option
                var queenButton = CreatePieceButton(PieceTypeInstance.Queen);
                var rookButton = CreatePieceButton(PieceTypeInstance.Rook);
                var bishopButton = CreatePieceButton(PieceTypeInstance.Bishop);
                var knightButton = CreatePieceButton(PieceTypeInstance.Knight);

                stackPanel.Children.Add(queenButton);
                stackPanel.Children.Add(rookButton);
                stackPanel.Children.Add(bishopButton);
                stackPanel.Children.Add(knightButton);

                dialog.Content = stackPanel;

                ViewModelSelectedPiece = null;

                // Show the dialog and wait for it to close
                _ = dialog.ShowAsync().AsTask().ContinueWith(t => { tcs.SetResult(ViewModelSelectedPiece); });
            });

            return await tcs.Task;
        }

    }
}
