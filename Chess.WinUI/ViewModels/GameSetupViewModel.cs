using Chess.UI.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.ViewModels
{
    public interface IGameSetupViewModel
    {
        Task<bool> StartGameAsync(GameConfiguration config);
        Task<bool> StartLocalCoopGameAsync();
        Task<bool> StartSinglePlayerGameAsync(Side playerColor, CPUDifficulty difficulty);
        Task<bool> StartMultiplayerGameAsync(Side playerColor);
        Task ResetCurrentGameAsync(); 
        
        // UI state
        void Reset();
        bool PlayerConfigVisible { get; set; }
        bool CPUConfigVisible { get; set; }
        Side PlayerColor { get; set; }
        CPUDifficulty CPUDifficulty { get; set; }

        event PropertyChangedEventHandler PropertyChanged;
    }


    public class GameSetupViewModel : IGameSetupViewModel
    {
        private readonly IChessGameService _gameService;
        private readonly INavigationService _navigationService;

        public event PropertyChangedEventHandler PropertyChanged;


        public GameSetupViewModel(IChessGameService gameService, INavigationService navigationService)
        {
            _gameService = gameService;
            _navigationService = navigationService;
        }


        public async Task<bool> StartGameAsync(GameConfiguration config)
        {
            if(config.IsValid())
            {
                Logger.LogError($"Invalid game configuration for mode: {config.Mode}");
                return false;
            }

            // delegate to specific setup method on mode
            return config.Mode switch
            {
                GameModeSelection.LocalCoop => await StartLocalCoopGameAsync(),
                GameModeSelection.SinglePlayer => await StartSinglePlayerGameAsync(config.PlayerColor, config.CpuDifficulty),
                GameModeSelection.MultiPlayer => await StartMultiplayerGameAsync(config.PlayerColor),
                _ => throw new NotSupportedException($"Game mode not supported: {config.Mode}")
            };
        }


        public async Task<bool> StartLocalCoopGameAsync()
        {
            var config = GameConfiguration.CreateLocalCoop();
            return await StartGameWithConfigAsync(config);
        }


        public async Task<bool> StartSinglePlayerGameAsync(Side playerColor, CPUDifficulty difficulty)
        {
            if (difficulty == CPUDifficulty.None)
            {
                Logger.LogError("CPU difficulty must be set for single-player mode");
                return false;
            }

            var config = GameConfiguration.CreateSinglePlayer(playerColor, difficulty);
            return await StartGameWithConfigAsync(config);
        }


        /// <summary>
        /// Starts a single-player game using the current UI state
        /// </summary>
        public async Task<bool> StartSinglePlayerWithCurrentStateAsync()
        {
            return await StartSinglePlayerGameAsync(PlayerColor, CPUDifficulty);
        }


        public async Task<bool> StartMultiplayerGameAsync(Side playerColor)
        {
            var config = GameConfiguration.CreateMultiplayer(playerColor);
            return await StartGameWithConfigAsync(config);
        }


        private async Task<bool> StartGameWithConfigAsync(GameConfiguration config)
        {
            try
            {
                Logger.LogInfo($"Starting game: Mode = {config.Mode}, Player = {config.PlayerColor}, Difficulty = {config.CpuDifficulty}");

                // Start game through service
                bool gameStarted = await _gameService.StartGameAsync(config);

                if(!gameStarted)
                {
                    Logger.LogError("Failed to start game!");
                    return false;
                }

                await _navigationService.NavigateToChessboardAsync();

                Logger.LogInfo("Successfully navigated to chessboard");
                return true;
            }
            catch(Exception ex)
            {
                Logger.LogError($"Exception occured : {ex.Message}");
                return false;
            }
        }


        public async Task ResetCurrentGameAsync()
        {
            await _gameService.ResetGameAsync();
        }


        #region UI State Properties

        private bool _playerConfigVisible;
        public bool PlayerConfigVisible
        {
            get => _playerConfigVisible;
            set
            {
                if (_playerConfigVisible != value)
                {
                    _playerConfigVisible = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _cpuConfigVisible;
        public bool CPUConfigVisible
        {
            get => _cpuConfigVisible;
            set
            {
                if (_cpuConfigVisible != value)
                {
                    _cpuConfigVisible = value;
                    OnPropertyChanged();
                }
            }
        }


        private Side _playerColor;
        public Side PlayerColor
        {
            get => _playerColor;
            set
            {
                if (_playerColor != value)
                {
                    _playerColor = value;
                    OnPropertyChanged();

                    // When player selects a color, show CPU difficulty selection
                    if (value != Side.None)
                    {
                        PlayerConfigVisible = false;
                        CPUConfigVisible = true;
                    }
                }
            }
        }


        private CPUDifficulty _cpuDifficulty;
        public CPUDifficulty CPUDifficulty
        {
            get => _cpuDifficulty;
            set
            {
                if (_cpuDifficulty != value)
                {
                    _cpuDifficulty = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <summary>
        /// Resets the UI state to initial configuration
        /// </summary>
        public void Reset()
        {
            PlayerConfigVisible = true;
            CPUConfigVisible = false;
            PlayerColor = Side.None;
            CPUDifficulty = CPUDifficulty.None;
        }


        #endregion

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
