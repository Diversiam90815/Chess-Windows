using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;


namespace Chess.UI.Views
{
    public sealed partial class StylePreferencesView : Page
    {
        private readonly StylesPreferencesViewModel _viewModel;


        public StylePreferencesView()
        {
            this.InitializeComponent();

            _viewModel = App.Current.Services.GetService<StylesPreferencesViewModel>();
        }
    }
}
