using Chess.UI.Audio.Core;
using Chess.UI.Services;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;


namespace Chess.UI.Audio.Modules
{
    public enum AtmosphereScenario
    {
        Fireplace,
        Forest,
    }


    public class AtmosphereChangedEventArgs : EventArgs
    {
        public AtmosphereScenario Scenario { get; }
        public float Volume { get; }
        public DateTime ChangeTime { get; }

        public AtmosphereChangedEventArgs(AtmosphereScenario scenario, float volume)
        {
            Scenario = scenario;
            Volume = volume;
            ChangeTime = DateTime.Now;
        }
    }


    public interface IAtmosphereModule : IAudioModule
    {
        Task SetAtmosphereAsync(AtmosphereScenario scenario);
        void StopAtmosphereAsync();
        void SetCrossfadeDuration(float seconds);

        AtmosphereScenario CurrentScenario { get; }
        bool IsPlaying { get; }
        bool IsEnabled { get; set; }

        event EventHandler<AtmosphereChangedEventArgs> AtmosphereChanged;
    }


    public class AtmosphereModule : IAtmosphereModule, IDisposable
    {
        private readonly ConcurrentDictionary<AtmosphereScenario, string> _atmosFilePaths;
        private MediaPlayer _currentPlayer;
        private MediaPlayer _crossfadePlayer;
        private readonly object _playerLock = new();

        private bool _disposed = false;

        private float _moduleVolume = 1.0f;
        private float _masterVolume = 1.0f;
        private float _crossfadeDuration = 2.0f; // seconds
        private bool _isInitialized = false;

        // IAudioModule Properties
        public string ModuleName => "Atmosphere";
        public AudioModuleType ModuleType => AudioModuleType.Atmosphere;

        // IAtmosphereModule Properties
        public AtmosphereScenario CurrentScenario { get; private set; } = AtmosphereScenario.Forest;
        public bool IsPlaying => !_disposed && _currentPlayer?.PlaybackSession?.PlaybackState == MediaPlaybackState.Playing;
        public bool IsEnabled { get; set; } = true;

        // Events
        public event EventHandler<AudioModuleEventArgs> StatusChanged;
        public event EventHandler<AtmosphereChangedEventArgs> AtmosphereChanged;


