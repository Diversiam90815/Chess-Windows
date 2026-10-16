using Chess.UI.Audio.Core;
using Chess.UI.Audio.Modules;
using Moq;


namespace Chess.UI.Test
{
    public class AudioEngineTests
    {
        private readonly AudioEngine _audioEngine = new();
        private readonly Mock<IAudioModule> _sfx = new();
        private readonly Mock<IAudioModule> _atmosphere = new();


        public AudioEngineTests()
        {
            _sfx.SetupGet(module => module.ModuleName).Returns("SFX");
            _atmosphere.SetupGet(module => module.ModuleName).Returns("Atmosphere");
            _sfx.Setup(module => module.InitializeAsync()).Returns(Task.CompletedTask);
            _atmosphere.Setup(module => module.InitializeAsync()).Returns(Task.CompletedTask);
        }


        private void RegisterBothModules()
        {
            _audioEngine.RegisterModule(AudioModuleType.SFX, _sfx.Object);
            _audioEngine.RegisterModule(AudioModuleType.Atmosphere, _atmosphere.Object);
        }


        [Fact]
        public void RegisteredModule_CanBeLookedUp()
        {
            AudioEngineEventArgs? registered = null;
            _audioEngine.ModuleRegistered += (_, e) => registered = e;

            _audioEngine.RegisterModule(AudioModuleType.SFX, _sfx.Object);

            Assert.Same(_sfx.Object, _audioEngine.GetModule(AudioModuleType.SFX));
            Assert.Same(_sfx.Object, _audioEngine.GetModule<IAudioModule>(AudioModuleType.SFX));
            Assert.Null(_audioEngine.GetModule(AudioModuleType.Music));
            Assert.Equal("SFX", registered?.ModuleName);
            Assert.Equal(AudioModuleType.SFX, registered?.ModuleType);
        }


        [Fact]
        public void SecondModuleOfAType_IsIgnored()
        {
            int registrations = 0;
            _audioEngine.ModuleRegistered += (_, _) => registrations++;

            _audioEngine.RegisterModule(AudioModuleType.SFX, _sfx.Object);
            _audioEngine.RegisterModule(AudioModuleType.SFX, _atmosphere.Object);
            _audioEngine.RegisterModule(AudioModuleType.Music, null);

            Assert.Same(_sfx.Object, _audioEngine.GetModule(AudioModuleType.SFX));
            Assert.Equal(1, registrations);
        }


        [Fact]
        public async Task Initialize_InitializesEveryModuleOnce()
        {
            RegisterBothModules();

            await _audioEngine.InitializeAsync();
            await _audioEngine.InitializeAsync();

            _sfx.Verify(module => module.InitializeAsync(), Times.Once);
            _atmosphere.Verify(module => module.InitializeAsync(), Times.Once);
        }


        [Fact]
        public async Task Initialize_ReportsAModuleThatFails()
        {
            _sfx.Setup(module => module.InitializeAsync()).ThrowsAsync(new InvalidOperationException("No audio device"));
            _audioEngine.RegisterModule(AudioModuleType.SFX, _sfx.Object);

            AudioErrorEventArgs? error = null;
            _audioEngine.AudioError += (_, e) => error = e;

            await Assert.ThrowsAsync<InvalidOperationException>(() => _audioEngine.InitializeAsync());

            Assert.IsType<InvalidOperationException>(error?.Exception);
        }


        [Theory]
        [InlineData(0.4f, 0.4f)]
        [InlineData(1.7f, 1.0f)]
        [InlineData(-0.5f, 0.0f)]
        public void MasterVolume_IsClampedAndPassedToEveryModule(float volume, float expected)
        {
            RegisterBothModules();

            _audioEngine.SetMasterVolume(volume);

            Assert.Equal(expected, _audioEngine.GetMasterVolume());
            _sfx.Verify(module => module.SetMasterVolume(expected), Times.Once);
            _atmosphere.Verify(module => module.SetMasterVolume(expected), Times.Once);
        }


        [Fact]
        public void UnregisteredModule_IsDisposedAndGone()
        {
            RegisterBothModules();

            AudioEngineEventArgs? unregistered = null;
            _audioEngine.ModuleUnregistered += (_, e) => unregistered = e;

            _audioEngine.UnregisterModule(AudioModuleType.SFX);
            _audioEngine.UnregisterModule(AudioModuleType.Music);

            _sfx.Verify(module => module.Dispose(), Times.Once);
            _atmosphere.Verify(module => module.Dispose(), Times.Never);
            Assert.Null(_audioEngine.GetModule(AudioModuleType.SFX));
            Assert.Equal(AudioModuleType.SFX, unregistered?.ModuleType);
        }


        [Fact]
        public void Dispose_DisposesEveryModule()
        {
            RegisterBothModules();

            _audioEngine.Dispose();

            _sfx.Verify(module => module.Dispose(), Times.Once);
            _atmosphere.Verify(module => module.Dispose(), Times.Once);
            Assert.Null(_audioEngine.GetModule(AudioModuleType.Atmosphere));
        }
    }


    public class AudioModuleVolumeTests
    {
        public static TheoryData<IAudioModule> Modules => new() { new SoundEffectsModule(), new AtmosphereModule() };


        [Theory]
        [MemberData(nameof(Modules))]
        public void EffectiveVolume_IsModuleTimesMasterVolume(IAudioModule module)
        {
            module.SetModuleVolume(0.5f);
            module.SetMasterVolume(0.5f);

            Assert.Equal(0.5f, module.GetModuleVolume());
            Assert.Equal(0.25f, module.GetEffectiveVolume());
        }


        [Theory]
        [MemberData(nameof(Modules))]
        public void Volumes_AreClampedToTheValidRange(IAudioModule module)
        {
            module.SetModuleVolume(3.0f);
            module.SetMasterVolume(-1.0f);

            Assert.Equal(1.0f, module.GetModuleVolume());
            Assert.Equal(0.0f, module.GetEffectiveVolume());
        }


        [Fact]
        public void Modules_NameTheirType()
        {
            Assert.Equal(AudioModuleType.SFX, new SoundEffectsModule().ModuleType);
            Assert.Equal(AudioModuleType.Atmosphere, new AtmosphereModule().ModuleType);
        }


        [Fact]
        public async Task SoundEffects_BeforeInitialization_PlayNothing()
        {
            var module = new SoundEffectsModule();
            int playedSounds = 0;
            module.SoundPlayed += (_, _) => playedSounds++;

            await module.PlaySoundAsync(SoundEffect.PieceMove);

            Assert.Equal(0, playedSounds);
        }


        [Fact]
        public async Task Atmosphere_BeforeInitialization_KeepsItsDefaultScenario()
        {
            var module = new AtmosphereModule();

            await module.SetAtmosphereAsync(AtmosphereScenario.Fireplace);

            Assert.Equal(AtmosphereScenario.Forest, module.CurrentScenario);
            Assert.False(module.IsPlaying);
        }
    }
}
