using Chess.UI.ViewModels;
using Chess.UI.Views;
using Chess.UI.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Services
{
    public interface INavigationService
    {
        Task NavigateToGameConfigurationView();
        Task<bool> NavigateToChessboardAsync(bool isMutiplayer, GameConfiguration? config = null);
        Task<bool> NavigateToMultiplayerAsync();
        Task<bool> NavigateToMainMenuAsync();
        Task ShowPreferencesAsync();
        void CloseChessboard();
    }


    public class NavigationService : INavigationService
    {
        private readonly IDispatcherQueueWrapper _dispatcherQueue;

        private GameConfigurationView _gameConfigurationView;
        private ChessBoardWindow _chessBoardWindow;
        private MultiplayerWindow _multiplayerWindow;
        private MainMenuWindow _mainMenuWindow;

        private bool _multiplayerWindowClosedProgrammatically = false;
        private bool _configurationWindowClosedProgrammatically = false;


        public NavigationService(IDispatcherQueueWrapper dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;
        }


        public void SetMainMenuWindow(MainMenuWindow mainMenuWindow)
        {
            _mainMenuWindow = mainMenuWindow;
        }


        public async Task NavigateToGameConfigurationView()
        {
            await Task.Run(() =>
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    if (_gameConfigurationView == null)
                    {
                        _gameConfigurationView = App.Current.Services.GetService<GameConfigurationView>();
                        _gameConfigurationView.Activate();
                        _gameConfigurationView.Closed += OnGameConfigWindowClosed;

                        _mainMenuWindow?.AppWindow.Hide();
                    }
                });
            });
        }


        public async Task<bool> NavigateToChessboardAsync(bool isMultiplayer, GameConfiguration? config = null)
        {
            return await Task.Run(() =>
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    if (_chessBoardWindow == null)
                    {
                        _chessBoardWindow = App.Current.Services.GetService<ChessBoardWindow>();
                        _chessBoardWindow.Activate();
                        _chessBoardWindow.Closed += OnChessBoardWindowClosed;
                        _mainMenuWindow?.AppWindow.Hide();

                        var chessBoardViewModel = App.Current.Services.GetService<ChessBoardViewModel>();
                        chessBoardViewModel.IsMultiplayerGame = isMultiplayer;

                        // Close multiplayer window when opening chessboard from multiplayer
                        if (isMultiplayer && _multiplayerWindow != null)
                        {
                            _multiplayerWindowClosedProgrammatically = true;
                            _multiplayerWindow.Close();
                        }

                        // Close Game Configuration Window when opening chessboard
                        if (_gameConfigurationView != null)
                        {
                            _configurationWindowClosedProgrammatically = true;
                            _gameConfigurationView.Close();
                        }

                        if (!isMultiplayer && config.HasValue)
                        {
                            chessBoardViewModel.StartGame(config.Value);
                        }
                    }
                    else
                    {
                        _chessBoardWindow.Activate();
                    }
                });
                return true;
            });
        }


        public async Task<bool> NavigateToMultiplayerAsync()
        {
            return await Task.Run(() =>
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    if (_multiplayerWindow == null)
                    {
                        _multiplayerWindow = App.Current.Services.GetService<MultiplayerWindow>();
                        _multiplayerWindow.Activate();
                        _multiplayerWindow.Closed += OnMultiplayerWindowClosed;
                        _mainMenuWindow?.AppWindow.Hide();
                    }
                    else
                    {
                        _multiplayerWindow.Activate();
                    }
                });
                return true;
            });
        }


        public async Task<bool> NavigateToMainMenuAsync()
        {
            return await Task.Run(() =>
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    _mainMenuWindow?.AppWindow.Show();
                    _mainMenuWindow?.Activate();
                });
                return true;
            });
        }


        public void CloseChessboard()
        {
            _ = _dispatcherQueue.TryEnqueue(() =>
            {
                if (_chessBoardWindow != null)
                {
                    _chessBoardWindow.Closed -= OnChessBoardWindowClosed;
                    _chessBoardWindow.Close();
                    _chessBoardWindow = null;

                    var chessBoardViewModel = App.Current.Services.GetService<ChessBoardViewModel>();
                    chessBoardViewModel.ResetGame();

                     _ = NavigateToMainMenuAsync();
                }
            });
        }


        public async Task ShowPreferencesAsync()
        {
            var preferencesView = App.Current.Services.GetRequiredService<PreferencesView>();
            preferencesView.XamlRoot = _mainMenuWindow.Content.XamlRoot;
            preferencesView.Width = 700;
            preferencesView.Height = 800;
            preferencesView.AddPreferencesTab("Audio", typeof(AudioPreferencesView), "\uE8D6");
            preferencesView.AddPreferencesTab("Styles", typeof(StylePreferencesView), "\uE790");
            preferencesView.AddPreferencesTab("Multiplayer", typeof(MultiplayerPreferencesView), "\uE774");

            var mainMenuViewModel = App.Current.Services.GetService<MainMenuViewModel>();
            preferencesView.ButtonClicked += mainMenuViewModel.OnButtonClicked;

            await preferencesView.ShowAsync();
        }


        private void OnGameConfigWindowClosed(object sender, WindowEventArgs e)
        {
            _gameConfigurationView.Closed -= OnGameConfigWindowClosed;
            _gameConfigurationView = null;

            if (!_configurationWindowClosedProgrammatically)
            {
                try
                {
                    if (_mainMenuWindow?.AppWindow != null)
                    {
                        _mainMenuWindow.AppWindow.Show();
                        _mainMenuWindow.Activate();
                    }
                }
                catch (COMException)
                {
                    // Window may already be closed during application shutdown
                }
            }

            _configurationWindowClosedProgrammatically = false;
        }


        private void OnChessBoardWindowClosed(object sender, WindowEventArgs args)
        {
            _chessBoardWindow.Closed -= OnChessBoardWindowClosed;
            _chessBoardWindow = null;

            var chessBoardViewModel = App.Current.Services.GetService<ChessBoardViewModel>();
            chessBoardViewModel.ResetGame();

            // Disconnect Multiplayer if this is a MP game
            if (chessBoardViewModel.IsMultiplayerGame)
            {
                var multiplayerViewModel = App.Current.Services.GetService<MultiplayerViewModel>();
                multiplayerViewModel.DisconnectMultiplayer();
            }

            _ = NavigateToMainMenuAsync();
        }


        private void OnMultiplayerWindowClosed(object sender, WindowEventArgs args)
        {
            _multiplayerWindow.Closed -= OnMultiplayerWindowClosed;
            _multiplayerWindow = null;

            if (!_multiplayerWindowClosedProgrammatically)
            {
                try
                {
                    if (_mainMenuWindow?.AppWindow != null)
                    {
                        _mainMenuWindow.AppWindow.Show();
                        _mainMenuWindow.Activate();
                    }
                }
                catch (COMException)
                {
                    // Window may already be closed during application shutdown
                }
            }

            _multiplayerWindowClosedProgrammatically = false;
        }
    }
}
