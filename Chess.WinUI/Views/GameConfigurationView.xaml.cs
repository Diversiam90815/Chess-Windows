using Chess.UI.Services;
using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;


namespace Chess.UI.Views
{
    public sealed partial class GameConfigurationView : Window
    {
        private readonly IWindowSizeService _windowSizeService;
        private readonly GameSetupViewModel _viewModel;


        public GameConfigurationView()
        {
            InitializeComponent();

            _windowSizeService = App.Current.Services.GetService<IWindowSizeService>();
            _windowSizeService.SetWindowSize(this, 500, 350);
            _windowSizeService.SetWindowNonResizable(this);

            _viewModel = App.Current.Services.GetService<GameSetupViewModel>();
        }


        private async void OnSinglePlayerClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = await _viewModel.StartSinglePlayerWithCurrentStateAsync();

                if (!success)
                    await ShowErrorDialogAsync("Failed to start single player game! Please try again!");
            }
            catch(Exception ex)
            {
                Logger.LogError($"Error starting single-player game: {ex.Message}");
                await ShowErrorDialogAsync($"An error occured: {ex.Message}");
            }
        }


        private async void OnMultiPlayerClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.PlayerColor == EngineAPI.Side.None)
                {
                    await ShowErrorDialogAsync("Please select your player color first.");
                    return;
                }

                bool success = await _viewModel.StartMultiplayerGameAsync(_viewModel.PlayerColor);

                if (!success)
                    await ShowErrorDialogAsync("Failed to start multiplayer game. Please try again.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error starting multiplayer game: {ex.Message}");
                await ShowErrorDialogAsync($"An error occurred: {ex.Message}");
            }
        }


        private async void OnLocalCoopClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = await _viewModel.StartLocalCoopGameAsync();

                if (!success)
                {
                    await ShowErrorDialogAsync("Failed to start local co-op game. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error starting local co-op game: {ex.Message}");
                await ShowErrorDialogAsync($"An error occurred: {ex.Message}");
            }
        }


        #region Navigation Handlers

        private void PlayerReturnButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Reset();
            this.Close();
        }


        private void DifficultyReturnButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Reset();
        }

        #endregion


        #region Player Color Selection Handlers

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

        #endregion


        #region CPU Difficulty Selection Handlers

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

        #endregion


        #region Helper Methods

        private async System.Threading.Tasks.Task ShowErrorDialogAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        #endregion
    }
}
