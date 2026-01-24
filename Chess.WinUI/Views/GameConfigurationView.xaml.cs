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
            _windowSizeService.SetWindowSize(this, 650, 500);
            _windowSizeService.SetWindowNonResizable(this);

            _viewModel = App.Current.Services.GetService<GameSetupViewModel>();
            Rootgrid.DataContext = _viewModel;

            // Set default navigation item
            if (GameModeNavigationView.MenuItems.Count > 0)
            {
                GameModeNavigationView.SelectedItem = GameModeNavigationView.MenuItems[0];
            }
        }


        private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                var tag = args.SelectedItemContainer.Tag?.ToString();

                switch (tag)
                {
                    case "LocalCoop":
                        ContentFrame.Navigate(typeof(LocalCoopConfigPage), _viewModel);
                        break;

                    case "SinglePlayer":
                        ContentFrame.Navigate(typeof(SinglePlayerConfigPage), _viewModel);
                        break;

                    case "Multiplayer":
                        ContentFrame.Navigate(typeof(MultiplayerConfigPage), _viewModel);
                        break;
                }
            }
        }


        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Reset();
            this.Close();
        }



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
