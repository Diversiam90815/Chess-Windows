using Chess.UI.ViewModels;
using Chess.UI.Views;
using Chess.UI.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Threading.Tasks;


namespace Chess.UI.Services
{
    public interface INavigationService
    {
        void NavigateTo<TPage>() where TPage : Page;
        void NavigateToGameSetup();
        Task NavigateToGameAsync();
        void NavigateToHome();
        Task ShowPreferencesAsync();
        void GoBack();
        Window ShellWindow { get; }
    }


    public class NavigationService : INavigationService
    {
        private readonly IDispatcherQueueWrapper _dispatcherQueue;
        private readonly IChessGameService _gameService;

        private Frame _rootFrame;
        private Window _shellWindow;


        public Window ShellWindow => _shellWindow;


        public NavigationService(IDispatcherQueueWrapper dispatcherQueue, IChessGameService gameService)
        {
            _dispatcherQueue = dispatcherQueue;
            _gameService = gameService;
        }


        public void SetRootFrame(Frame frame, Window window)
        {
            _rootFrame = frame;
            _shellWindow = window;
        }


        public void NavigateTo<TPage>() where TPage : Page
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                _rootFrame?.Navigate(typeof(TPage), null, new SlideNavigationTransitionInfo
                {
                    Effect = SlideNavigationTransitionEffect.FromRight
                });
            });
        }


        public void NavigateToGameSetup()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                _rootFrame?.Navigate(typeof(GameSetupPage), null, new SlideNavigationTransitionInfo
                {
                    Effect = SlideNavigationTransitionEffect.FromRight
                });
            });
        }


        public async Task NavigateToGameAsync()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                _rootFrame?.Navigate(typeof(GamePage), null, new SlideNavigationTransitionInfo
                {
                    Effect = SlideNavigationTransitionEffect.FromRight
                });
            });

            await Task.CompletedTask;
        }


        public void NavigateToHome()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                _rootFrame?.Navigate(typeof(HomePage), null, new SlideNavigationTransitionInfo
                {
                    Effect = SlideNavigationTransitionEffect.FromLeft
                });
            });
        }


        public void GoBack()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                if (_rootFrame?.CanGoBack == true)
                {
                    _rootFrame.GoBack();
                }
            });
        }


        public async Task ShowPreferencesAsync()
        {
            await Task.Run(() =>
            {
                _dispatcherQueue.TryEnqueue(async () =>
                {
                    var preferencesView = App.Current.Services.GetRequiredService<PreferencesView>();
                    preferencesView.XamlRoot = _rootFrame.XamlRoot;
                    preferencesView.Width = 700;
                    preferencesView.Height = 550;
                    preferencesView.AddPreferencesTab("Appearance", typeof(StylePreferencesView), "\uE790");
                    preferencesView.AddPreferencesTab("Audio", typeof(AudioPreferencesView), "\uE8D6");
                    preferencesView.AddPreferencesTab("Multiplayer", typeof(MultiplayerPreferencesView), "\uE774");

                    var mainMenuViewModel = App.Current.Services.GetService<MainMenuViewModel>();
                    preferencesView.ButtonClicked += mainMenuViewModel.OnButtonClicked;

                    await preferencesView.ShowAsync();
                });
            });
        }
    }
}
