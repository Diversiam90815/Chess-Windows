using Chess.UI.Wrappers;
using System;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Services
{
    /// <summary>
    /// Core service responsible for managing the chess game lifecycle and state.
    /// </summary>
    public interface IChessGameService
    {
        // Game lifecycle
        Task<bool> StartGameAsync(GameConfiguration config);
        Task ResetGameAsync();
        Task EndGameAsync();

        // Game State Queries
        bool IsGameActive { get; }
        bool IsMultiplayerGame { get; }
        GameConfiguration? CurrentConfiguration { get; }


        // Events
        event Action GameStarted;
        event Action GameEnded;
        event Action GameReset;
        event Action<GameConfiguration> ConfigurationChanged;
    }


    public class ChessGameService : IChessGameService
    {
        private readonly IDispatcherQueueWrapper _dispatcherQueue;
        private GameConfiguration? _currentConfiguration;
        private bool _isGameActive;

        public event Action GameStarted;
        public event Action GameEnded;
        public event Action GameReset;
        public event Action<GameConfiguration> ConfigurationChanged;

        public bool IsGameActive => _isGameActive;
        public bool IsMultiplayerGame => _currentConfiguration?.Mode == GameModeSelection.MultiPlayer;
        public GameConfiguration? CurrentConfiguration => _currentConfiguration;


        public ChessGameService(IDispatcherQueueWrapper dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;
        }

        public async Task<bool> StartGameAsync(GameConfiguration config)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _currentConfiguration = config;

                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        EngineAPI.StartGame(config);
                        _isGameActive = true;

                        Logger.LogInfo($"Game started: Mode={config.Mode}, Player={config.PlayerColor}");

                        ConfigurationChanged?.Invoke(config);
                        GameStarted?.Invoke();
                    });

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to start game: {ex.Message}");
                    return false;
                }
            });
        }


        public async Task ResetGameAsync()
        {
            await Task.Run(() =>
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    EngineAPI.ResetGame();
                    _isGameActive = false;

                    Logger.LogInfo("Game reset");

                    GameReset?.Invoke();
                });
            });
        }


        public async Task EndGameAsync()
        {
            await Task.Run(() =>
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    _isGameActive = false;
                    _currentConfiguration = null;

                    Logger.LogInfo("Game ended");

                    GameEnded?.Invoke();
                });
            });
        }

    }
}
