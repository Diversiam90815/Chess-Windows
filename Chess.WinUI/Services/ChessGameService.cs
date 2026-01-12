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

        public Task<bool> StartGameAsync(GameConfiguration config)
        {
            // TODO
        }


        public Task ResetGameAsync()
        {
            // TODO
        }


        public Task EndGameAsync()
        {
            // TODO
        }

    }
}
