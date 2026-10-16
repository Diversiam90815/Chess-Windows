using Chess.UI.Services;
using Moq;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Test
{
    public class ChessGameServiceTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

        private readonly Mock<ICommunicationLayer> _engine = new();
        private readonly ChessGameService _service;
        private readonly GameConfiguration _coop = GameConfiguration.CreateLocalCoop();
        private readonly GameConfiguration _singlePlayer = GameConfiguration.CreateSinglePlayer(Side.Black, CPUDifficulty.Hard);


        public ChessGameServiceTests()
        {
            _service = new ChessGameService(_engine.Object);
        }


        private void EngineReports(GamePhase phase)
        {
            _engine.Raise(engine => engine.GameStateChanged += null, phase);
        }


        private async Task StartAndConfirmAsync(GameConfiguration config)
        {
            Task<bool> start = _service.StartGameAsync(config);
            EngineReports(GamePhase.PlayerTurn);

            Assert.True(await start.WaitAsync(Timeout));
        }


        [Fact]
        public void StartGame_InInitialState_SendsTheStartAndWaitsForTheEngine()
        {
            Task<bool> start = _service.StartGameAsync(_coop);

            _engine.Verify(engine => engine.StartGame(_coop), Times.Once);
            _engine.Verify(engine => engine.ResetGame(), Times.Never);
            Assert.False(start.IsCompleted);
            Assert.False(_service.IsGameActive);
        }


        [Fact]
        public async Task StartGame_IsReportedWhenTheEngineLeavesItsInitialState()
        {
            var raisedEvents = new List<string>();
            _service.ConfigurationChanged += config => raisedEvents.Add($"Configuration:{config.Mode}");
            _service.GameStarted += () => raisedEvents.Add("Started");

            Task<bool> start = _service.StartGameAsync(_coop);
            EngineReports(GamePhase.PlayerTurn);

            Assert.True(await start.WaitAsync(Timeout));
            Assert.True(_service.IsGameActive);
            Assert.Equal(_coop, _service.CurrentConfiguration);
            Assert.Equal(new[] { "Configuration:LocalCoop", "Started" }, raisedEvents);
        }


        [Fact]
        public async Task StartGame_IsAlsoConfirmedWhenTheOpponentMovesFirst()
        {
            Task<bool> start = _service.StartGameAsync(_singlePlayer);
            EngineReports(GamePhase.OpponentTurn);

            Assert.True(await start.WaitAsync(Timeout));
            Assert.Equal(_singlePlayer, _service.CurrentConfiguration);
        }


        [Fact]
        public async Task StartGame_WhileAGameIsRunning_ResetsTheEngineBeforeStarting()
        {
            await StartAndConfirmAsync(_coop);

            int resets = 0;
            _service.GameReset += () => resets++;

            Task<bool> restart = _service.StartGameAsync(_singlePlayer);

            _engine.Verify(engine => engine.ResetGame(), Times.Once);
            _engine.Verify(engine => engine.StartGame(_singlePlayer), Times.Never);

            EngineReports(GamePhase.Initializing);

            Assert.Equal(1, resets);
            Assert.False(_service.IsGameActive);
            _engine.Verify(engine => engine.StartGame(_singlePlayer), Times.Once);

            EngineReports(GamePhase.OpponentTurn);

            Assert.True(await restart.WaitAsync(Timeout));
            Assert.True(_service.IsGameActive);
            Assert.Equal(_singlePlayer, _service.CurrentConfiguration);
        }


        [Fact]
        public async Task StartGame_ReplacesAStartTheEngineHasNotConfirmed()
        {
            await StartAndConfirmAsync(_coop);

            Task<bool> first = _service.StartGameAsync(_coop);
            Task<bool> second = _service.StartGameAsync(_singlePlayer);

            Assert.False(await first.WaitAsync(Timeout));

            EngineReports(GamePhase.Initializing);
            EngineReports(GamePhase.OpponentTurn);

            Assert.True(await second.WaitAsync(Timeout));
            Assert.Equal(_singlePlayer, _service.CurrentConfiguration);
            _engine.Verify(engine => engine.StartGame(_coop), Times.Once);
        }


        [Fact]
        public async Task ResetGame_InInitialState_IsReportedImmediately()
        {
            int resets = 0;
            _service.GameReset += () => resets++;

            Assert.True(await _service.ResetGameAsync().WaitAsync(Timeout));

            Assert.Equal(1, resets);
            _engine.Verify(engine => engine.ResetGame(), Times.Never);
        }


        [Fact]
        public async Task ResetGame_WhileAGameIsRunning_WaitsForTheEngine()
        {
            await StartAndConfirmAsync(_coop);

            int resets = 0;
            _service.GameReset += () => resets++;

            Task<bool> reset = _service.ResetGameAsync();

            _engine.Verify(engine => engine.ResetGame(), Times.Once);
            Assert.False(reset.IsCompleted);
            Assert.Equal(0, resets);

            EngineReports(GamePhase.Initializing);

            Assert.True(await reset.WaitAsync(Timeout));
            Assert.Equal(1, resets);
            Assert.False(_service.IsGameActive);
            Assert.Equal(_coop, _service.CurrentConfiguration);
        }


        [Fact]
        public async Task ResetGame_DoesNotStartANewGame()
        {
            await StartAndConfirmAsync(_coop);

            Task<bool> reset = _service.ResetGameAsync();
            EngineReports(GamePhase.Initializing);
            await reset.WaitAsync(Timeout);

            _engine.Verify(engine => engine.StartGame(It.IsAny<GameConfiguration>()), Times.Once);
        }


        [Fact]
        public async Task EndGame_EndsTheGameAndResetsTheEngine()
        {
            await StartAndConfirmAsync(_coop);

            var raisedEvents = new List<string>();
            _service.GameEnded += () => raisedEvents.Add("Ended");
            _service.GameReset += () => raisedEvents.Add("Reset");

            await _service.EndGameAsync();

            Assert.False(_service.IsGameActive);
            Assert.Null(_service.CurrentConfiguration);
            Assert.Equal(new[] { "Ended" }, raisedEvents);
            _engine.Verify(engine => engine.ResetGame(), Times.Once);

            EngineReports(GamePhase.Initializing);

            Assert.Equal(new[] { "Ended", "Reset" }, raisedEvents);
        }


        [Fact]
        public async Task EndGame_AfterTheGameIsOver_StillResetsTheEngine()
        {
            await StartAndConfirmAsync(_coop);
            EngineReports(GamePhase.GameEnded);

            await _service.EndGameAsync();

            _engine.Verify(engine => engine.ResetGame(), Times.Once);
        }


        [Fact]
        public async Task EndGame_InInitialState_DoesNotBotherTheEngine()
        {
            await _service.EndGameAsync();

            _engine.Verify(engine => engine.ResetGame(), Times.Never);
        }


        [Fact]
        public async Task EndGame_DropsAStartThatIsWaitingForTheReset()
        {
            await StartAndConfirmAsync(_coop);

            Task<bool> restart = _service.StartGameAsync(_singlePlayer);
            await _service.EndGameAsync();

            Assert.False(await restart.WaitAsync(Timeout));

            EngineReports(GamePhase.Initializing);

            _engine.Verify(engine => engine.StartGame(_singlePlayer), Times.Never);
            Assert.False(_service.IsGameActive);
        }


        [Fact]
        public async Task EngineResettingOnItsOwn_IsReportedAsGameReset()
        {
            await StartAndConfirmAsync(_coop);

            int resets = 0;
            _service.GameReset += () => resets++;

            EngineReports(GamePhase.Initializing);

            Assert.Equal(1, resets);
            Assert.False(_service.IsGameActive);
        }


        [Fact]
        public async Task PhaseChangesDuringAGame_DoNotRestartIt()
        {
            await StartAndConfirmAsync(_coop);

            int starts = 0;
            _service.GameStarted += () => starts++;

            EngineReports(GamePhase.OpponentTurn);
            EngineReports(GamePhase.PromotionDialog);
            EngineReports(GamePhase.PlayerTurn);
            EngineReports(GamePhase.GameEnded);

            Assert.Equal(0, starts);
            Assert.True(_service.IsGameActive);
        }


        [Fact]
        public async Task IsMultiplayerGame_FollowsTheCurrentConfiguration()
        {
            Assert.False(_service.IsMultiplayerGame);

            await StartAndConfirmAsync(GameConfiguration.CreateMultiplayer(Side.White));

            Assert.True(_service.IsMultiplayerGame);
        }
    }
}
