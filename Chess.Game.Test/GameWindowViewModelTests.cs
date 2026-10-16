using Chess.UI.Models;
using Chess.UI.Services;
using Chess.UI.Settings;
using Chess.UI.Styles;
using Chess.UI.Test.Helpers;
using Chess.UI.ViewModels;
using Moq;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Test
{
    /// <summary>
    /// Builds the view models of the game page on top of mocked models and services.
    /// </summary>
    public class GameViewModelsFixture
    {
        public Mock<IChessGameService> GameService { get; } = new();
        public Mock<IMoveModel> MoveModel { get; } = new();
        public Mock<IMoveHistoryModel> MoveHistoryModel { get; } = new();
        public Mock<ICapturedPiecesModel> CapturedPiecesModel { get; } = new();
        public Mock<IMultiplayerModel> MultiplayerModel { get; } = new();

        public MultiplayerViewModel MultiplayerViewModel { get; }
        public MultiplayerInfoViewModel MultiplayerInfoViewModel { get; }
        public ChessBoardViewModel ChessBoardViewModel { get; }


        public GameViewModelsFixture()
        {
            var dispatcher = new InlineDispatcherQueue();

            MoveModel.SetupGet(model => model.LegalMoves).Returns(new List<Move>());
            MoveHistoryModel.SetupGet(model => model.MoveHistory).Returns(new List<string>());
            CapturedPiecesModel.SetupGet(model => model.CapturedPieces).Returns(new Dictionary<PieceType, int>());
            GameService.Setup(service => service.StartGameAsync(It.IsAny<GameConfiguration>())).ReturnsAsync(true);

            MultiplayerViewModel = new MultiplayerViewModel(dispatcher, MultiplayerModel.Object, Mock.Of<IMultiplayerPreferencesModel>());
            MultiplayerInfoViewModel = new MultiplayerInfoViewModel(dispatcher, GameService.Object, MoveModel.Object, MultiplayerViewModel);
            ChessBoardViewModel = new ChessBoardViewModel(dispatcher, GameService.Object, Mock.Of<IStyleManager>(), MoveModel.Object,
                Mock.Of<IBoardModel>(), Mock.Of<IImageService>(), Mock.Of<ICommunicationLayer>());
        }


        public GameWindowViewModel CreateGameWindowViewModel()
        {
            var dispatcher = new InlineDispatcherQueue();

            return new GameWindowViewModel(
                dispatcher,
                GameService.Object,
                ChessBoardViewModel,
                new CapturedPiecesViewModel(dispatcher, CapturedPiecesModel.Object, Mock.Of<IImageService>()),
                new MoveHistoryViewModel(dispatcher, MoveHistoryModel.Object),
                MultiplayerInfoViewModel);
        }
    }


    public class GameWindowViewModelTests
    {
        private readonly GameViewModelsFixture _fixture = new();


        private void GameServiceReports(GameConfiguration config)
        {
            _fixture.GameService.Raise(service => service.ConfigurationChanged += null, config);
        }


        [Fact]
        public void ChildViewModels_AreTheInjectedOnes()
        {
            GameWindowViewModel viewModel = _fixture.CreateGameWindowViewModel();

            Assert.Same(_fixture.ChessBoardViewModel, viewModel.ChessBoardViewModel);
            Assert.Same(_fixture.MultiplayerInfoViewModel, viewModel.MultiplayerInfoViewModel);
            Assert.NotNull(viewModel.CapturedPiecesViewModel);
            Assert.NotNull(viewModel.MoveHistoryViewModel);
        }


        [Theory]
        [InlineData(GameModeSelection.LocalCoop, true, false, "Local Co-op")]
        [InlineData(GameModeSelection.MultiPlayer, false, true, "Multiplayer")]
        [InlineData(GameModeSelection.SinglePlayer, false, false, "vs Computer")]
        public void Configuration_DecidesTheGameModeShown(GameModeSelection mode, bool isCoop, bool isMultiplayer, string modeText)
        {
            GameWindowViewModel viewModel = _fixture.CreateGameWindowViewModel();

            GameServiceReports(new GameConfiguration { Mode = mode, PlayerColor = Side.White, CpuDifficulty = CPUDifficulty.Easy });

            Assert.Equal(isCoop, viewModel.IsKoopGame);
            Assert.Equal(isMultiplayer, viewModel.IsMultiplayerGame);
            Assert.Equal(modeText, viewModel.GameModeText);
        }


        [Fact]
        public void GameThatIsAlreadyRunning_IsPickedUpOnCreation()
        {
            _fixture.GameService.SetupGet(service => service.CurrentConfiguration).Returns(GameConfiguration.CreateLocalCoop());

            GameWindowViewModel viewModel = _fixture.CreateGameWindowViewModel();

            Assert.True(viewModel.IsKoopGame);
        }


        [Fact]
        public void GameModeChange_IsReported()
        {
            GameWindowViewModel viewModel = _fixture.CreateGameWindowViewModel();
            List<string?> changedProperties = TestData.RecordPropertyChanges(viewModel);

            GameServiceReports(GameConfiguration.CreateLocalCoop());

            Assert.Contains(nameof(GameWindowViewModel.IsKoopGame), changedProperties);
            Assert.Contains(nameof(GameWindowViewModel.GameModeText), changedProperties);
        }


        [Fact]
        public void GameReset_ClearsMoveHistoryAndCapturedPieces()
        {
            _fixture.CreateGameWindowViewModel();

            _fixture.GameService.Raise(service => service.GameReset += null);

            _fixture.MoveHistoryModel.Verify(model => model.ClearHistory(), Times.Once);
            _fixture.CapturedPiecesModel.Verify(model => model.Clear(), Times.Once);
        }


        [Fact]
        public async Task ResetCurrentGame_RestartsWithTheCurrentConfiguration()
        {
            GameConfiguration config = GameConfiguration.CreateSinglePlayer(Side.Black, CPUDifficulty.Medium);
            _fixture.GameService.SetupGet(service => service.CurrentConfiguration).Returns(config);
            GameWindowViewModel viewModel = _fixture.CreateGameWindowViewModel();

            await viewModel.ResetCurrentGameAsync();

            _fixture.GameService.Verify(service => service.StartGameAsync(config), Times.Once);
        }


        [Fact]
        public async Task ResetCurrentGame_WithoutAGame_DoesNothing()
        {
            GameWindowViewModel viewModel = _fixture.CreateGameWindowViewModel();

            await viewModel.ResetCurrentGameAsync();

            _fixture.GameService.Verify(service => service.StartGameAsync(It.IsAny<GameConfiguration>()), Times.Never);
        }


        [Fact]
        public void EndOfGame_IsPassedOnToThePage()
        {
            GameWindowViewModel viewModel = _fixture.CreateGameWindowViewModel();
            (EndGameState State, Side Winner)? dialog = null;
            viewModel.ShowEndGameDialog += (state, winner) =>
            {
                dialog = (state, winner);
                return Task.CompletedTask;
            };

            _fixture.MoveModel.Raise(model => model.GameOverEvent += null, EndGameState.StaleMate, Side.None);

            Assert.Equal((EndGameState.StaleMate, Side.None), dialog);
        }


        [Fact]
        public void Promotion_AsksThePageAndSendsItsAnswerToTheEngine()
        {
            GameWindowViewModel viewModel = _fixture.CreateGameWindowViewModel();
            viewModel.ShowPawnPromotionDialogRequested += () => Task.FromResult<PieceType?>(PieceType.BQueen);

            _fixture.MoveModel.Raise(model => model.PawnPromotionEvent += null);

            _fixture.MoveModel.Verify(model => model.SetPromotionPieceType(PieceType.BQueen), Times.Once);
        }


        [Fact]
        public void Promotion_WithoutAPage_SendsNothing()
        {
            _fixture.CreateGameWindowViewModel();

            _fixture.MoveModel.Raise(model => model.PawnPromotionEvent += null);

            _fixture.MoveModel.Verify(model => model.SetPromotionPieceType(It.IsAny<PieceType>()), Times.Never);
        }


        [Fact]
        public void ButtonClicks_FromTheBoardAndThePage_AreAnnounced()
        {
            GameWindowViewModel viewModel = _fixture.CreateGameWindowViewModel();
            int clicks = 0;
            viewModel.ButtonClicked += () => clicks++;

            _fixture.ChessBoardViewModel.OnButtonClicked();
            viewModel.OnButtonClicked();

            Assert.Equal(2, clicks);
        }
    }


    public class MultiplayerInfoViewModelTests
    {
        private readonly GameViewModelsFixture _fixture = new();

        private MultiplayerInfoViewModel ViewModel => _fixture.MultiplayerInfoViewModel;


        [Theory]
        [InlineData(GameModeSelection.MultiPlayer, true)]
        [InlineData(GameModeSelection.LocalCoop, false)]
        [InlineData(GameModeSelection.SinglePlayer, false)]
        public void IsVisible_OnlyInMultiplayerGames(GameModeSelection mode, bool isVisible)
        {
            _fixture.GameService.Raise(service => service.ConfigurationChanged += null, new GameConfiguration { Mode = mode });

            Assert.Equal(isVisible, ViewModel.IsVisible);
        }


        [Fact]
        public void LocalPlayer_FollowsTheColorChosenInTheMultiplayerSetup()
        {
            _fixture.MultiplayerViewModel.SelectPlayerColor(Side.Black);

            Assert.Equal(Side.Black, ViewModel.LocalPlayer);
        }


        [Fact]
        public void PlayerChange_TellsWhetherItIsTheLocalPlayersTurn()
        {
            _fixture.MultiplayerViewModel.SelectPlayerColor(Side.Black);
            List<string?> changedProperties = TestData.RecordPropertyChanges(ViewModel);

            _fixture.MoveModel.Raise(model => model.PlayerChanged += null, Side.Black);

            Assert.Equal(Side.Black, ViewModel.CurrentPlayer);
            Assert.True(ViewModel.IsLocalPlayersTurn);
            Assert.Contains(nameof(MultiplayerInfoViewModel.IsLocalPlayersTurn), changedProperties);

            _fixture.MoveModel.Raise(model => model.PlayerChanged += null, Side.White);

            Assert.False(ViewModel.IsLocalPlayersTurn);
        }


        [Fact]
        public void OtherChangesInTheMultiplayerSetup_DoNotTouchTheLocalPlayer()
        {
            _fixture.MultiplayerViewModel.SelectPlayerColor(Side.Black);
            List<string?> changedProperties = TestData.RecordPropertyChanges(ViewModel);

            _fixture.MultiplayerViewModel.RemotePlayerName = "Anna";

            Assert.Empty(changedProperties);
            Assert.Equal(Side.Black, ViewModel.LocalPlayer);
        }
    }
}
