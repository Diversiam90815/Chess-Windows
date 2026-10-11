using Chess.UI.Audio.Core;
using Chess.UI.Audio.Services;
using Chess.UI.Models;
using Chess.UI.Services;
using Chess.UI.Settings;
using Chess.UI.Styles;
using Chess.UI.ViewModels;
using Chess.UI.Views;
using Chess.UI.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;


namespace Chess.UI
{
    public partial class App : Application
    {
        private ShellWindow _shellWindow;

        public static new App Current { get; private set; }

        public IServiceProvider Services { get; }


        public App()
        {
            Services = ConfigureServices();

            Current = this;
            this.InitializeComponent();
        }


        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var engineCommunication = Services.GetService<ICommunicationLayer>();
            engineCommunication.Init();

            // Audio loads in the background so it does not hold up the window
            _ = InitializeAudioAsync();

            _shellWindow = new ShellWindow();

            _shellWindow.Closed += (sender, args) =>
            {
                engineCommunication.Deinit();
            };

            _shellWindow.Activate();

            Logger.LogInfo("App initialized!");
        }


        private async Task InitializeAudioAsync()
        {
            try
            {
                var audioService = Services.GetService<IChessAudioService>();
                await audioService.InitializeAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to initialize audio: {ex.Message}");
            }
        }


        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IDispatcherQueueWrapper, DispatcherQueueWrapper>();

            services.AddSingleton<ISettingsService, SettingsService>();

            services.AddSingleton<ICommunicationLayer, CommunicationLayer>();
            services.AddSingleton<IStyleManager, StyleManager>();
            services.AddSingleton<IImageService, ImageServices>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IChessGameService, ChessGameService>();
            services.AddSingleton<IWindowSizeService, WindowSizeService>();

            services.AddSingleton<IMoveModel, MoveModel>();
            services.AddSingleton<ICapturedPiecesModel, CapturedPiecesModel>();
            services.AddSingleton<IMoveHistoryModel, MoveHistoryModel>();
            services.AddSingleton<IBoardModel, BoardModel>();
            services.AddSingleton<IMultiplayerModel, MultiplayerModel>();
            services.AddSingleton<IMultiplayerPreferencesModel, MultiplayerPreferencesModel>();

            // Register view models
            services.AddSingleton<GameWindowViewModel>();
            services.AddSingleton<CapturedPiecesViewModel>();
            services.AddSingleton<ChessBoardViewModel>();
            services.AddSingleton<MoveHistoryViewModel>();
            services.AddSingleton<MainMenuViewModel>();
            services.AddSingleton<MultiplayerViewModel>();
            services.AddSingleton<MultiplayerInfoViewModel>();
            services.AddSingleton<StylesPreferencesViewModel>();
            services.AddSingleton<MultiplayerPreferencesViewModel>();
            services.AddSingleton<AudioPreferencesViewModel>();
            services.AddSingleton<GameSetupViewModel>();

            // Audio Services
            services.AddSingleton<IAudioEngine, AudioEngine>();
            services.AddSingleton<IChessAudioService, ChessAudioService>();

            return services.BuildServiceProvider();
        }
    }
}
