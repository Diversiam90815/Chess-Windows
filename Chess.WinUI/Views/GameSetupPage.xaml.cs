using Chess.UI.Services;
using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;


namespace Chess.UI.Views
{
    public sealed partial class GameSetupPage : Page
    {
        private readonly GameSetupViewModel _setupViewModel;
        private readonly INavigationService _navigationService;

        private enum GameMode { Coop, SinglePlayer, Online }
        private GameMode _currentMode = GameMode.Coop;


        public GameSetupPage()
        {
            this.InitializeComponent();

            _setupViewModel = App.Current.Services.GetService<GameSetupViewModel>();
            _navigationService = App.Current.Services.GetService<INavigationService>();

            _setupViewModel.Reset();
            this.DataContext = _setupViewModel;
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _setupViewModel.Reset();
            _navigationService.GoBack();
        }


        private void ModeChanged(object sender, RoutedEventArgs e)
        {
            if (sender == CoopRadio)
                SetMode(GameMode.Coop);
            else if (sender == SinglePlayerRadio)
                SetMode(GameMode.SinglePlayer);
            else if (sender == OnlineRadio)
                SetMode(GameMode.Online);
        }


        private void SetMode(GameMode mode)
        {
            _currentMode = mode;
            
            if (CoopPanel == null) return;

            CoopPanel.Visibility = mode == GameMode.Coop ? Visibility.Visible : Visibility.Collapsed;
            SinglePlayerPanel.Visibility = mode == GameMode.SinglePlayer ? Visibility.Visible : Visibility.Collapsed;
            MultiplayerControl.Visibility = mode == GameMode.Online ? Visibility.Visible : Visibility.Collapsed;
            StartGameButton.Visibility = mode == GameMode.Online ? Visibility.Collapsed : Visibility.Visible;

            if (mode == GameMode.Online)
            {
                MultiplayerControl.Initialize();
            }
        }


        private void SelectWhiteButton_Click(object sender, RoutedEventArgs e)
        {
            _setupViewModel.PlayerColor = EngineAPI.Side.White;
        }


        private void SelectBlackButton_Click(object sender, RoutedEventArgs e)
        {
            _setupViewModel.PlayerColor = EngineAPI.Side.Black;
        }


        private void EasyDifficultyButton_Click(object sender, RoutedEventArgs e)
        {
            _setupViewModel.CPUDifficulty = EngineAPI.CPUDifficulty.Easy;
        }


        private void MediumDifficultyButton_Click(object sender, RoutedEventArgs e)
        {
            _setupViewModel.CPUDifficulty = EngineAPI.CPUDifficulty.Medium;
        }


        private void HardDifficultyButton_Click(object sender, RoutedEventArgs e)
        {
            _setupViewModel.CPUDifficulty = EngineAPI.CPUDifficulty.Hard;
        }


        private async void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = _currentMode switch
                {
                    GameMode.Coop => await _setupViewModel.StartLocalCoopGameAsync(),
                    GameMode.SinglePlayer => await _setupViewModel.StartSinglePlayerWithCurrentStateAsync(),
                    _ => false
                };

                if (!success)
                {
                    Logger.LogError($"Failed to start game in mode: {_currentMode}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error starting game: {ex.Message}");
            }
        }
    }
}
