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

        // Events for audio feedback
        public event Action ButtonClicked;

        private readonly IDispatcherQueueWrapper _dispatcherQueue;
        private readonly INavigationService _navigationService;


        public MainMenuViewModel(
          IDispatcherQueueWrapper dispatcher,
          IImageService imageServices,
          INavigationService navigationService)
        {
            _dispatcherQueue = dispatcher;
            _navigationService = navigationService;
        }


        public void OnButtonClicked()
        {
            ButtonClicked?.Invoke();
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
