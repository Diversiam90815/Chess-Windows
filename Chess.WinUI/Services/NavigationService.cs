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
        Task<bool> NavigateToChessboardAsync();
        Task<bool> NavigateToMultiplayerAsync();
        Task<bool> NavigateToMainMenuAsync();
        Task ShowPreferencesAsync();
        void CloseCurrentView();
    }


    public class NavigationService : INavigationService
    {
        private readonly IDispatcherQueueWrapper _dispatcherQueue;
        private readonly IChessGameService _gameService;

        private GameConfigurationView _gameConfigurationView;
        private ChessBoardWindow _chessBoardWindow;
        private MultiplayerWindow _multiplayerWindow;
        private MainMenuWindow _mainMenuWindow;

        private bool _multiplayerWindowClosedProgrammatically = false;
        private bool _configurationWindowClosedProgrammatically = false;


        public NavigationService(IDispatcherQueueWrapper dispatcherQueue, IChessGameService gameService)
        {
            _dispatcherQueue = dispatcherQueue;
            _gameService = gameService;
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
                    else
                    {
                        _gameConfigurationView.Activate();
                    }
                });
            });
        }


        public async Task<bool> NavigateToChessboardAsync()
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

                        // Close transitional windows
                        CloseMultiplayerWindow();
                        CloseGameConfigurationWindow();
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


        public void CloseCurrentView()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                if (_chessBoardWindow != null)
                {
                    _chessBoardWindow.Close();
                }
                else if (_multiplayerWindow != null)
                {
                    _multiplayerWindow.Close();
                }
                else if (_gameConfigurationView != null)
                {
                    _gameConfigurationView.Close();
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


        private void CloseMultiplayerWindow()
        {
            if (_multiplayerWindow != null)
            {
                _multiplayerWindowClosedProgrammatically = true;
                _multiplayerWindow.Close();
            }
        }


        private void CloseGameConfigurationWindow()
        {
            if (_gameConfigurationView != null)
            {
                _configurationWindowClosedProgrammatically = true;
                _gameConfigurationView.Close();
            }
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

            _ = _gameService.EndGameAsync();

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
