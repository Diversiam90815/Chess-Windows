using Chess.UI.Services;
using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace Chess.UI.Views.Controls
{
    public sealed partial class MultiplayerSetupControl : UserControl
    {
        private readonly MultiplayerViewModel _viewModel;


        public MultiplayerSetupControl()
        {
            this.InitializeComponent();

            _viewModel = App.Current.Services.GetService<MultiplayerViewModel>();
            this.Rootgrid.DataContext = _viewModel;
        }


        /// <summary>
        /// Called when the control becomes visible to initialize the multiplayer session.
        /// </summary>
        public void Initialize()
        {
            _viewModel.ResetViewState();
            _viewModel.StartMultiplayerSetup();
        }


        private void HostGameButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            _viewModel.EnterServerMultiplayerMode();
        }


        private void JoinGameButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            _viewModel.EnterClientMultiplayerMode();
        }


        private void HostAcceptButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            _viewModel.AcceptClientConnection();
        }


        private void HostDeclineButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            _viewModel.DeclineClientConnection();
        }


        private void JoinAcceptButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            _viewModel.AcceptConnectingToHost();
        }


        private void JoinDiscardButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            _viewModel.DeclineConnectingToHost();
        }


        private void AbortWaitButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            _viewModel.DisplayClientView();
        }


        private void SelectWhiteButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            _viewModel.SelectPlayerColor(EngineAPI.Side.White);
        }


        private void SelectBlackButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            _viewModel.SelectPlayerColor(EngineAPI.Side.Black);
        }


        private void ReadyButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();
            _viewModel.SetPlayerReady();
        }
    }
}
