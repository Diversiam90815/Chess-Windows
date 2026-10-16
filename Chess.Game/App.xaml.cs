using Chess.UI.Audio.Core;
using Chess.UI.Audio.Modules;
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
            Services = ConfigureServices().BuildServiceProvider();

            Current = this;
            this.InitializeComponent();
        }


        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // First of all, so everything after it is in the log. The level is set by the app, for the engine and NetLink as well.
            Logger.Initialize(Project.AppDataDirectory);
            Logger.SetLevel(Services.GetService<ISettingsService>().LogLevel);

            var engineCommunication = Services.GetService<ICommunicationLayer>();
            engineCommunication.Init();

            // Audio loads in the background so it does not hold up the window
            _ = InitializeAudioAsync();

            _shellWindow = new ShellWindow();

            _shellWindow.Closed += (sender, args) =>
            {
                // Tells a connected opponent that the session ends
                Services.GetService<INetworkService>().Dispose();
                engineCommunication.Deinit();

                Logger.LogInfo("App closed");
                Logger.Shutdown();
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

                ConnectUISounds(audioService);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to initialize audio: {ex.Message}");
            }
        }


        // The view models only announce clicks and selections. Which of them make a sound is decided here,
        // so the audio service and the view models do not need to know each other.
        private void ConnectUISounds(IChessAudioService audioService)
        {
            void Play(UIInteraction interaction) => _ = Task.Run(async () => await audioService.HandleUIInteractionAsync(interaction));

            Services.GetService<MainMenuViewModel>().ButtonClicked += () => Play(UIInteraction.ButtonClick);
            Services.GetService<ChessBoardViewModel>().ButtonClicked += () => Play(UIInteraction.ButtonClick);
            Services.GetService<MultiplayerViewModel>().ButtonClicked += () => Play(UIInteraction.ButtonClick);

            Services.GetService<StylesPreferencesViewModel>().ItemSelected += () => Play(UIInteraction.ItemSelected);
            Services.GetService<MultiplayerPreferencesViewModel>().ItemSelected += () => Play(UIInteraction.ItemSelected);
            Services.GetService<AudioPreferencesViewModel>().ItemSelected += () => Play(UIInteraction.ItemSelected);
        }


        public static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IDispatcherQueueWrapper, DispatcherQueueWrapper>();

            services.AddSingleton<ISettingsService>(_ => new SettingsService(Project.AppDataDirectory));

            services.AddSingleton<ICommunicationLayer, CommunicationLayer>();
            services.AddSingleton<INetworkService, NetworkService>();
            services.AddSingleton<IStyleManager, StyleManager>();
            services.AddSingleton<IStyleLoader, StyleLoader>();
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
            services.AddSingleton<ISoundEffectsModule, SoundEffectsModule>();
            services.AddSingleton<IAtmosphereModule, AtmosphereModule>();
            services.AddSingleton<IChessAudioService, ChessAudioService>();

            return services;
        }
    }
}
