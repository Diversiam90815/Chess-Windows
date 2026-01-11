using Chess.UI.Services;
using Chess.UI.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.ViewModels
{
    public class GameSetupViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IDispatcherQueueWrapper _dispatcherQueue;

        private readonly IGameConfigurationService _configurationService;


        public GameSetupViewModel(IDispatcherQueueWrapper dispatcher)
        {
            _dispatcherQueue = dispatcher;

            _configurationService = App.Current.Services.GetService<IGameConfigurationService>();
        }


        public void Reset()
        {
            CPUDifficulty = CPUDifficulty.None;
            PlayerColor = Side.None;

            PlayerConfigVisible = true;
            CPUConfigVisible = false;
        }


        public async void LocalCoopInitiated()
        {
            GameMode = GameModeSelection.LocalCoop;

            var config = GameConfiguration.CreateLocalCoop();
            await StartGameAsync(config);
        }



        public void CPUGameInitiated()
        {
            GameMode = GameModeSelection.SinglePlayer;

            PlayerConfigVisible = false;
            CPUConfigVisible = true;
        }


        public async void StartSinglePlayerGameAsnyc()
        {
            if (!CanStartSinglePlayerGame)
                return;

            var config = GameConfiguration.CreateSinglePlayer(PlayerColor, CPUDifficulty);
            await StartGameAsync(config);
        }



        public async Task<bool> StartGameAsync(GameConfiguration configuration)
        {
            try
            {
                bool success = await _configurationService.StartGameAsync(configuration);

                if (success)
                    Reset();

                return success;
            }
            catch(Exception ex)
            {
                Logger.LogError($"Failed to start game: {ex.Message} ");
                return false;
            }
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


        public bool CanStartSinglePlayerGame => (CPUDifficulty != CPUDifficulty.None && PlayerColor != Side.None);


        private Side _playerColor = Side.None;
        public Side PlayerColor
        {
            get => _playerColor;
            set
            {
                if (value != _playerColor)
                {
                    _playerColor = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanStartSinglePlayerGame));
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
                    OnPropertyChanged(nameof(CanStartSinglePlayerGame));
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
