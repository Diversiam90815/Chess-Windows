using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;


namespace Chess.UI.Services
{
    public interface IWindowSizeService
    {
        void SetWindowSize(Window window, double width, double height);
        void SetWindowNonResizable(Window window);
    }


    public class WindowSizeService : IWindowSizeService
    {
        private OverlappedPresenter _presenter;

        public void SetWindowSize(Window window, double width, double height)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            float scalingFactor = EngineAPI.GetWindowScalingFactor(hwnd);
            int scaledWidth = (int)(width * scalingFactor);
            int scaledHeight = (int)(height * scalingFactor);
            window.AppWindow.Resize(new(scaledWidth, scaledHeight));
        }

        public void SetWindowNonResizable(Window window)
        {
            _presenter = window.AppWindow.Presenter as OverlappedPresenter;
            _presenter.IsResizable = false;
            _presenter.IsMaximizable = false;
        }
    }
}
