using Chess.UI.Services;
using Chess.UI.Styles;
using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace Chess.UI.Views
{
    public sealed partial class HomePage : Page
    {
        private readonly HomePageViewModel _viewModel;
        private readonly INavigationService _navigationService;


        public HomePage()
        {
            this.InitializeComponent();

            _viewModel = new HomePageViewModel();
            _navigationService = App.Current.Services.GetService<INavigationService>();

            this.DataContext = _viewModel;
        }


        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var mainMenuVM = App.Current.Services.GetService<MainMenuViewModel>();
            mainMenuVM?.OnButtonClicked();

            _navigationService.NavigateToGameSetup();
        }


        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var mainMenuVM = App.Current.Services.GetService<MainMenuViewModel>();
            mainMenuVM?.OnButtonClicked();

            await _navigationService.ShowPreferencesAsync();
        }


        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }


    /// <summary>
    /// Simple ViewModel for the HomePage providing board preview image
    /// </summary>
    public class HomePageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ImageSource _boardPreviewImage;
        public ImageSource BoardPreviewImage
        {
            get
            {
                if (_boardPreviewImage == null)
                {
                    var styleManager = App.Current.Services.GetService<IStyleManager>();
                    var imageService = App.Current.Services.GetService<IImageService>();
                    if (styleManager != null && imageService != null)
                    {
                        _boardPreviewImage = imageService.GetImage(styleManager.CurrentBoardStyle);
                    }
                }
                return _boardPreviewImage;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
