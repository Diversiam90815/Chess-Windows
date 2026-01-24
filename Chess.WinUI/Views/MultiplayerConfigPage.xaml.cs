using Chess.UI.Services;
using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace Chess.UI.Views
{


    public sealed partial class MultiplayerConfigPage : Page
    {
        private readonly MultiplayerViewModel _viewModel;

        private readonly IWindowSizeService _windowSizeService;


        public MultiplayerConfigPage()
        {
            this.InitializeComponent();
            //AppWindow.SetIcon(Project.IconPath);

            _viewModel = App.Current.Services.GetService<MultiplayerViewModel>();
            _windowSizeService = App.Current.Services.GetService<IWindowSizeService>();

            this.Rootgrid.DataContext = _viewModel;

            Init();
            //_windowSizeService.SetWindowSize(this, 600, 400);
            //_windowSizeService.SetWindowNonResizable(this);
        }


        private void Init()
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


        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OnButtonClicked();

            if (_viewModel.Processing)
            {
                _viewModel.EnterInitMode();
            }

            //else
            //{
            //    this.Close();
            //}
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

