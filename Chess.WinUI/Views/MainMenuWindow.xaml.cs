using Chess.UI.Services;
using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;


namespace Chess.UI
{
    public sealed partial class MainMenuWindow : Window
    {
        private readonly MainMenuViewModel _viewModel;
        private readonly INavigationService _navigationService;
        private readonly IWindowSizeService _windowSizeService;


        public MainMenuWindow()
        {
            this.InitializeComponent();

            AppWindow.SetIcon(Project.IconPath);

            _viewModel = App.Current.Services.GetService<MainMenuViewModel>();
            _navigationService = App.Current.Services.GetService<INavigationService>();
            _windowSizeService = App.Current.Services.GetService<IWindowSizeService>();

            // Set this window in navigation service
            if (_navigationService is NavigationService navService)
            {
                navService.SetMainMenuWindow(this);
            }

            this.RootGrid.DataContext = _viewModel;

            Init();

            _windowSizeService.SetWindowSize(this, 800, 750);
            _windowSizeService.SetWindowNonResizable(this);
        }


        private void Init()
        {
            _viewModel.SetOwnerWindow(this);

            SubscribeToViewModelEvents();
            SubscribeToMultiplayerEvents();
        }


        private void SubscribeToMultiplayerEvents()
        {
            var multiplayerViewModel = App.Current.Services.GetService<MultiplayerViewModel>();
            multiplayerViewModel.RequestNavigationToChessboard += () =>
            {
                _ = Task.Run(async () => await _navigationService.NavigateToChessboardAsync());
            };

            // TODO
            //multiplayerViewModel.RequestCloseChessboard += () => _navigationService.CloseChessboard();
        }


        private void SubscribeToViewModelEvents()
        {
            _viewModel.StartGameRequested += HandleStartGameAsync;
            _viewModel.MultiplayerRequested += HandleMultiplayerAsync;
            _viewModel.QuitRequested += HandleQuitAction;
            _viewModel.SettingsRequested += HandleSettingsAsync;
        }


        private async void StartGameButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            await _viewModel.OnStartGameRequestedAsync();
        }


        private async void MultiplayerButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            await _viewModel.OnMultiplayerRequestedAsync();
        }


        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            await _viewModel.OnSettingsRequestedAsync();
        }


        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            _viewModel.OnQuitRequested();
        }


        private async Task HandleStartGameAsync()
        {
            await _viewModel.HandleStartGameAsync();
        }


        private async Task HandleMultiplayerAsync()
        {
            await _viewModel.HandleMultiplayerAsync();
        }


        private async Task HandleSettingsAsync()
        {
            await _viewModel.HandleSettingsAsync();
        }


        private void HandleQuitAction()
        {
            // Exit the application
            var app = Application.Current;
            app.Exit();
        }
    }
}
