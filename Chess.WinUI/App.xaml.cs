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

            var audioService = Services.GetService<IChessAudioService>();
            audioService.InitializeAsync();

            _shellWindow = new ShellWindow();

            _shellWindow.Closed += (sender, args) =>
            {
                engineCommunication.Deinit();
            };

            _shellWindow.Activate();

            Logger.LogInfo("App initialized!");
        }


        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IDispatcherQueueWrapper, DispatcherQueueWrapper>();

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
            services.AddSingleton<StylesPreferencesViewModel>();
            services.AddSingleton<MultiplayerPreferencesViewModel>();
            services.AddSingleton<AudioPreferencesViewModel>();
            services.AddSingleton<GameSetupViewModel>();

            // Views that need DI resolution
            services.AddTransient<StylePreferencesView>();
            services.AddTransient<MultiplayerPreferencesView>();
            services.AddTransient<AudioPreferencesView>();

            // Audio Services
            services.AddSingleton<IAudioEngine, AudioEngine>();
            services.AddSingleton<IChessAudioService, ChessAudioService>();

            services.AddTransient<PreferencesView>();

            return services.BuildServiceProvider();
        }
    }
}
