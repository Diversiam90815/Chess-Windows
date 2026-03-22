using Chess.UI.Services;
using Chess.UI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;


namespace Chess.UI.Views
{
    public sealed partial class LocalCoopConfigPage : Page
    {
        private GameSetupViewModel _viewModel;


        public LocalCoopConfigPage()
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


        private async void StartLocalCoopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool success = await _viewModel.StartLocalCoopGameAsync();

                if (!success)
                {
                    Logger.LogError("Failed to start local coop game!");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error starting local co-op game: {ex.Message}");
            }
        }
    }
}