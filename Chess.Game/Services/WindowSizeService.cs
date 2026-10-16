using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;


namespace Chess.UI.Services
{
    public interface IWindowSizeService
    {
        void SetWindowSize(Window window, double width, double height);
        void SetWindowNonResizable(Window window);
    }


    public class WindowSizeService : IWindowSizeService
    {
        private const float DefaultDpi = 96.0f;

        private OverlappedPresenter _presenter;

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(nint hwnd);


        public void SetWindowSize(Window window, double width, double height)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            float scalingFactor = GetWindowScalingFactor(hwnd);
            int scaledWidth = (int)(width * scalingFactor);
            int scaledHeight = (int)(height * scalingFactor);
            window.AppWindow.Resize(new(scaledWidth, scaledHeight));
        }


        private static float GetWindowScalingFactor(nint hwnd)
        {
            uint dpi = GetDpiForWindow(hwnd);

            // GetDpiForWindow returns 0 for an invalid window handle
            return dpi == 0 ? 1.0f : dpi / DefaultDpi;
        }

        public void SetWindowNonResizable(Window window)
        {
            _presenter = window.AppWindow.Presenter as OverlappedPresenter;
            _presenter.IsResizable = false;
            _presenter.IsMaximizable = false;
        }
    }
}
