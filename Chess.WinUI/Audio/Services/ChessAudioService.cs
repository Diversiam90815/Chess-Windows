using Chess.UI.Audio.Core;
using Chess.UI.Audio.Modules;
using Chess.UI.Moves;
using Chess.UI.Services;
using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Audio.Services
{
    public class ChessAudioService : IChessAudioService
    {
        private readonly IAudioEngine _audioEngine;
        private ISoundEffectsModule _soundEffectsModule;
        private IAtmosphereModule _atmosphereModule;

        public ISoundEffectsModule SoundEffectsModule => _soundEffectsModule;
        public IAtmosphereModule AtmosphereModule => _atmosphereModule;


        public ChessAudioService(IAudioEngine engine)
        {
            _audioEngine = engine;
        }


        public async Task InitializeAsync()
        {
            _soundEffectsModule = new SoundEffectsModule();
            _atmosphereModule = new AtmosphereModule();

            _audioEngine.RegisterModule(AudioModuleType.SFX, _soundEffectsModule);
            _audioEngine.RegisterModule(AudioModuleType.Atmosphere, _atmosphereModule);

            await _audioEngine.InitializeAsync();

            // Apply settings from the config
            await LoadAudioSettingsFromConfig();

            // Subscribe to the events from the viewmodels
            SubscribeToEvents();
        }


        private void SubscribeToEvents()
        {
            var mainMenuViewModel = App.Current.Services.GetService<MainMenuViewModel>();
            mainMenuViewModel.ButtonClicked += () => _ = Task.Run(async () => await HandleUIInteractionAsync(UIInteraction.ButtonClick));
            mainMenuViewModel.StartGameRequested += () => _ = Task.Run(async () => await HandleGameStartAsync());

            var chessboardVM = App.Current.Services.GetService<ChessBoardViewModel>();
            chessboardVM.ButtonClicked += () => _ = Task.Run(async () => await HandleUIInteractionAsync(UIInteraction.ButtonClick));

            var moveModel = App.Current.Services.GetService<IMoveModel>();
            moveModel.GameOverEvent += (EndGameState state, PlayerColor player) => _ = Task.Run(async () => await HandleEndGameStateAsync(state));
            moveModel.ChesspieceSelected += () => _ = Task.Run(async () => await HandleUIInteractionAsync(UIInteraction.PieceSelect));

            var backendCom = App.Current.ChessLogicCommunication;
            backendCom.MoveExecuted += (PossibleMoveInstance move) => _ = Task.Run(async () => await HandleMoveAsync(move));

            var themePreferences = App.Current.Services.GetService<StylesPreferencesViewModel>();
            themePreferences.ItemSelected += () => _ = Task.Run(async () => await HandleUIInteractionAsync(UIInteraction.ItemSelected));

            var multiplayerPreferences = App.Current.Services.GetService<MultiplayerPreferencesViewModel>();
            multiplayerPreferences.ItemSelected += () => _ = Task.Run(async () => await HandleUIInteractionAsync(UIInteraction.ItemSelected));

            var multiplayerVM = App.Current.Services.GetService<MultiplayerViewModel>();
            multiplayerVM.ButtonClicked += () => _ = Task.Run(async () => await HandleUIInteractionAsync(UIInteraction.ButtonClick));

            var audioPref = App.Current.Services.GetService<AudioPreferencesViewModel>();
            audioPref.ItemSelected += () => _ = Task.Run(async () => await HandleUIInteractionAsync(UIInteraction.ItemSelected));
        }


        private async Task LoadAudioSettingsFromConfig()
        {
            try
            {
                // Load settings from the backend/UserSettings via EngineAPI
                float masterVolume = EngineAPI.GetMasterVolume();
                float sfxVolume = EngineAPI.GetSFXVolume();
                float atmosVolume = EngineAPI.GetAtmosVolume();
                bool sfxEnabled = EngineAPI.GetSFXEnabled();
                bool atmosEnabled = EngineAPI.GetAtmosEnabled();
                string atmosScenario = EngineAPI.GetAtmosScenario();

                // Apply master volume to audio engine
                _audioEngine.SetMasterVolume(masterVolume);

                // Apply SFX settings
                if (_soundEffectsModule != null)
                {
                    _soundEffectsModule.SetModuleVolume(sfxVolume);
                    _soundEffectsModule.IsEnabled = sfxEnabled;
                }

                // Apply atmosphere settings
                if (_atmosphereModule != null)
                {
                    _atmosphereModule.SetModuleVolume(atmosVolume);
                    _atmosphereModule.IsEnabled = atmosEnabled;

                    if (!string.IsNullOrEmpty(atmosScenario) &&
                        Enum.TryParse<AtmosphereScenario>(atmosScenario, out var scenario))
                    {
                        await _atmosphereModule.SetAtmosphereAsync(scenario);   // Also triggers playback
                    }
                }

                Logger.LogInfo("Audio settings loaded and applied from configuration");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load audio settings from configuration: {ex.Message}");
                // Continue with default settings
            }
        }


        public async Task HandleMoveAsync(PossibleMoveInstance move)
        {
            bool isCapture = move.type.HasFlag(MoveTypeInstance.MoveType_Capture);

            var sfx = DetermineMoveSound(move, isCapture);
            var volume = DetermineMoveVolume(move, isCapture);

            await _soundEffectsModule?.PlaySoundAsync(sfx, volume);
        }


        public async Task HandleEndGameStateAsync(EndGameState gameState)
        {
            // For now we just have a SFX for CheckMate
            var sfx = gameState switch
            {
                EndGameState.Checkmate => SoundEffect.Checkmate,
                _ => SoundEffect.Checkmate
            };

            await _soundEffectsModule?.PlaySoundAsync(sfx, 0.8f);
        }


        public async Task HandleGameStartAsync()
        {
            await _soundEffectsModule?.PlaySoundAsync(SoundEffect.GameStart, 0.6f);
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
            await _soundEffectsModule?.PlaySoundAsync(soundEffect, volume);
        }


        public bool GetSFXEnabled()
        {
            if (_soundEffectsModule == null) return false;
            return _soundEffectsModule.IsEnabled;
        }


        public void SetSFXEnabled(bool enabled)
        {
            if (_soundEffectsModule == null) return;
            if (_soundEffectsModule.IsEnabled == enabled) return;

            _soundEffectsModule.IsEnabled = enabled;

            EngineAPI.SetSFXEnabled(enabled);
        }


        public float GetSFXVolume()
        {
            if (_soundEffectsModule == null) return 0.0f;
            return _soundEffectsModule.GetModuleVolume();
        }


        public void SetSFXVolume(float volume)
        {
            if (_soundEffectsModule.GetModuleVolume() == volume) return;

            _soundEffectsModule?.SetModuleVolume(volume);
            EngineAPI.SetSFXVolume(volume);
        }


        public async Task SetAtmosphereAsync(AtmosphereScenario scenario)
        {
            if (_atmosphereModule?.CurrentScenario == scenario) return;

            await _atmosphereModule?.SetAtmosphereAsync(scenario);
            EngineAPI.SetAtmosScenario(scenario.ToString());
        }


        public AtmosphereScenario GetCurrentAtmosphere()
        {
            return _atmosphereModule.CurrentScenario;
        }


        public void StopAtmosphereAsync()
        {
            _atmosphereModule?.StopAtmosphereAsync();
        }


        public void SetAtmosphereEnabled(bool enabled)
        {
            if (_atmosphereModule == null) return;
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

            EngineAPI.SetAtmosEnabled(enabled);
        }


        public bool GetAtmosphereEnabled()
        {
            if (_atmosphereModule == null) return false;
            return _atmosphereModule.IsEnabled;
        }


        public float GetAtmosphereVolume()
        {
            if (_atmosphereModule == null) return 0.0f;
            return _atmosphereModule.GetModuleVolume();
        }


        public void SetAtmosphereVolume(float volume)
        {
            if (_atmosphereModule?.GetModuleVolume() == volume) return;

            _atmosphereModule?.SetModuleVolume(volume);
            EngineAPI.SetAtmosVolume(volume);
        }


        public void SetMasterVolume(float volume)
        {
            if (_audioEngine.GetMasterVolume() == volume) return;

            _audioEngine.SetMasterVolume(volume);
            EngineAPI.SetMasterVolume(volume);
        }


        public float GetMasterVolume()
        {
            return _audioEngine.GetMasterVolume();
        }


        private SoundEffect DetermineMoveSound(PossibleMoveInstance move, bool isCapture)
        {
            if (move.type.HasFlag(MoveTypeInstance.MoveType_Checkmate))
                return SoundEffect.Checkmate;
            if (isCapture)
                return SoundEffect.PieceCapture;

            return SoundEffect.PieceMove;
        }


        private float DetermineMoveVolume(PossibleMoveInstance move, bool isCapture)
        {
            if (move.type.HasFlag(MoveTypeInstance.MoveType_Checkmate))
                return 0.9f;
            if (move.type.HasFlag(MoveTypeInstance.MoveType_Check))
                return 0.7f;
            if (isCapture)
                return 0.6f;

            return 0.5f;
        }

    }
}
