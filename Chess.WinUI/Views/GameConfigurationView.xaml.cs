using Chess.UI.Services;
using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;


namespace Chess.UI.Views
{
    public sealed partial class GameConfigurationView : Window
    {
        private readonly IWindowSizeService _windowSizeService;
        private readonly GameSetupViewModel _viewModel;


        public GameConfigurationView()
        {
            InitializeComponent();

            _windowSizeService = App.Current.Services.GetService<IWindowSizeService>();
            _windowSizeService.SetWindowSize(this, 500, 350);
            _windowSizeService.SetWindowNonResizable(this);

            _viewModel = App.Current.Services.GetService<GameSetupViewModel>();
        }


        private void CPUGame_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CPUGameInitiated();
        }


        private void CoopGame_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LocalCoopInitiated();
        }


        private void StartCPUGameButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.StartSinglePlayerGameAsnyc();
        }


        private void PlayerReturnButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void DifficultyReturnButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Reset();

            _viewModel.PlayerConfigVisible = true;
            _viewModel.CPUConfigVisible = false;
        }


        private void SelectWhiteButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.PlayerColor = EngineAPI.Side.White;
        }


        private void SelectBlackButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.PlayerColor = EngineAPI.Side.Black;
        }


        private void EasyDifficultyButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CPUDifficulty = EngineAPI.CPUDifficulty.Easy;
        }


        private void MediumDifficultyButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CPUDifficulty = EngineAPI.CPUDifficulty.Medium;
        }


        private void HardDifficultyButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CPUDifficulty = EngineAPI.CPUDifficulty.Hard;
        }
    }
}
