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
        Task<bool> ResetGameAsync();
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


    /// <summary>
    /// Sends the lifecycle requests to the engine and reports them once the engine has confirmed them
    /// through its state change event.
    /// </summary>
    public class ChessGameService : IChessGameService
    {
        private readonly ICommunicationLayer _backendCommunication;
        private GameConfiguration? _currentConfiguration;
        private bool _isGameActive;

        // The engine starts out in its initial state without reporting it
        private GamePhase _enginePhase = GamePhase.Initializing;

        // Requests the engine has not confirmed yet
        private GameConfiguration? _requestedConfiguration;
        private TaskCompletionSource<bool> _startRequest;
        private TaskCompletionSource<bool> _resetRequest;

        public event Action GameStarted;
        public event Action GameEnded;
        public event Action GameReset;
        public event Action<GameConfiguration> ConfigurationChanged;

        public bool IsGameActive => _isGameActive;
        public bool IsMultiplayerGame => _currentConfiguration?.Mode == GameModeSelection.MultiPlayer;
        public GameConfiguration? CurrentConfiguration => _currentConfiguration;


        public ChessGameService(ICommunicationLayer backendCommunication)
        {
            _backendCommunication = backendCommunication;
            _backendCommunication.GameStateChanged += OnEngineStateChanged;
        }


        public Task<bool> StartGameAsync(GameConfiguration config)
        {
            // A newer request replaces one the engine has not confirmed
            Complete(ref _startRequest, false);

            _startRequest = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _requestedConfiguration = config;

            // The engine only starts a game from its initial state. A running game is reset first,
            // the start then follows when the engine reports that state.
            if (_enginePhase == GamePhase.Initializing)
                _backendCommunication.StartGame(config);
            else
                _backendCommunication.ResetGame();

            return _startRequest.Task;
        }


        public Task<bool> ResetGameAsync()
        {
            if (_enginePhase == GamePhase.Initializing)
            {
                OnGameReset();
                return Task.FromResult(true);
            }

            _resetRequest ??= new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _backendCommunication.ResetGame();

            return _resetRequest.Task;
        }


        public Task EndGameAsync()
        {
            // A start the engine has not confirmed must not follow the reset below
            _requestedConfiguration = null;
            Complete(ref _startRequest, false);

            _isGameActive = false;
            _currentConfiguration = null;

            Logger.LogInfo("Game ended");

            GameEnded?.Invoke();

            // Leaves the engine ready for the next game. GameReset is raised once it has confirmed.
            if (_enginePhase != GamePhase.Initializing)
                _backendCommunication.ResetGame();

            return Task.CompletedTask;
        }


        private void OnEngineStateChanged(GamePhase phase)
        {
            GamePhase previousPhase = _enginePhase;
            _enginePhase = phase;

            if (phase == GamePhase.Initializing)
            {
                OnGameReset();

                if (_requestedConfiguration.HasValue)
                    _backendCommunication.StartGame(_requestedConfiguration.Value);
            }
            else if (previousPhase == GamePhase.Initializing)
            {
                OnGameStarted();
            }
        }


        private void OnGameReset()
        {
            _isGameActive = false;

            Logger.LogInfo("Game reset");

            GameReset?.Invoke();
            Complete(ref _resetRequest, true);
        }


        private void OnGameStarted()
        {
            if (_requestedConfiguration.HasValue)
            {
                _currentConfiguration = _requestedConfiguration;
                _requestedConfiguration = null;

                ConfigurationChanged?.Invoke(_currentConfiguration.Value);
            }

            _isGameActive = true;

            Logger.LogInfo($"Game started: Mode={_currentConfiguration?.Mode}, Player={_currentConfiguration?.PlayerColor}");

            GameStarted?.Invoke();
            Complete(ref _startRequest, true);
        }


        private static void Complete(ref TaskCompletionSource<bool> request, bool result)
        {
            request?.TrySetResult(result);
            request = null;
        }

    }
}
