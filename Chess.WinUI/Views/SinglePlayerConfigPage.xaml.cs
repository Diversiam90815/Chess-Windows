using Chess.UI.Services;
using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;


namespace Chess.UI.Views
{
    public sealed partial class SinglePlayerConfigPage : Page
    {
        private GameSetupViewModel _viewModel;


        public SinglePlayerConfigPage()
        {
            this.InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is GameSetupViewModel viewModel)
            {
                _viewModel = viewModel;
                this.DataContext = _viewModel;
            }
        }


        private void SelectWhiteButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.PlayerColor = EngineAPI.Side.White;
            Logger.LogInfo("Player selected White");
        }


        private void SelectBlackButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.PlayerColor = EngineAPI.Side.Black;
            Logger.LogInfo("Player selected Black");
        }


        private void EasyDifficultyButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CPUDifficulty = EngineAPI.CPUDifficulty.Easy;
            Logger.LogInfo("Easy difficulty selected");
        }


        private void MediumDifficultyButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CPUDifficulty = EngineAPI.CPUDifficulty.Medium;
            Logger.LogInfo("Medium difficulty selected");
        }


        private void HardDifficultyButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CPUDifficulty = EngineAPI.CPUDifficulty.Hard;
            Logger.LogInfo("Hard difficulty selected");
        }


        private async void StartSinglePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = await _viewModel.StartSinglePlayerWithCurrentStateAsync();

                if (!success)
                {
                    Logger.LogError("Failed to start single-player game");

                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error starting single-player game: {ex.Message}");
            }
        }
    }
}
