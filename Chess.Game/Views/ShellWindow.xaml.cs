using Chess.UI.Services;
using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;


namespace Chess.UI.Views
{
    public sealed partial class ShellWindow : Window
    {
        private readonly IWindowSizeService _windowSizeService;
        private readonly INavigationService _navigationService;


        public ShellWindow()
        {
            this.InitializeComponent();
            AppWindow.SetIcon(Project.IconPath);

            _windowSizeService = App.Current.Services.GetService<IWindowSizeService>();
            _navigationService = App.Current.Services.GetService<INavigationService>();

            // Set initial window size and allow resizing
            _windowSizeService.SetWindowSize(this, 1100, 800);

            // Set minimum size via presenter
            var presenter = AppWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
            if (presenter != null)
            {
                presenter.IsResizable = true;
                presenter.IsMaximizable = true;
            }

            // Enforce minimum window size to prevent layout crash
            this.SizeChanged += (s, e) =>
            {
                const int MinWidth = 800;
                const int MinHeight = 600;

                var size = AppWindow.Size;
                bool changed = false;
                int newWidth = size.Width;
                int newHeight = size.Height;

                if (size.Width < MinWidth)
                {
                    newWidth = MinWidth;
                    changed = true;
                }
                if (size.Height < MinHeight)
                {
                    newHeight = MinHeight;
                    changed = true;
                }
                if (changed)
                {
                    AppWindow.Resize(new Windows.Graphics.SizeInt32(newWidth, newHeight));
                }
            };

            // Apply Mica backdrop
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();

            // Register this window's frame with the navigation service
            if (_navigationService is NavigationService navService)
            {
                navService.SetRootFrame(RootFrame, this);
            }

            // Subscribe to multiplayer navigation (lives for entire app lifetime)
            var multiplayerViewModel = App.Current.Services.GetService<MultiplayerViewModel>();
            multiplayerViewModel.RequestNavigationToChessboard += () =>
            {
                _ = _navigationService.NavigateToGameAsync();
            };

            // Navigate to home page
            RootFrame.Navigate(typeof(HomePage));
        }


        public Frame GetRootFrame() => RootFrame;
    }
}
