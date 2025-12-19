using Chess.UI.Audio.Core;
using Chess.UI.Audio.Services;
using Chess.UI.Board;
using Chess.UI.Coordinates;
using Chess.UI.Images;
using Chess.UI.Models;
using Chess.UI.MoveHistory;
using Chess.UI.Moves;
using Chess.UI.Multiplayer;
using Chess.UI.Score;
using Chess.UI.Services;
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
        private Window MainMenu;

        public CommunicationLayer ChessLogicCommunication { get; set; }

        public static new App Current { get; private set; }

        public IServiceProvider Services { get; }


        public App()
        {
            Services = ConfigureServices();

            ChessLogicCommunication = new CommunicationLayer();
            Current = this;
            this.InitializeComponent();
        }


        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            ChessLogicCommunication.Init();

            var audioService = Services.GetService<IChessAudioService>();
            audioService.InitializeAsync();

            MainMenu = new MainMenuWindow();

            MainMenu.Closed += (sender, args) =>
            {
                ChessLogicCommunication.Deinit();
            };

            MainMenu.Activate();

            Logger.LogInfo("App initialized!");
        }


        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IDispatcherQueueWrapper, DispatcherQueueWrapper>();

            services.AddSingleton<IChessCoordinate, ChessCoordinate>();
            services.AddSingleton<IStyleManager, StyleManager>();
            services.AddSingleton<IImageService, ImageServices>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IGameConfigurationService, GameConfigurationService>();
            services.AddSingleton<IGameConfigurationBuilder, GameConfigurationBuilder>();
            services.AddSingleton<IWindowSizeService, WindowSizeService>();

            services.AddSingleton<IMoveModel, MoveModel>();
            services.AddSingleton<IScoreModel, ScoreModel>();
            services.AddSingleton<IMoveHistoryModel, MoveHistoryModel>();
            services.AddSingleton<IBoardModel, BoardModel>();
            services.AddSingleton<IMultiplayerModel, MultiplayerModel>();
            services.AddSingleton<IMultiplayerPreferencesModel, MultiplayerPreferencesModel>();

            // Register view models
            services.AddSingleton<ChessBoardViewModel>();
            services.AddSingleton<ScoreViewModel>();
            services.AddSingleton<MoveHistoryViewModel>();
            services.AddSingleton<MainMenuViewModel>();
            services.AddSingleton<MultiplayerViewModel>();
            services.AddSingleton<StylesPreferencesViewModel>();
            services.AddSingleton<MultiplayerPreferencesViewModel>();
            services.AddSingleton<AudioPreferencesViewModel>();
            services.AddSingleton<GameSetupViewModel>();

            services.AddTransient<MainMenuWindow>();
            services.AddTransient<ChessBoardWindow>();
            services.AddTransient<MultiplayerWindow>();
            services.AddTransient<StylePreferencesView>();
            services.AddTransient<MultiplayerPreferencesView>();
            services.AddTransient<AudioPreferencesView>();
            services.AddTransient<GameConfigurationView>();

            // Audio Services
            services.AddSingleton<IAudioEngine, AudioEngine>();
            services.AddSingleton<IChessAudioService, ChessAudioService>();

            services.AddTransient<PreferencesView>();

            return services.BuildServiceProvider();
        }
    }
}
