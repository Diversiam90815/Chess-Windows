using Chess.UI.Images;
using Chess.UI.Services;
using Chess.UI.Wrappers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace Chess.UI.ViewModels
{
    public partial class MainMenuViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Specific events for different actions
        public event Action ButtonClicked;
        public event Func<Task> StartGameRequested;
        public event Func<Task> SettingsRequested;
        public event Func<Task> MultiplayerRequested;
        public event Action QuitRequested;

        private readonly IDispatcherQueueWrapper _dispatcherQueue;
        private readonly IImageService _imageServices;
        private readonly INavigationService _navigationService;
        private readonly IGameConfigurationService _gameConfigurationService;

        private Window _ownerWindow;


        public MainMenuViewModel(
          IDispatcherQueueWrapper dispatcher,
          IImageService imageServices,
          INavigationService navigationService,
          IGameConfigurationService gameConfigurationService)
        {
            _dispatcherQueue = dispatcher;
            _imageServices = imageServices;
            _navigationService = navigationService;
            _gameConfigurationService = gameConfigurationService;

            Init();
        }


        private void Init()
        {
            StartGameButtonImage = _imageServices.GetImage(ImageServices.MainMenuButton.StartGame);
            SettingButtonImage = _imageServices.GetImage(ImageServices.MainMenuButton.Settings);
            MultiplayerButtonImage = _imageServices.GetImage(ImageServices.MainMenuButton.Multiplayer);
            EndGameButtonImage = _imageServices.GetImage(ImageServices.MainMenuButton.EndGame);
        }

        public void SetOwnerWindow(Window owner)
        {
            _ownerWindow = owner;
        }


        public void OnButtonClicked()
        {
            ButtonClicked?.Invoke();
        }


        public async Task OnStartGameRequestedAsync()
        {
            await (StartGameRequested?.Invoke() ?? Task.CompletedTask);
        }


        public async Task OnSettingsRequestedAsync()
        {
            await (SettingsRequested?.Invoke() ?? Task.CompletedTask);
        }


        public async Task OnMultiplayerRequestedAsync()
        {
            await (MultiplayerRequested?.Invoke() ?? Task.CompletedTask);
        }


        public void OnQuitRequested()
        {
            QuitRequested?.Invoke();
        }


        private ImageSource startGameButtonImage;
        public ImageSource StartGameButtonImage
        {
            get => startGameButtonImage;
            set
            {
                if (startGameButtonImage != value)
                {
                    startGameButtonImage = value;
                    OnPropertyChanged();
                }
            }
        }


        private ImageSource settingButtonImage;
        public ImageSource SettingButtonImage
        {
            get => settingButtonImage;
            set
            {
                if (settingButtonImage != value)
                {
                    settingButtonImage = value;
                    OnPropertyChanged();
                }
            }
        }


        private ImageSource multiplayerButtonImage;
        public ImageSource MultiplayerButtonImage
        {
            get => multiplayerButtonImage;
            set
            {
                if (multiplayerButtonImage != value)
                {
                    multiplayerButtonImage = value;
                    OnPropertyChanged();
                }
            }
        }


        private ImageSource endGameButtonImage;
        public ImageSource EndGameButtonImage
        {
            get => endGameButtonImage;
            set
            {
                if (endGameButtonImage != value)
                {
                    endGameButtonImage = value;
                    OnPropertyChanged();
                }
            }
        }


        public async Task HandleStartGameAsync()
        {
            try
            {
                await _navigationService.NavigateToGameConfigurationView();
            }
            catch (Exception ex)
            {
                // Handle error - could add error service here
                Logger.LogError($"Failed to start game: {ex.Message}");
            }
        }


        public async Task HandleSettingsAsync()
        {
            await _navigationService.ShowPreferencesAsync();
        }


        public async Task HandleMultiplayerAsync()
        {
            await _navigationService.NavigateToMultiplayerAsync();
        }


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }
}
