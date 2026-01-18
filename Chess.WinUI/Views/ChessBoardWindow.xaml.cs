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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Text;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Views
{
    public sealed partial class ChessBoardWindow : Window
    {
        private readonly GameWindowViewModel _viewModel;
        private readonly IImageService _images;
        private readonly IDispatcherQueueWrapper _dispatcher;
        private readonly IWindowSizeService _windowSizeService;
        private readonly IStyleManager _styleManager;

        private PieceType? _selectedPromotionPiece { get; set; }


        public ChessBoardWindow()
        {
            this.InitializeComponent();
            AppWindow.SetIcon(Project.IconPath);

            _dispatcher = App.Current.Services.GetService<IDispatcherQueueWrapper>();
            _images = App.Current.Services.GetService<IImageService>();
            _viewModel = App.Current.Services.GetService<GameWindowViewModel>();
            _windowSizeService = App.Current.Services.GetService<IWindowSizeService>();
            _styleManager = App.Current.Services.GetService<IStyleManager>();

            RootPanel.DataContext = _viewModel;

            _viewModel.ShowPawnPromotionDialogRequested += OnShowPawnPromotionDialog;
            _viewModel.ShowEndGameDialog += OnGameOverState;

            _windowSizeService.SetWindowSize(this, 1100, 800);
            _windowSizeService.SetWindowNonResizable(this);
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


        private void EndGame_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            this.Close();
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
                    Text = $"🏆 Winner: {winner}",
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
                    Text = "🤝 Game ends in a draw",
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
                XamlRoot = this.Content.XamlRoot,
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();

            await HandleEndGameDialogResult(result);
        }


        private void AddGameStatistics(StackPanel stackPanel)
        {
            var statsHeader = new TextBlock
            {
                Text = "📊 Game Statistics",
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

            // Add statistics
            var totalMoves = _viewModel.MoveHistoryViewModel.MoveHistoryColumns
                .SelectMany(col => col)
                .Count();

            AddStatRow(statsGrid, 0, "Total Moves:", totalMoves.ToString());

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

            AddStatRow(statsGrid, 3, "White Captured:", $"{whiteCaptured} pieces");
            AddStatRow(statsGrid, 4, "Black Captured:", $"{blackCaptured} pieces");

            var gameType = _viewModel.IsMultiplayerGame ? "Multiplayer" :
                          _viewModel.IsKoopGame ? "Local Co-op" : "Single Player";
            AddStatRow(statsGrid, 5, "Game Type:", gameType);

            stackPanel.Children.Add(statsGrid);

            // Captured pieces details
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
            var whitePanel = CreatePlayerCapturedPanel("White Captured:", Side.White);
            Grid.SetRow(whitePanel, 0);
            Grid.SetColumn(whitePanel, 0);
            grid.Children.Add(whitePanel);

            // Black captured pieces
            var blackPanel = CreatePlayerCapturedPanel("Black Captured:", Side.Black);
            Grid.SetRow(blackPanel, 0);
            Grid.SetColumn(blackPanel, 1);
            grid.Children.Add(blackPanel);

            return grid;
        }


        private StackPanel CreatePlayerCapturedPanel(string title, Side player)
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
                ("♟", player == Side.White ? _viewModel.ScoreViewModel.WhiteCapturedPawn : _viewModel.ScoreViewModel.BlackCapturedPawn),
                ("♞", player == Side.White ? _viewModel.ScoreViewModel.WhiteCapturedKnight : _viewModel.ScoreViewModel.BlackCapturedKnight),
                ("♝", player == Side.White ? _viewModel.ScoreViewModel.WhiteCapturedBishop : _viewModel.ScoreViewModel.BlackCapturedBishop),
                ("♜", player == Side.White ? _viewModel.ScoreViewModel.WhiteCapturedRook : _viewModel.ScoreViewModel.BlackCapturedRook),
                ("♛", player == Side.White ? _viewModel.ScoreViewModel.WhiteCapturedQueen : _viewModel.ScoreViewModel.BlackCapturedQueen)
            };

            foreach (var (symbol, count) in pieces)
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
                        XamlRoot = this.Content.XamlRoot
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

                    // Create promotion piece buttons
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
                    this.Close();
                    break;
            }
        }


        #endregion
    }
}
