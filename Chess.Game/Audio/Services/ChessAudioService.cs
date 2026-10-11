using Chess.UI.Audio.Core;
using Chess.UI.Audio.Modules;
using Chess.UI.Models;
using Chess.UI.Services;
using Chess.UI.Settings;
using System;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Audio.Services
{
    public class ChessAudioService : IChessAudioService
    {
        private readonly IAudioEngine _audioEngine;
        private readonly ISettingsService _settingsService;
        private readonly ISoundEffectsModule _soundEffectsModule;
        private readonly IAtmosphereModule _atmosphereModule;
        private readonly IMoveModel _moveModel;
        private readonly ICommunicationLayer _backendCommunication;

        public ISoundEffectsModule SoundEffectsModule => _soundEffectsModule;
        public IAtmosphereModule AtmosphereModule => _atmosphereModule;


        public ChessAudioService(
            IAudioEngine engine,
            ISettingsService settingsService,
            ISoundEffectsModule soundEffectsModule,
            IAtmosphereModule atmosphereModule,
            IMoveModel moveModel,
            ICommunicationLayer backendCommunication)
        {
            _audioEngine = engine;
            _settingsService = settingsService;
            _soundEffectsModule = soundEffectsModule;
            _atmosphereModule = atmosphereModule;
            _moveModel = moveModel;
            _backendCommunication = backendCommunication;
        }


        public async Task InitializeAsync()
        {
            _audioEngine.RegisterModule(AudioModuleType.SFX, _soundEffectsModule);
            _audioEngine.RegisterModule(AudioModuleType.Atmosphere, _atmosphereModule);

            await _audioEngine.InitializeAsync();

            // Apply settings from the config
            await LoadAudioSettingsFromConfig();

            // Subscribe to the events of the game
            SubscribeToGameEvents();
        }


        // The sounds for clicks and selections in the UI are connected in App, where the view models are known
        private void SubscribeToGameEvents()
        {
            _moveModel.GameOverEvent += (EndGameState state, Side player) => _ = Task.Run(async () => await HandleEndGameStateAsync(state));
            _moveModel.ChesspieceSelected += () => _ = Task.Run(async () => await HandleUIInteractionAsync(UIInteraction.PieceSelect));

            _backendCommunication.MoveExecuted += (Move move, string notation) => _ = Task.Run(async () => await HandleMoveAsync(move));
        }


        private async Task LoadAudioSettingsFromConfig()
        {
            try
            {
                float masterVolume = _settingsService.AudioMasterVolume;
                float sfxVolume = _settingsService.AudioSFXVolume;
                float atmosVolume = _settingsService.AudioAtmosVolume;
                bool sfxEnabled = _settingsService.AudioSFXEnabled;
                bool atmosEnabled = _settingsService.AudioAtmosEnabled;
                string atmosScenario = _settingsService.AudioAtmosScenario;

                // Apply master volume to audio engine
                _audioEngine.SetMasterVolume(masterVolume);

                // Apply SFX settings
                _soundEffectsModule.SetModuleVolume(sfxVolume);
                _soundEffectsModule.IsEnabled = sfxEnabled;

                // Apply atmosphere settings
                _atmosphereModule.SetModuleVolume(atmosVolume);
                _atmosphereModule.IsEnabled = atmosEnabled;

                if (!string.IsNullOrEmpty(atmosScenario) &&
                    Enum.TryParse<AtmosphereScenario>(atmosScenario, out var scenario))
                {
                    await _atmosphereModule.SetAtmosphereAsync(scenario);   // Also triggers playback
                }

                Logger.LogInfo("Audio settings loaded and applied from configuration");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load audio settings from configuration: {ex.Message}");
                // Continue with default settings
            }
        }


        public async Task HandleMoveAsync(Move move)
        {
            bool isCapture = move.IsCapture;

            var sfx = DetermineMoveSound(move, isCapture);
            var volume = DetermineMoveVolume(move, isCapture);

            await PlaySoundAsync(sfx, volume);
        }


        public async Task HandleEndGameStateAsync(EndGameState gameState)
        {
            // For now we just have a SFX for CheckMate
            var sfx = gameState switch
            {
                EndGameState.Checkmate => SoundEffect.Checkmate,
                _ => SoundEffect.Checkmate
            };

            await PlaySoundAsync(sfx, 0.8f);
        }


        public async Task HandleGameStartAsync()
        {
            await PlaySoundAsync(SoundEffect.GameStart, 0.6f);
        }


        public async Task HandleUIInteractionAsync(UIInteraction interaction)
        {
            var soundEffect = interaction switch
            {
                UIInteraction.ButtonClick => SoundEffect.ButtonClick,
                UIInteraction.MenuOpen => SoundEffect.MenuOpen,
                UIInteraction.ItemSelected => SoundEffect.ItemSelected,
                UIInteraction.PieceMove => SoundEffect.PieceMove,
                UIInteraction.PieceSelect => SoundEffect.PieceSelected,
                _ => SoundEffect.ButtonClick
            };

            var volume = interaction == UIInteraction.PieceSelect ? 0.3f : 0.5f;
            await PlaySoundAsync(soundEffect, volume);
        }


        private Task PlaySoundAsync(SoundEffect effect, float volume)
        {
            return _soundEffectsModule.PlaySoundAsync(effect, volume);
        }


        public bool GetSFXEnabled()
        {
            return _soundEffectsModule.IsEnabled;
        }


        public void SetSFXEnabled(bool enabled)
        {
            if (_soundEffectsModule.IsEnabled == enabled) return;

            _soundEffectsModule.IsEnabled = enabled;

            _settingsService.AudioSFXEnabled = enabled;
        }


        public float GetSFXVolume()
        {
            return _soundEffectsModule.GetModuleVolume();
        }


        public void SetSFXVolume(float volume)
        {
            if (_soundEffectsModule.GetModuleVolume() == volume) return;

            _soundEffectsModule.SetModuleVolume(volume);
            _settingsService.AudioSFXVolume = volume;
        }


        public async Task SetAtmosphereAsync(AtmosphereScenario scenario)
        {
            if (_atmosphereModule.CurrentScenario == scenario) return;

            await _atmosphereModule.SetAtmosphereAsync(scenario);
            _settingsService.AudioAtmosScenario = scenario.ToString();
        }


        public AtmosphereScenario GetCurrentAtmosphere()
        {
            return _atmosphereModule.CurrentScenario;
        }


        public void StopAtmosphereAsync()
        {
            _atmosphereModule.StopAtmosphereAsync();
        }


        public void SetAtmosphereEnabled(bool enabled)
        {
            if (_atmosphereModule.IsEnabled == enabled) return;

            _atmosphereModule.IsEnabled = enabled;

            // If disabling, stop the current atmosphere playback
            if (!enabled)
            {
                StopAtmosphereAsync();
            }
            // If enabling and there's a selected scenario, start playing it
            else
            {
                var currentScrenario = _atmosphereModule.CurrentScenario;
                _ = Task.Run(async () => await _atmosphereModule.SetAtmosphereAsync(currentScrenario));
            }

            _settingsService.AudioAtmosEnabled = enabled;
        }


        public bool GetAtmosphereEnabled()
        {
            return _atmosphereModule.IsEnabled;
        }


        public float GetAtmosphereVolume()
        {
            return _atmosphereModule.GetModuleVolume();
        }


        public void SetAtmosphereVolume(float volume)
        {
            if (_atmosphereModule.GetModuleVolume() == volume) return;

            _atmosphereModule.SetModuleVolume(volume);
            _settingsService.AudioAtmosVolume = volume;
        }


        public void SetMasterVolume(float volume)
        {
            if (_audioEngine.GetMasterVolume() == volume) return;

            _audioEngine.SetMasterVolume(volume);
            _settingsService.AudioMasterVolume = volume;
        }


        public float GetMasterVolume()
        {
            return _audioEngine.GetMasterVolume();
        }


        private SoundEffect DetermineMoveSound(Move move, bool isCapture)
        {
            //if (move.type.HasFlag(MoveTypeInstance.MoveType_Checkmate))
            //    return SoundEffect.Checkmate;
            if (isCapture)
                return SoundEffect.PieceCapture;

            return SoundEffect.PieceMove;
        }


        private float DetermineMoveVolume(Move move, bool isCapture)
        {
            if (isCapture)
                return 0.8f;

            return 0.6f;
        }

    }
}
