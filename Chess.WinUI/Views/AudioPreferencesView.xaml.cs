using Chess.UI.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;


namespace Chess.UI.Views
{
    public sealed partial class AudioPreferencesView : Page
    {
        private readonly AudioPreferencesViewModel _viewModel;


        public AudioPreferencesView()
        {
            InitializeComponent();

            _viewModel = App.Current.Services.GetService<AudioPreferencesViewModel>();
        }


        private void SFX_Volume_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Slider slider && _viewModel != null)
            {
                _viewModel.SfxVolume = (int)slider.Value;
            }
        }


        private void Atmos_Volume_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Slider slider && _viewModel != null)
            {
                _viewModel.AtmosVolume = (int)slider.Value;
            }
        }


        private void Master_Volume_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Slider slider && _viewModel != null)
            {
                _viewModel.MasterVolume = (int)slider.Value;
            }
        }
    }
}
