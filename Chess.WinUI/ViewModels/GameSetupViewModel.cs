using Chess.UI.Services;
using Chess.UI.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.ViewModels
{
    public class GameSetupViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IGameConfigurationBuilder _configuration;

        private readonly IDispatcherQueueWrapper _dispatcherQueue;

        private readonly IGameConfigurationService _configurationService;


        public GameSetupViewModel(IDispatcherQueueWrapper dispatcher)
        {
            _dispatcherQueue = dispatcher;

            _configuration = App.Current.Services.GetService<IGameConfigurationBuilder>();
            _configurationService = App.Current.Services.GetService<IGameConfigurationService>();
        }


        public void Reset()
        {
            _configuration.Reset();

            CPUDifficulty = CPUDifficulty.None;
            PlayerColor = PlayerColor.NoColor;

            PlayerConfigVisible = true;
            CPUConfigVisible = false;
        }


        public void LocalCoopInitiated()
        {
            GameMode = GameModeSelection.LocalCoop;

            StartGameAsync();
        }


        public void CPUGameInitiated()
        {
            GameMode = GameModeSelection.VsCPU;

            PlayerConfigVisible = false;
            CPUConfigVisible = true;
        }


        public async void StartGameAsync()
        {
            // Set the values 
            _configuration.SetGameMode(GameMode);

            if (_gameMode == GameModeSelection.VsCPU)
            {
                _configuration.SetPlayerColor(PlayerColor);
                _configuration.SetCPUDifficulty(CPUDifficulty);
            }

            var config = _configuration.GetConfiguration();

            await _configurationService.StartGameAsync(config);

            Reset();
        }


        private GameModeSelection _gameMode;
        public GameModeSelection GameMode
        {
            get => _gameMode;
            set
            {
                if (_gameMode != value)
                {
                    _gameMode = value;
                    OnPropertyChanged();
                }
            }
        }


        // Game Config Menu : CPU Configuration
        private bool _cpuConfigVisible = false;
        public bool CPUConfigVisible
        {
            get => _cpuConfigVisible;
            set
            {
                if (value != _cpuConfigVisible)
                {
                    _cpuConfigVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        // Game Config Menu : CPU or Coop Game
        private bool _playerConfigVisible = true;
        public bool PlayerConfigVisible
        {
            get => _playerConfigVisible;
            set
            {
                if (value != _playerConfigVisible)
                {
                    _playerConfigVisible = value;
                    OnPropertyChanged();
                }
            }
        }


        public bool StartGameButtonVisible => (CPUDifficulty != CPUDifficulty.None && PlayerColor != PlayerColor.NoColor);


        private PlayerColor _playerColor = PlayerColor.NoColor;
        public PlayerColor PlayerColor
        {
            get => _playerColor;
            set
            {
                if (value != _playerColor)
                {
                    _playerColor = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StartGameButtonVisible));
                }
            }
        }


        private CPUDifficulty _cpuDifficulty = CPUDifficulty.None;
        public CPUDifficulty CPUDifficulty
        {
            get => _cpuDifficulty;
            set
            {
                if (value != _cpuDifficulty)
                {
                    _cpuDifficulty = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StartGameButtonVisible));
                }
            }
        }


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }
}
