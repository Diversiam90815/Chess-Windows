using Chess.UI.Board;
using Chess.UI.Models;
using Chess.UI.Services;
using Chess.UI.Settings;
using Chess.UI.Styles;
using Chess.UI.Test.Helpers;
using Chess.UI.ViewModels;
using Microsoft.UI.Xaml;
using Moq;
using System.ComponentModel;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Test
{
    public class CapturedPiecesViewModelTests
    {
        private readonly Mock<ICapturedPiecesModel> _model = new();
        private readonly Dictionary<PieceType, int> _counts = new();
        private readonly CapturedPiecesViewModel _viewModel;


        public CapturedPiecesViewModelTests()
        {
            foreach (PieceType piece in Enum.GetValues<PieceType>())
                _counts[piece] = 0;

            _model.SetupGet(model => model.CapturedPieces).Returns(_counts);
            _viewModel = new CapturedPiecesViewModel(new InlineDispatcherQueue(), _model.Object, Mock.Of<IImageService>());
        }


        [Fact]
        public void BlackPieces_CountForWhite_AndWhitePiecesForBlack()
        {
            _counts[PieceType.BPawn] = 3;
            _counts[PieceType.BBishop] = 1;
            _counts[PieceType.BKnight] = 2;
            _counts[PieceType.BRook] = 1;
            _counts[PieceType.BQueen] = 1;
            _counts[PieceType.WPawn] = 4;
            _counts[PieceType.WQueen] = 1;

            Assert.Equal(3, _viewModel.WhiteCapturedPawn);
            Assert.Equal(1, _viewModel.WhiteCapturedBishop);
            Assert.Equal(2, _viewModel.WhiteCapturedKnight);
            Assert.Equal(1, _viewModel.WhiteCapturedRook);
            Assert.Equal(1, _viewModel.WhiteCapturedQueen);
            Assert.Equal(4, _viewModel.BlackCapturedPawn);
            Assert.Equal(0, _viewModel.BlackCapturedBishop);
            Assert.Equal(0, _viewModel.BlackCapturedKnight);
            Assert.Equal(0, _viewModel.BlackCapturedRook);
            Assert.Equal(1, _viewModel.BlackCapturedQueen);
        }


        [Theory]
        [InlineData(PieceType.BPawn, nameof(CapturedPiecesViewModel.WhiteCapturedPawn))]
        [InlineData(PieceType.BQueen, nameof(CapturedPiecesViewModel.WhiteCapturedQueen))]
        [InlineData(PieceType.WKnight, nameof(CapturedPiecesViewModel.BlackCapturedKnight))]
        [InlineData(PieceType.WRook, nameof(CapturedPiecesViewModel.BlackCapturedRook))]
        public void Capture_RefreshesTheCountOfTheCapturingSide(PieceType capturedPiece, string expectedProperty)
        {
            List<string?> changedProperties = TestData.RecordPropertyChanges(_viewModel);
            Side capturingSide = capturedPiece >= PieceType.BKing ? Side.White : Side.Black;

            _model.Raise(model => model.PieceCaptured += null, TestData.Capture(capturingSide, capturedPiece));

            Assert.Equal(new[] { expectedProperty }, changedProperties);
        }


        [Fact]
        public void ClearCaptures_ClearsTheModelAndRefreshesEveryCount()
        {
            List<string?> changedProperties = TestData.RecordPropertyChanges(_viewModel);

            _viewModel.ClearCaptures();

            _model.Verify(model => model.Clear(), Times.Once);
            Assert.Equal(10, changedProperties.Distinct().Count());
        }
    }


    public class MoveHistoryViewModelTests
    {
        private readonly Mock<IMoveHistoryModel> _model = new();
        private readonly List<string> _moves = new();
        private readonly MoveHistoryViewModel _viewModel;


        public MoveHistoryViewModelTests()
        {
            _model.SetupGet(model => model.MoveHistory).Returns(_moves);
            _viewModel = new MoveHistoryViewModel(new InlineDispatcherQueue(), _model.Object);
        }


        private void ModelReportsMoves(params string[] moves)
        {
            _moves.Clear();
            _moves.AddRange(moves);
            _model.Raise(model => model.MoveHistoryUpdated += null);
        }


        [Fact]
        public void Moves_ArePairedIntoNumberedRows()
        {
            ModelReportsMoves("e4", "e5", "Nf3", "Nc6");

            Assert.Equal(2, _viewModel.MoveEntries.Count);
            Assert.Equal((1, "e4", "e5"), (_viewModel.MoveEntries[0].MoveNumber, _viewModel.MoveEntries[0].WhiteMove, _viewModel.MoveEntries[0].BlackMove));
            Assert.Equal((2, "Nf3", "Nc6"), (_viewModel.MoveEntries[1].MoveNumber, _viewModel.MoveEntries[1].WhiteMove, _viewModel.MoveEntries[1].BlackMove));
        }


        [Fact]
        public void WhiteMoveWithoutReply_LeavesTheBlackColumnEmpty()
        {
            ModelReportsMoves("e4", "e5", "Nf3");

            Assert.Equal(2, _viewModel.MoveEntries.Count);
            Assert.Equal("Nf3", _viewModel.MoveEntries[1].WhiteMove);
            Assert.Equal(string.Empty, _viewModel.MoveEntries[1].BlackMove);
        }


        [Fact]
        public void UndoneMove_DisappearsFromTheRows()
        {
            ModelReportsMoves("e4", "e5", "Nf3");
            ModelReportsMoves("e4", "e5");

            Assert.Single(_viewModel.MoveEntries);
        }


        [Fact]
        public void OnReset_ClearsModelAndRows()
        {
            ModelReportsMoves("e4");

            _viewModel.OnReset();

            _model.Verify(model => model.ClearHistory(), Times.Once);
            Assert.Empty(_viewModel.MoveEntries);
        }


        [Fact]
        public void Entry_ReportsChangedMoves()
        {
            var entry = new MoveHistoryEntry { MoveNumber = 1 };
            List<string?> changedProperties = TestData.RecordPropertyChanges(entry);

            entry.WhiteMove = "d4";
            entry.BlackMove = "d5";
            entry.BlackMove = "d5";

            Assert.Equal(new[] { nameof(MoveHistoryEntry.WhiteMove), nameof(MoveHistoryEntry.BlackMove) }, changedProperties);
        }
    }


    public class ChessBoardViewModelTests
    {
        private readonly Mock<IChessGameService> _gameService = new();
        private readonly Mock<IStyleManager> _styleManager = new();
        private readonly Mock<IMoveModel> _moveModel = new();
        private readonly Mock<IBoardModel> _boardModel = new();
        private readonly Mock<IImageService> _imageService = new();
        private readonly Mock<ICommunicationLayer> _communication = new();
        private readonly List<Move> _legalMoves = new();
        private readonly ChessBoardViewModel _viewModel;


        public ChessBoardViewModelTests()
        {
            _styleManager.SetupGet(manager => manager.CurrentBoardStyle).Returns(BoardStyle.Wood);
            _moveModel.SetupGet(model => model.LegalMoves).Returns(_legalMoves);

            _viewModel = new ChessBoardViewModel(new InlineDispatcherQueue(), _gameService.Object, _styleManager.Object,
                _moveModel.Object, _boardModel.Object, _imageService.Object, _communication.Object);
        }


        private BoardSquare SquareAt(Square square) => _viewModel.Board[(int)square];


        [Fact]
        public void NewBoard_ShowsSixtyFourEmptySquaresFromA8ToH1()
        {
            Assert.Equal(64, _viewModel.Board.Count);
            Assert.Equal(Square.a8, _viewModel.Board[0].Position);
            Assert.Equal(Square.h8, _viewModel.Board[7].Position);
            Assert.Equal(Square.h1, _viewModel.Board[63].Position);
            Assert.All(_viewModel.Board, square => Assert.Equal(PieceType.None, square.Piece));
        }


        [Fact]
        public void LegalMoves_HighlightTheirTargetSquares()
        {
            _legalMoves.Add(new Move(Square.e2, Square.e3));
            _legalMoves.Add(new Move(Square.e2, Square.e4, MoveFlag.DoublePawnPush));

            _moveModel.Raise(model => model.LegalMovesCalculated += null);

            Assert.True(SquareAt(Square.e3).IsHighlighted);
            Assert.True(SquareAt(Square.e4).IsHighlighted);
            Assert.Equal(2, _viewModel.Board.Count(square => square.IsHighlighted));
        }


        [Fact]
        public void NewSelection_ReplacesTheOldHighlights()
        {
            _legalMoves.Add(new Move(Square.e2, Square.e4, MoveFlag.DoublePawnPush));
            _moveModel.Raise(model => model.LegalMovesCalculated += null);

            _legalMoves.Clear();
            _legalMoves.Add(new Move(Square.g1, Square.f3));
            _moveModel.Raise(model => model.LegalMovesCalculated += null);

            Assert.False(SquareAt(Square.e4).IsHighlighted);
            Assert.True(SquareAt(Square.f3).IsHighlighted);
        }


        [Fact]
        public void BoardFromEngine_ReplacesChangedSquaresAndClearsHighlights()
        {
            var board = new PieceType[64];
            var movedPawn = new BoardSquare(Square.e4, PieceType.WPawn);
            _boardModel.Setup(model => model.UpdateBoardState(board)).Returns(new Dictionary<Square, PieceType> { [Square.e4] = PieceType.WPawn });
            _boardModel.Setup(model => model.GetSquare(Square.e4)).Returns(movedPawn);

            SquareAt(Square.e3).IsHighlighted = true;
            BoardSquare untouched = SquareAt(Square.a1);

            _moveModel.Raise(model => model.NewBoardFromBackendEvent += null, board);

            Assert.Same(movedPawn, SquareAt(Square.e4));
            Assert.Same(untouched, SquareAt(Square.a1));
            Assert.DoesNotContain(_viewModel.Board, square => square.IsHighlighted);
        }


        [Fact]
        public void GameStart_ShowsTheSquaresOfTheBoardModel()
        {
            var modelSquares = Enumerable.Range(0, 64).Select(index => new BoardSquare((Square)index, PieceType.None)).ToArray();
            modelSquares[(int)Square.e1].Piece = PieceType.WKing;
            _boardModel.Setup(model => model.GetSquare(It.IsAny<Square>())).Returns((Square square) => modelSquares[(int)square]);

            _gameService.Raise(service => service.GameStarted += null);

            _boardModel.Verify(model => model.SyncBoardStateFromNative(), Times.Once);
            Assert.Equal(modelSquares, _viewModel.Board);
            Assert.Equal(PieceType.WKing, SquareAt(Square.e1).Piece);
        }


        [Fact]
        public void GameReset_ShowsAnEmptyBoardAgain()
        {
            var king = new BoardSquare(Square.e1, PieceType.WKing);
            _viewModel.Board[(int)Square.e1] = king;

            _gameService.Raise(service => service.GameReset += null);

            Assert.Equal(64, _viewModel.Board.Count);
            Assert.All(_viewModel.Board, square => Assert.Equal(PieceType.None, square.Piece));
        }


        [Fact]
        public void GameEnd_ClearsTheHighlights()
        {
            SquareAt(Square.d4).IsHighlighted = true;

            _gameService.Raise(service => service.GameEnded += null);

            Assert.DoesNotContain(_viewModel.Board, square => square.IsHighlighted);
        }


        [Fact]
        public void PlayerChange_UpdatesTheTurnText()
        {
            List<string?> changedProperties = TestData.RecordPropertyChanges(_viewModel);

            _moveModel.Raise(model => model.PlayerChanged += null, Side.Black);

            Assert.Equal(Side.Black, _viewModel.CurrentPlayer);
            Assert.Equal("Black's Turn", _viewModel.CurrentPlayerText);
            Assert.Contains(nameof(ChessBoardViewModel.CurrentPlayer), changedProperties);
            Assert.Contains(nameof(ChessBoardViewModel.CurrentPlayerText), changedProperties);

            _moveModel.Raise(model => model.PlayerChanged += null, Side.White);

            Assert.Equal("White's Turn", _viewModel.CurrentPlayerText);
        }


        [Fact]
        public void EndOfGame_AsksForTheEndGameDialog()
        {
            (EndGameState State, Side Winner)? dialog = null;
            _viewModel.ShowEndGameDialog += (state, winner) =>
            {
                dialog = (state, winner);
                return Task.CompletedTask;
            };

            _moveModel.Raise(model => model.GameOverEvent += null, EndGameState.Checkmate, Side.White);

            Assert.Equal((EndGameState.Checkmate, Side.White), dialog);
        }


        [Fact]
        public void Promotion_SendsTheChosenPieceToTheEngine()
        {
            _viewModel.ShowPawnPromotionDialogRequested += () => Task.FromResult<PieceType?>(PieceType.WKnight);

            _moveModel.Raise(model => model.PawnPromotionEvent += null);

            _moveModel.Verify(model => model.SetPromotionPieceType(PieceType.WKnight), Times.Once);
        }


        [Fact]
        public void Promotion_WithoutAChoice_SendsNothing()
        {
            _viewModel.ShowPawnPromotionDialogRequested += () => Task.FromResult<PieceType?>(null);

            _moveModel.Raise(model => model.PawnPromotionEvent += null);

            _moveModel.Verify(model => model.SetPromotionPieceType(It.IsAny<PieceType>()), Times.Never);
        }


        [Fact]
        public void BoardStyleChange_RefreshesTheBackground()
        {
            List<string?> changedProperties = TestData.RecordPropertyChanges(_viewModel);
            _styleManager.SetupGet(manager => manager.CurrentBoardStyle).Returns(BoardStyle.Glass);

            _styleManager.Raise(manager => manager.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IStyleManager.CurrentBoardStyle)));

            Assert.Equal(BoardStyle.Glass, _viewModel.CurrentBoardStyle);
            Assert.Contains(nameof(ChessBoardViewModel.BoardBackgroundImage), changedProperties);
        }


        [Fact]
        public void IsMultiplayerGame_ComesFromTheGameService()
        {
            _gameService.SetupGet(service => service.IsMultiplayerGame).Returns(true);

            Assert.True(_viewModel.IsMultiplayerGame);
        }
    }


    public class GameSetupViewModelTests
    {
        private readonly Mock<IChessGameService> _gameService = new();
        private readonly Mock<INavigationService> _navigationService = new();
        private readonly GameSetupViewModel _viewModel;


        public GameSetupViewModelTests()
        {
            _gameService.Setup(service => service.StartGameAsync(It.IsAny<GameConfiguration>())).ReturnsAsync(true);
            _viewModel = new GameSetupViewModel(_gameService.Object, _navigationService.Object);
        }


        [Fact]
        public async Task StartGameAsync_ValidConfiguration_StartsGameAndNavigates()
        {
            var config = GameConfiguration.CreateSinglePlayer(Side.White, CPUDifficulty.Easy);

            bool started = await _viewModel.StartGameAsync(config);

            Assert.True(started);
            _gameService.Verify(service => service.StartGameAsync(config), Times.Once);
            _navigationService.Verify(navigation => navigation.NavigateToGameAsync(), Times.Once);
        }


        [Fact]
        public async Task StartGameAsync_InvalidConfiguration_DoesNotStartGame()
        {
            var config = GameConfiguration.CreateSinglePlayer(Side.None, CPUDifficulty.Easy);

            bool started = await _viewModel.StartGameAsync(config);

            Assert.False(started);
            _gameService.Verify(service => service.StartGameAsync(It.IsAny<GameConfiguration>()), Times.Never);
        }


        [Fact]
        public async Task StartLocalCoopGame_StartsALocalCoopGame()
        {
            Assert.True(await _viewModel.StartLocalCoopGameAsync());

            _gameService.Verify(service => service.StartGameAsync(GameConfiguration.CreateLocalCoop()), Times.Once);
        }


        [Fact]
        public async Task StartSinglePlayerWithCurrentState_UsesTheSelectedColorAndDifficulty()
        {
            _viewModel.PlayerColor = Side.Black;
            _viewModel.CPUDifficulty = CPUDifficulty.Hard;

            Assert.True(await _viewModel.StartSinglePlayerWithCurrentStateAsync());

            _gameService.Verify(service => service.StartGameAsync(GameConfiguration.CreateSinglePlayer(Side.Black, CPUDifficulty.Hard)), Times.Once);
        }


        [Fact]
        public async Task StartSinglePlayerGame_WithoutDifficulty_DoesNotStart()
        {
            Assert.False(await _viewModel.StartSinglePlayerGameAsync(Side.White, CPUDifficulty.None));

            _gameService.Verify(service => service.StartGameAsync(It.IsAny<GameConfiguration>()), Times.Never);
        }


        [Fact]
        public async Task StartMultiplayerGame_StartsWithTheLocalColor()
        {
            Assert.True(await _viewModel.StartMultiplayerGameAsync(Side.Black));

            _gameService.Verify(service => service.StartGameAsync(GameConfiguration.CreateMultiplayer(Side.Black)), Times.Once);
        }


        [Fact]
        public async Task FailedStart_DoesNotNavigateToTheGame()
        {
            _gameService.Setup(service => service.StartGameAsync(It.IsAny<GameConfiguration>())).ReturnsAsync(false);

            Assert.False(await _viewModel.StartLocalCoopGameAsync());

            _navigationService.Verify(navigation => navigation.NavigateToGameAsync(), Times.Never);
        }


        [Fact]
        public async Task StartThatThrows_IsReportedAsFailed()
        {
            _gameService.Setup(service => service.StartGameAsync(It.IsAny<GameConfiguration>())).ThrowsAsync(new InvalidOperationException());

            Assert.False(await _viewModel.StartLocalCoopGameAsync());
        }


        [Fact]
        public async Task ResetCurrentGame_ResetsThroughTheGameService()
        {
            await _viewModel.ResetCurrentGameAsync();

            _gameService.Verify(service => service.ResetGameAsync(), Times.Once);
        }


        [Fact]
        public void Reset_ClearsTheSelection()
        {
            _viewModel.PlayerColor = Side.White;
            _viewModel.CPUDifficulty = CPUDifficulty.Medium;

            _viewModel.Reset();

            Assert.Equal(Side.None, _viewModel.PlayerColor);
            Assert.Equal(CPUDifficulty.None, _viewModel.CPUDifficulty);
        }


        [Fact]
        public void Selection_IsReportedOnlyWhenItChanges()
        {
            List<string?> changedProperties = TestData.RecordPropertyChanges(_viewModel);

            _viewModel.PlayerColor = Side.Black;
            _viewModel.PlayerColor = Side.Black;
            _viewModel.CPUDifficulty = CPUDifficulty.Easy;

            Assert.Equal(new[] { nameof(GameSetupViewModel.PlayerColor), nameof(GameSetupViewModel.CPUDifficulty) }, changedProperties);
        }
    }


    public class MultiplayerViewModelTests
    {
        private readonly Mock<IMultiplayerModel> _model = new();
        private readonly Mock<IMultiplayerPreferencesModel> _preferences = new();
        private readonly MultiplayerViewModel _viewModel;


        public MultiplayerViewModelTests()
        {
            _preferences.Setup(preferences => preferences.GetLocalPlayerName()).Returns("Jens");
            _viewModel = new MultiplayerViewModel(new InlineDispatcherQueue(), _model.Object, _preferences.Object);
        }


        private void EngineReports(MultiplayerState state, string remoteName = "")
        {
            _model.Raise(model => model.OnMultiplayerStateChanged += null, state, remoteName);
        }


        private void AssertOnlyVisible(string viewProperty)
        {
            var views = new Dictionary<string, Visibility>
            {
                [nameof(MultiplayerViewModel.InitView)] = _viewModel.InitView,
                [nameof(MultiplayerViewModel.SearchingView)] = _viewModel.SearchingView,
                [nameof(MultiplayerViewModel.OpponentFoundView)] = _viewModel.OpponentFoundView,
                [nameof(MultiplayerViewModel.ConnectionRequestedView)] = _viewModel.ConnectionRequestedView,
                [nameof(MultiplayerViewModel.WaitingForResponseView)] = _viewModel.WaitingForResponseView,
                [nameof(MultiplayerViewModel.SettingLocalPlayerView)] = _viewModel.SettingLocalPlayerView,
            };

            Assert.Equal(new[] { viewProperty }, views.Where(view => view.Value == Visibility.Visible).Select(view => view.Key));
        }


        [Fact]
        public void PlayerName_ComesFromThePreferences_AndFollowsChanges()
        {
            Assert.Equal("Jens", _viewModel.LocalPlayerName);

            _preferences.Raise(preferences => preferences.PlayerNameChanged += null, "Magnus");

            Assert.Equal("Magnus", _viewModel.LocalPlayerName);
        }


        [Fact]
        public void Initially_OnlyTheInitViewIsShown()
        {
            AssertOnlyVisible(nameof(MultiplayerViewModel.InitView));
            Assert.True(_viewModel.SettingsEditable);
        }


        [Fact]
        public void StartMultiplayerSetup_StartsMultiplayerInTheEngine()
        {
            _viewModel.StartMultiplayerSetup();

            _model.Verify(model => model.StartMultiplayer(), Times.Once);
        }


        [Fact]
        public void FindOpponent_SearchesAndLocksTheSettings()
        {
            _viewModel.FindOpponent();

            _model.Verify(model => model.FindOpponent(), Times.Once);
            AssertOnlyVisible(nameof(MultiplayerViewModel.SearchingView));
            Assert.True(_viewModel.Processing);
            Assert.False(_viewModel.SettingsEditable);
        }


        [Fact]
        public void DiscoveredOpponent_IsListedOnceAndSelected()
        {
            _model.Raise(model => model.OnOpponentDiscovered += null, "Anna");
            _model.Raise(model => model.OnOpponentDiscovered += null, "Anna");

            Assert.Equal(new[] { "Anna" }, _viewModel.DiscoveredOpponents);
            Assert.Equal(0, _viewModel.SelectedOpponentIndex);
            AssertOnlyVisible(nameof(MultiplayerViewModel.OpponentFoundView));
            Assert.False(_viewModel.Processing);
        }


        [Fact]
        public void ConnectToOpponent_WithoutSelection_DoesNothing()
        {
            _viewModel.ConnectToOpponent();

            _model.Verify(model => model.ConnectToOpponent(It.IsAny<int>()), Times.Never);
        }


        [Fact]
        public void ConnectToOpponent_ConnectsToTheSelectedOne()
        {
            _model.Raise(model => model.OnOpponentDiscovered += null, "Anna");
            _model.Raise(model => model.OnOpponentDiscovered += null, "Ben");
            _viewModel.SelectedOpponentIndex = 1;

            _viewModel.ConnectToOpponent();

            _model.Verify(model => model.ConnectToOpponent(1), Times.Once);
            Assert.Equal("Ben", _viewModel.RemotePlayerName);
            AssertOnlyVisible(nameof(MultiplayerViewModel.WaitingForResponseView));
        }


        [Fact]
        public void ConnectionRequest_ShowsWhoIsAsking()
        {
            EngineReports(MultiplayerState.ConnectionRequested, "Anna");

            Assert.Equal("Anna", _viewModel.RemotePlayerName);
            AssertOnlyVisible(nameof(MultiplayerViewModel.ConnectionRequestedView));
        }


        [Fact]
        public void AcceptConnectionRequest_AnswersTheEngine()
        {
            _viewModel.AcceptConnectionRequest();

            _model.Verify(model => model.RespondToConnectionRequest(true), Times.Once);
        }


        [Fact]
        public void DeclineConnectionRequest_DeclinesAndSearchesAgain()
        {
            _viewModel.DeclineConnectionRequest();

            _model.Verify(model => model.RespondToConnectionRequest(false), Times.Once);
            _model.Verify(model => model.FindOpponent(), Times.Once);
            AssertOnlyVisible(nameof(MultiplayerViewModel.SearchingView));
        }


        [Fact]
        public void PlayerSetup_ShowsTheColorSelectionWithNothingChosen()
        {
            EngineReports(MultiplayerState.PlayerSetup);

            AssertOnlyVisible(nameof(MultiplayerViewModel.SettingLocalPlayerView));
            Assert.Equal(Side.None, _viewModel.LocalPlayer);
            Assert.False(_viewModel.IsReady);
        }


        [Fact]
        public void SelectPlayerColor_TellsTheEngineAndEnablesReady()
        {
            _viewModel.SelectPlayerColor(Side.Black);

            _model.Verify(model => model.SetLocalPlayerColor(Side.Black), Times.Once);
            Assert.Equal(Side.Black, _viewModel.LocalPlayer);
            Assert.True(_viewModel.ReadyButtonEnabled);
        }


        [Fact]
        public void SetPlayerReady_TellsTheEngineOnce()
        {
            _viewModel.SetPlayerReady();
            _viewModel.SetPlayerReady();

            _model.Verify(model => model.SetPlayerReady(true), Times.Once);
            Assert.True(_viewModel.IsReady);
        }


        [Fact]
        public void InGame_AsksToShowTheChessboard()
        {
            int navigationRequests = 0;
            _viewModel.RequestNavigationToChessboard += () => navigationRequests++;

            EngineReports(MultiplayerState.InGame);

            Assert.Equal(1, navigationRequests);
        }


        [Fact]
        public void Disconnect_ClosesTheChessboardAndStartsOver()
        {
            int closeRequests = 0;
            _viewModel.RequestCloseChessboard += () => closeRequests++;
            _model.Raise(model => model.OnOpponentDiscovered += null, "Anna");

            EngineReports(MultiplayerState.Disconnected);

            Assert.Equal(1, closeRequests);
            Assert.Empty(_viewModel.DiscoveredOpponents);
            Assert.Equal(-1, _viewModel.SelectedOpponentIndex);
            AssertOnlyVisible(nameof(MultiplayerViewModel.InitView));
        }


        [Fact]
        public void CancelSearch_StopsSearchingAndStartsOver()
        {
            _viewModel.FindOpponent();
            _model.Raise(model => model.OnOpponentDiscovered += null, "Anna");

            _viewModel.CancelSearch();

            _model.Verify(model => model.CancelSearch(), Times.Once);
            Assert.Empty(_viewModel.DiscoveredOpponents);
            Assert.Equal(-1, _viewModel.SelectedOpponentIndex);
            AssertOnlyVisible(nameof(MultiplayerViewModel.InitView));
            Assert.True(_viewModel.SettingsEditable);
        }


        [Fact]
        public void PlayerChange_TellsWhetherItIsTheLocalPlayersTurn()
        {
            _viewModel.SelectPlayerColor(Side.White);

            _model.Raise(model => model.OnPlayerChanged += null, Side.White);
            Assert.True(_viewModel.IsLocalPlayersTurn);

            _model.Raise(model => model.OnPlayerChanged += null, Side.Black);
            Assert.False(_viewModel.IsLocalPlayersTurn);
        }


        [Fact]
        public void ResetViewState_ForgetsThePreviousSession()
        {
            _model.Raise(model => model.OnOpponentDiscovered += null, "Anna");
            EngineReports(MultiplayerState.ConnectionRequested, "Anna");
            _viewModel.SelectPlayerColor(Side.White);

            _viewModel.ResetViewState();

            AssertOnlyVisible(nameof(MultiplayerViewModel.InitView));
            Assert.Equal(Side.None, _viewModel.LocalPlayer);
            Assert.Equal(string.Empty, _viewModel.RemotePlayerName);
            Assert.False(_viewModel.ReadyButtonEnabled);
            Assert.Empty(_viewModel.DiscoveredOpponents);
        }
    }
}
