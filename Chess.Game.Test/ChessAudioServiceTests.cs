using Chess.UI.Audio.Core;
using Chess.UI.Audio.Modules;
using Chess.UI.Audio.Services;
using Chess.UI.Board;
using Chess.UI.Models;
using Chess.UI.Services;
using Chess.UI.Settings;
using Moq;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Test
{
    public class ChessAudioServiceTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

        private readonly Mock<IAudioEngine> _audioEngine = new();
        private readonly Mock<ISettingsService> _settings = new();
        private readonly Mock<ISoundEffectsModule> _soundEffects = new();
        private readonly Mock<IAtmosphereModule> _atmosphere = new();
        private readonly Mock<IMoveModel> _moveModel = new();
        private readonly Mock<ICommunicationLayer> _communication = new();
        private readonly ChessAudioService _service;


        public ChessAudioServiceTests()
        {
            _soundEffects.SetupProperty(module => module.IsEnabled, true);
            _atmosphere.SetupProperty(module => module.IsEnabled, true);
            _audioEngine.Setup(engine => engine.InitializeAsync()).Returns(Task.CompletedTask);

            _settings.SetupGet(settings => settings.AudioMasterVolume).Returns(0.7f);
            _settings.SetupGet(settings => settings.AudioSFXVolume).Returns(0.5f);
            _settings.SetupGet(settings => settings.AudioAtmosVolume).Returns(0.3f);
            _settings.SetupGet(settings => settings.AudioSFXEnabled).Returns(true);
            _settings.SetupGet(settings => settings.AudioAtmosEnabled).Returns(false);
            _settings.SetupGet(settings => settings.AudioAtmosScenario).Returns("Fireplace");

            _service = new ChessAudioService(_audioEngine.Object, _settings.Object, _soundEffects.Object, _atmosphere.Object,
                _moveModel.Object, _communication.Object);
        }


        // Completes when the sound effects module is asked to play the given sound
        private Task<float> WhenPlayed(SoundEffect effect)
        {
            var played = new TaskCompletionSource<float>(TaskCreationOptions.RunContinuationsAsynchronously);

            _soundEffects.Setup(module => module.PlaySoundAsync(effect, It.IsAny<float>()))
                .Callback((SoundEffect _, float volume) => played.TrySetResult(volume))
                .Returns(Task.CompletedTask);

            return played.Task.WaitAsync(Timeout);
        }


        [Fact]
        public async Task Initialize_RegistersAndInitializesTheModules()
        {
            await _service.InitializeAsync();

            _audioEngine.Verify(engine => engine.RegisterModule(AudioModuleType.SFX, _soundEffects.Object), Times.Once);
            _audioEngine.Verify(engine => engine.RegisterModule(AudioModuleType.Atmosphere, _atmosphere.Object), Times.Once);
            _audioEngine.Verify(engine => engine.InitializeAsync(), Times.Once);
            Assert.Same(_soundEffects.Object, _service.SoundEffectsModule);
            Assert.Same(_atmosphere.Object, _service.AtmosphereModule);
        }


        [Fact]
        public async Task Initialize_AppliesTheSavedSettings()
        {
            await _service.InitializeAsync();

            _audioEngine.Verify(engine => engine.SetMasterVolume(0.7f), Times.Once);
            _soundEffects.Verify(module => module.SetModuleVolume(0.5f), Times.Once);
            _atmosphere.Verify(module => module.SetModuleVolume(0.3f), Times.Once);
            _atmosphere.Verify(module => module.SetAtmosphereAsync(AtmosphereScenario.Fireplace), Times.Once);
            Assert.True(_soundEffects.Object.IsEnabled);
            Assert.False(_atmosphere.Object.IsEnabled);
        }


        [Fact]
        public async Task Initialize_WithAnUnknownScenario_StartsNoAtmosphere()
        {
            _settings.SetupGet(settings => settings.AudioAtmosScenario).Returns("Volcano");

            await _service.InitializeAsync();

            _atmosphere.Verify(module => module.SetAtmosphereAsync(It.IsAny<AtmosphereScenario>()), Times.Never);
        }


        [Fact]
        public async Task Initialize_SurvivesSettingsThatCannotBeApplied()
        {
            _soundEffects.Setup(module => module.SetModuleVolume(It.IsAny<float>())).Throws<InvalidOperationException>();

            await _service.InitializeAsync();

            _audioEngine.Verify(engine => engine.InitializeAsync(), Times.Once);
        }


        [Fact]
        public async Task Move_PlaysTheMoveSound_AndCaptureTheLouderCaptureSound()
        {
            await _service.HandleMoveAsync(new Move(Square.e2, Square.e4, MoveFlag.DoublePawnPush));
            await _service.HandleMoveAsync(new Move(Square.e4, Square.d5, MoveFlag.Capture));

            _soundEffects.Verify(module => module.PlaySoundAsync(SoundEffect.PieceMove, 0.6f), Times.Once);
            _soundEffects.Verify(module => module.PlaySoundAsync(SoundEffect.PieceCapture, 0.8f), Times.Once);
        }


        [Fact]
        public async Task GameEvents_PlayTheirSounds()
        {
            await _service.HandleEndGameStateAsync(EndGameState.Checkmate);
            await _service.HandleGameStartAsync();

            _soundEffects.Verify(module => module.PlaySoundAsync(SoundEffect.Checkmate, 0.8f), Times.Once);
            _soundEffects.Verify(module => module.PlaySoundAsync(SoundEffect.GameStart, 0.6f), Times.Once);
        }


        [Theory]
        [InlineData(UIInteraction.ButtonClick, SoundEffect.ButtonClick, 0.5f)]
        [InlineData(UIInteraction.MenuOpen, SoundEffect.MenuOpen, 0.5f)]
        [InlineData(UIInteraction.ItemSelected, SoundEffect.ItemSelected, 0.5f)]
        [InlineData(UIInteraction.PieceMove, SoundEffect.PieceMove, 0.5f)]
        [InlineData(UIInteraction.PieceSelect, SoundEffect.PieceSelected, 0.3f)]
        public async Task UIInteraction_PlaysItsSound(UIInteraction interaction, SoundEffect expectedSound, float expectedVolume)
        {
            await _service.HandleUIInteractionAsync(interaction);

            _soundEffects.Verify(module => module.PlaySoundAsync(expectedSound, expectedVolume), Times.Once);
        }


        [Fact]
        public async Task AfterInitialization_GameEventsFromTheEngineAreAudible()
        {
            await _service.InitializeAsync();

            Task<float> moveSound = WhenPlayed(SoundEffect.PieceCapture);
            _communication.Raise(layer => layer.MoveExecuted += null, new Move(Square.e4, Square.d5, MoveFlag.Capture), "exd5");
            Assert.Equal(0.8f, await moveSound);

            Task<float> selectionSound = WhenPlayed(SoundEffect.PieceSelected);
            _moveModel.Raise(model => model.ChesspieceSelected += null);
            Assert.Equal(0.3f, await selectionSound);

            Task<float> gameOverSound = WhenPlayed(SoundEffect.Checkmate);
            _moveModel.Raise(model => model.GameOverEvent += null, EndGameState.Checkmate, Side.White);
            Assert.Equal(0.8f, await gameOverSound);
        }


        [Fact]
        public void SfxEnabled_IsAppliedAndSaved_OnlyWhenItChanges()
        {
            _service.SetSFXEnabled(true);

            _settings.VerifySet(settings => settings.AudioSFXEnabled = It.IsAny<bool>(), Times.Never);

            _service.SetSFXEnabled(false);

            Assert.False(_service.GetSFXEnabled());
            _settings.VerifySet(settings => settings.AudioSFXEnabled = false, Times.Once);
        }


        [Fact]
        public void SfxVolume_IsAppliedAndSaved_OnlyWhenItChanges()
        {
            _soundEffects.Setup(module => module.GetModuleVolume()).Returns(0.5f);

            _service.SetSFXVolume(0.5f);

            _soundEffects.Verify(module => module.SetModuleVolume(It.IsAny<float>()), Times.Never);

            _service.SetSFXVolume(0.2f);

            Assert.Equal(0.5f, _service.GetSFXVolume());
            _soundEffects.Verify(module => module.SetModuleVolume(0.2f), Times.Once);
            _settings.VerifySet(settings => settings.AudioSFXVolume = 0.2f, Times.Once);
        }


        [Fact]
        public void AtmosphereVolume_IsAppliedAndSaved_OnlyWhenItChanges()
        {
            _atmosphere.Setup(module => module.GetModuleVolume()).Returns(1.0f);

            _service.SetAtmosphereVolume(1.0f);
            _service.SetAtmosphereVolume(0.4f);

            Assert.Equal(1.0f, _service.GetAtmosphereVolume());
            _atmosphere.Verify(module => module.SetModuleVolume(0.4f), Times.Once);
            _settings.VerifySet(settings => settings.AudioAtmosVolume = It.IsAny<float>(), Times.Once);
        }


        [Fact]
        public void MasterVolume_IsAppliedAndSaved_OnlyWhenItChanges()
        {
            _audioEngine.Setup(engine => engine.GetMasterVolume()).Returns(0.7f);

            _service.SetMasterVolume(0.7f);
            _service.SetMasterVolume(0.9f);

            Assert.Equal(0.7f, _service.GetMasterVolume());
            _audioEngine.Verify(engine => engine.SetMasterVolume(0.9f), Times.Once);
            _settings.VerifySet(settings => settings.AudioMasterVolume = 0.9f, Times.Once);
        }


        [Fact]
        public async Task Atmosphere_IsSwitchedAndSaved_OnlyWhenItChanges()
        {
            _atmosphere.SetupGet(module => module.CurrentScenario).Returns(AtmosphereScenario.Forest);

            await _service.SetAtmosphereAsync(AtmosphereScenario.Forest);

            _atmosphere.Verify(module => module.SetAtmosphereAsync(It.IsAny<AtmosphereScenario>()), Times.Never);

            await _service.SetAtmosphereAsync(AtmosphereScenario.Fireplace);

            Assert.Equal(AtmosphereScenario.Forest, _service.GetCurrentAtmosphere());
            _atmosphere.Verify(module => module.SetAtmosphereAsync(AtmosphereScenario.Fireplace), Times.Once);
            _settings.VerifySet(settings => settings.AudioAtmosScenario = "Fireplace", Times.Once);
        }


        [Fact]
        public void DisablingTheAtmosphere_StopsItAndSaves()
        {
            _service.SetAtmosphereEnabled(false);

            Assert.False(_service.GetAtmosphereEnabled());
            _atmosphere.Verify(module => module.StopAtmosphereAsync(), Times.Once);
            _settings.VerifySet(settings => settings.AudioAtmosEnabled = false, Times.Once);
        }


        [Fact]
        public async Task EnablingTheAtmosphere_StartsTheCurrentScenarioAndSaves()
        {
            _atmosphere.Object.IsEnabled = false;
            _atmosphere.SetupGet(module => module.CurrentScenario).Returns(AtmosphereScenario.Fireplace);

            var started = new TaskCompletionSource<AtmosphereScenario>(TaskCreationOptions.RunContinuationsAsynchronously);
            _atmosphere.Setup(module => module.SetAtmosphereAsync(It.IsAny<AtmosphereScenario>()))
                .Callback((AtmosphereScenario scenario) => started.TrySetResult(scenario))
                .Returns(Task.CompletedTask);

            _service.SetAtmosphereEnabled(true);

            Assert.Equal(AtmosphereScenario.Fireplace, await started.Task.WaitAsync(Timeout));
            Assert.True(_service.GetAtmosphereEnabled());
            _settings.VerifySet(settings => settings.AudioAtmosEnabled = true, Times.Once);
        }


        [Fact]
        public void AtmosphereEnabled_Unchanged_DoesNothing()
        {
            _service.SetAtmosphereEnabled(true);

            _atmosphere.Verify(module => module.StopAtmosphereAsync(), Times.Never);
            _settings.VerifySet(settings => settings.AudioAtmosEnabled = It.IsAny<bool>(), Times.Never);
        }


        [Fact]
        public void StopAtmosphere_StopsTheModule()
        {
            _service.StopAtmosphereAsync();

            _atmosphere.Verify(module => module.StopAtmosphereAsync(), Times.Once);
        }
    }
}