        public AtmosphereModule()
        {
            _atmosFilePaths = new ConcurrentDictionary<AtmosphereScenario, string>();
        }


        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        PreloadAtmosphereTracksAsync();
                        StatusChanged?.Invoke(this, new AudioModuleEventArgs(ModuleName, "Preloading completed"));
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Background preloading failed: {ex.Message}");
                    }
                });

                InitializeMediaPlayers();

                _isInitialized = true;
                StatusChanged?.Invoke(this, new AudioModuleEventArgs(ModuleName, "Initialized"));
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, new AudioModuleEventArgs(ModuleName, $"Failed to initialize: {ex.Message}"));
                throw;
            }
        }


        // Volume Management - IAudioModule Implementation
        public void SetModuleVolume(float volume)
        {
            _moduleVolume = Math.Clamp(volume, 0.0f, 1.0f);
            UpdateCurrentPlayerVolume();
        }


        public float GetModuleVolume() => _moduleVolume;


        public void SetMasterVolume(float masterVolume)
        {
            _masterVolume = Math.Clamp(masterVolume, 0.0f, 1.0f);
            UpdateCurrentPlayerVolume();
        }


        public float GetEffectiveVolume() => _moduleVolume * _masterVolume;


        public void SetCrossfadeDuration(float seconds)
        {
            _crossfadeDuration = Math.Max(0.1f, seconds);
        }


        public async Task SetAtmosphereAsync(AtmosphereScenario scenario)
        {
            if (!_isInitialized || _disposed) return;

            try
            {
                CurrentScenario = scenario;

                lock (_playerLock)
                {
                    // If module is disabled, stop any current playback
                    if (!IsEnabled)
                    {
                        StopAtmosphereAsync();
                        return;
                    }
                }

                var filePath = GetAtmosphereTrackPath(scenario);
                if (!File.Exists(filePath))
                {
                    Logger.LogWarning($"Atmosphere track not found: {filePath}");
                    return;
                }

                var mediaSource = await CreateMediaSourceAsync(filePath);
                if (mediaSource == null) return;

                float volume = GetEffectiveVolume();

                lock (_playerLock)
                {
                    if (_currentPlayer?.PlaybackSession?.PlaybackState == MediaPlaybackState.Playing)
                    {
                        //Start crossfade
                        StartCrossfade(mediaSource, volume);
                    }
                    else
                    {
                        // Start direct play
                        StartDirectPlayback(mediaSource, volume);
                    }
                }

                CurrentScenario = scenario;
                AtmosphereChanged?.Invoke(this, new AtmosphereChangedEventArgs(scenario, volume));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to set atmosphere {scenario}: {ex.Message}");
                StatusChanged?.Invoke(this, new AudioModuleEventArgs(ModuleName, $"Failed to set atmosphere {scenario}: {ex.Message}"));
            }
        }


        public void StopAtmosphereAsync()
        {
            if (!_isInitialized) return;

            lock (_playerLock)
            {
                _currentPlayer?.Pause();
                _crossfadePlayer?.Pause();
            }

            AtmosphereChanged?.Invoke(this, new AtmosphereChangedEventArgs(CurrentScenario, 0.0f));
        }


        private void PreloadAtmosphereTracksAsync()
        {
            foreach (AtmosphereScenario scenario in Enum.GetValues<AtmosphereScenario>())
            {
                LoadAtmosphereTrackAsync(scenario);
            }
        }


        private async Task<MediaSource> CreateMediaSourceAsync(string filePath)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(filePath);
                return MediaSource.CreateFromStorageFile(file);
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Failed to create media source from {filePath}: {ex.Message}");
                return null;
            }
        }


        private void LoadAtmosphereTrackAsync(AtmosphereScenario scenario)
        {
            try
            {
                var filePath = GetAtmosphereTrackPath(scenario);
                if (!File.Exists(filePath))
                {
                    Logger.LogWarning($"Atmosphere track not found: {filePath}");
                    return;
                }

                _atmosFilePaths.TryAdd(scenario, filePath);
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Failed to load atmosphere track {scenario}: {ex.Message}");
            }
        }


        private void InitializeMediaPlayers()
        {
            _currentPlayer = new MediaPlayer
            {
                AudioCategory = MediaPlayerAudioCategory.GameMedia,
                IsLoopingEnabled = true
            };
            _currentPlayer.MediaEnded += OnMediaPlayerEnded;
            _currentPlayer.MediaFailed += OnMediaPlayerFailed;

            _crossfadePlayer = new MediaPlayer
            {
                AudioCategory = MediaPlayerAudioCategory.GameMedia,
                IsLoopingEnabled = true
            };
            _crossfadePlayer.MediaEnded += OnCrossfadePlayerEnded;
            _crossfadePlayer.MediaFailed += OnMediaPlayerFailed;
        }


        private void StartDirectPlayback(MediaSource mediaSource, float volume)
        {
            try
            {
                if (_currentPlayer == null) return;

                _currentPlayer.Source = mediaSource;
                _currentPlayer.Volume = Math.Clamp(volume, 0.0f, 1.0f);
                _currentPlayer.Play();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error in direct playback: {ex.Message}");
            }
        }


        private void StartCrossfade(MediaSource mediaSource, float targetVolume)
        {
            try
            {
                // Setup crossfade player
                if (_crossfadePlayer is null)
                {
                    _crossfadePlayer = new MediaPlayer
                    {
                        AudioCategory = MediaPlayerAudioCategory.GameMedia,
                        IsLoopingEnabled = true
                    };
                    _crossfadePlayer.MediaEnded += OnCrossfadePlayerEnded;
                    _crossfadePlayer.MediaFailed += OnMediaPlayerFailed;
                }

                _crossfadePlayer.Source = mediaSource;
                _crossfadePlayer.Volume = 0.0f;
                _crossfadePlayer.Play();

                // Start crossfade animation
                Task.Run(async () => await PerformCrossfade(targetVolume));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error starting crossfade: {ex.Message}");
            }
        }


        private void UpdateCurrentPlayerVolume()
        {
            lock (_playerLock)
            {
                try
                {
                    if (_currentPlayer == null && !_disposed)
                        return;

                    if (IsEnabled)
                    {
                        _currentPlayer.Volume = Math.Clamp(GetEffectiveVolume(), 0.0f, 1.0f);
                    }
                    else
                    {
                        _currentPlayer.Volume = 0.0f;
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error updating player volume: {ex.Message}");
                }
            }
        }


        private async Task PerformCrossfade(float targetVolume)
        {
            int steps = (int)(_crossfadeDuration * 10);
            int stepDelay = (int)((_crossfadeDuration * 1000) / steps);
            float effectiveVolume = targetVolume * GetEffectiveVolume();

            for (int i = 0; i <= steps; i++)
            {
                float progress = (float)i / steps;

                lock (_playerLock)
                {
                    if (_currentPlayer != null)
                        _currentPlayer.Volume = Math.Clamp(effectiveVolume * (1.0 - progress), 0.0f, 1.0f);

                    if (_crossfadePlayer != null)
                        _crossfadePlayer.Volume = Math.Clamp(effectiveVolume * progress, 0.0f, 1.0f);
                }

                await Task.Delay(stepDelay);
            }

            // Swap the players
            lock (_playerLock)
            {
                var tmp = _currentPlayer;
                _currentPlayer = _crossfadePlayer;
                _crossfadePlayer = tmp;

                if (_crossfadePlayer != null)
                {
                    _crossfadePlayer.Pause();
                    _crossfadePlayer.Source = null;
                }
                _crossfadePlayer = null;
            }
        }


        private void OnMediaPlayerEnded(MediaPlayer sender, object args)
        {
            // Should not happen with looping enabled, but just in case
            Logger.LogWarning("Atmosphere track ended unexpectedly");
        }


        private void OnCrossfadePlayerEnded(MediaPlayer sender, object args)
        {
            // Crossfade player ended - should not happen during normal operation
        }


        private void OnMediaPlayerFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            Logger.LogError($"Atmosphere player failed: {args.ErrorMessage}");
            StatusChanged?.Invoke(this, new AudioModuleEventArgs(ModuleName, $"Playback failed: {args.ErrorMessage}"));
        }


        private string GetAtmosphereTrackPath(AtmosphereScenario scenario)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "Assets", "Audio", "Atmosphere", $"{scenario}.wav");
        }


        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _isInitialized = false;

            lock (_playerLock)
            {
                try
                {
                    if (_currentPlayer != null)
                    {
                        _currentPlayer.MediaEnded -= OnMediaPlayerEnded;
                        _currentPlayer.MediaFailed -= OnMediaPlayerFailed;
                        _currentPlayer.Source = null;
                        _currentPlayer.Dispose();
                        _currentPlayer = null;
                    }

                    if (_crossfadePlayer != null)
                    {
                        _crossfadePlayer.MediaEnded -= OnCrossfadePlayerEnded;
                        _crossfadePlayer.MediaFailed -= OnMediaPlayerFailed;
                        _crossfadePlayer.Source = null;
                        _crossfadePlayer.Dispose();
                        _crossfadePlayer = null;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error disposing atmosphere module: {ex.Message}");
                }
            }

            _atmosFilePaths.Clear();
        }
    }
}
