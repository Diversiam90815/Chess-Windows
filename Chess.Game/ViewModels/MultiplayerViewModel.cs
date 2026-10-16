using Chess.UI.Models;
using Chess.UI.Services;
using Chess.UI.Settings;
using Chess.UI.Wrappers;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace Chess.UI.ViewModels
{
    public partial class MultiplayerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IDispatcherQueueWrapper _dispatcherQueue;

        private readonly IMultiplayerModel _model;

        private readonly IMultiplayerPreferencesModel _preferencesModel;

        public event Action RequestNavigationToChessboard;
        public event Action RequestCloseChessboard;

        public event Action ButtonClicked;

#pragma warning disable CS0067
        public event Action ChatMessageReceived;
#pragma warning restore CS0067

        public ObservableCollection<string> DiscoveredOpponents { get; } = new();


        public MultiplayerViewModel(IDispatcherQueueWrapper dispatcher, IMultiplayerModel multiplayerModel, IMultiplayerPreferencesModel preferencesModel)
        {
            _dispatcherQueue = dispatcher;
            _model = multiplayerModel;
            _preferencesModel = preferencesModel;

            _preferencesModel.PlayerNameChanged += HandlePlayerNameChanged;
            LocalPlayerName = _preferencesModel.GetLocalPlayerName();

            _model.OnConnectionErrorOccured += HandleConnectionError;
            _model.OnMultiplayerStateChanged += HandleMultiplayerStateUpdated;
            _model.OnOpponentDiscovered += HandleOpponentDiscovered;
            _model.OnPlayerChanged += HandlePlayerChanged;
            _model.OnMultiplayerPlayerSetFromRemote += SelectLocalPlayerFromRemote;
        }


        public void StartMultiplayerSetup()
        {
            _model.StartMultiplayer();
        }


        #region Properties

        private bool _isLocalPlayersTurn = false;
        public bool IsLocalPlayersTurn
        {
            get => _isLocalPlayersTurn;
            set
            {
                if (_isLocalPlayersTurn != value)
                {
                    _isLocalPlayersTurn = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _processing = false;
        public bool Processing
        {
            get => _processing;
            set
            {
                if (_processing != value)
                {
                    _processing = value;
                    SettingsEditable = !value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _settingsEditable = true;
        public bool SettingsEditable
        {
            get => _settingsEditable;
            set
            {
                if (_settingsEditable != value)
                {
                    _settingsEditable = value;
                    OnPropertyChanged();
                }
            }
        }


        private EngineAPI.Side _localPlayer;
        public EngineAPI.Side LocalPlayer
        {
            get => _localPlayer;
            set
            {
                if (_localPlayer != value)
                {
                    _localPlayer = value;
                    OnPropertyChanged();
                }
            }
        }


        private string _remotePlayerName;
        public string RemotePlayerName
        {
            get => _remotePlayerName;
            set
            {
                if (_remotePlayerName != value)
                {
                    _remotePlayerName = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _isReady = false;
        public bool IsReady
        {
            get => _isReady;
            set
            {
                if (_isReady != value)
                {
                    _isReady = value;
                    _model.SetPlayerReady(value);

                    OnPropertyChanged();
                }
            }
        }


        private bool _remotePlayerReady = false;
        public bool RemotePlayerReady
        {
            get => _remotePlayerReady;
            set
            {
                if (_remotePlayerReady != value)
                {
                    _remotePlayerReady = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _readyButtonEnabled;
        public bool ReadyButtonEnabled
        {
            get => _readyButtonEnabled;
            set
            {
                if (_readyButtonEnabled != value)
                {
                    _readyButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _localPlayerName;
        public string LocalPlayerName
        {
            get => _localPlayerName;
            set
            {
                if (_localPlayerName != value)
                {
                    _localPlayerName = value;
                    OnPropertyChanged();
                }
            }
        }


        private int _selectedOpponentIndex = -1;
        public int SelectedOpponentIndex
        {
            get => _selectedOpponentIndex;
            set
            {
                if (_selectedOpponentIndex != value)
                {
                    _selectedOpponentIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion


        #region Visibilities

        private Visibility _initView = Visibility.Visible;
        public Visibility InitView
        {
            get => _initView;
            set
            {
                if (_initView != value)
                {
                    _initView = value;
                    OnPropertyChanged();
                }
            }
        }


        private Visibility _searchingView = Visibility.Collapsed;
        public Visibility SearchingView
        {
            get => _searchingView;
            set
            {
                if (_searchingView != value)
                {
                    _searchingView = value;
                    OnPropertyChanged();
                }
            }
        }


        private Visibility _opponentFoundView = Visibility.Collapsed;
        public Visibility OpponentFoundView
        {
            get => _opponentFoundView;
            set
            {
                if (_opponentFoundView != value)
                {
                    _opponentFoundView = value;
                    OnPropertyChanged();
                }
            }
        }


        private Visibility _connectionRequestedView = Visibility.Collapsed;
        public Visibility ConnectionRequestedView
        {
            get => _connectionRequestedView;
            set
            {
                if (_connectionRequestedView != value)
                {
                    _connectionRequestedView = value;
                    OnPropertyChanged();
                }
            }
        }


        private Visibility _waitingForResponseView = Visibility.Collapsed;
        public Visibility WaitingForResponseView
        {
            get => _waitingForResponseView;
            set
            {
                if (_waitingForResponseView != value)
                {
                    _waitingForResponseView = value;
                    OnPropertyChanged();
                }
            }
        }


        private Visibility _settingLocalPlayerView = Visibility.Collapsed;
        public Visibility SettingLocalPlayerView
        {
            get => _settingLocalPlayerView;
            set
            {
                if (_settingLocalPlayerView != value)
                {
                    _settingLocalPlayerView = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion


        #region Actions

        public void OnButtonClicked() => ButtonClicked?.Invoke();


        public void FindOpponent()
        {
            _model.FindOpponent();
            DisplaySearchingView();
        }


        public void ConnectToOpponent()
        {
            if (SelectedOpponentIndex < 0) return;

            RemotePlayerName = DiscoveredOpponents[SelectedOpponentIndex];
            _model.ConnectToOpponent(SelectedOpponentIndex);
            DisplayWaitingForResponseView();
        }


        public void AcceptConnectionRequest()
        {
            _model.RespondToConnectionRequest(true);
        }


        public void DeclineConnectionRequest()
        {
            _model.RespondToConnectionRequest(false);
            FindOpponent();
        }


        public void CancelSearch()
        {
            _model.CancelSearch();
            DiscoveredOpponents.Clear();
            SelectedOpponentIndex = -1;
            DisplayInitView();
        }


        public void SelectPlayerColor(EngineAPI.Side color)
        {
            LocalPlayer = color;
            _model.SetLocalPlayerColor(color);
            ReadyButtonEnabled = true;
        }


        public void SetPlayerReady()
        {
            IsReady = true;
        }

        #endregion


        #region Event Handlers

        private void HandlePlayerNameChanged(string newName)
        {
            LocalPlayerName = newName;
        }


        public void HandlePlayerChanged(EngineAPI.Side player)
        {
            IsLocalPlayersTurn = player == LocalPlayer;
        }


        private void SelectLocalPlayerFromRemote(EngineAPI.Side local)
        {
            LocalPlayer = local;
            _model.SetLocalPlayerColor(local);
            IsReady = false;

            ReadyButtonEnabled = true;
        }


        private void HandleConnectionError(string errorMessage)
        {
            Logger.LogError($"Connection Error message received: {errorMessage}");
        }


        private void HandleMultiplayerStateUpdated(MultiplayerState state, string remotePlayerName)
        {
            switch (state)
            {
                case MultiplayerState.Searching:
                    DisplaySearchingView();
                    break;

                case MultiplayerState.ConnectionRequested:
                    RemotePlayerName = remotePlayerName;
                    DisplayConnectionRequestedView();
                    break;

                case MultiplayerState.PlayerSetup:
                    DisplaySettingPlayerColorView();
                    break;

                case MultiplayerState.InGame:
                    RequestNavigationToChessboard?.Invoke();
                    break;

                case MultiplayerState.Disconnected:
                    RequestCloseChessboard?.Invoke();
                    DiscoveredOpponents.Clear();
                    SelectedOpponentIndex = -1;
                    DisplayInitView();
                    break;

                case MultiplayerState.Connected:
                case MultiplayerState.ReadyToStart:
                    break;

                default:
                    break;
            }
        }


        private void HandleOpponentDiscovered(string opponentName)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                if (!DiscoveredOpponents.Contains(opponentName))
                {
                    DiscoveredOpponents.Add(opponentName);
                }

                if (DiscoveredOpponents.Count == 1)
                {
                    SelectedOpponentIndex = 0;
                }

                DisplayOpponentFoundView();
            });
        }

        #endregion


        #region Display Methods

        private void CollapseAllViews()
        {
            InitView = Visibility.Collapsed;
            SearchingView = Visibility.Collapsed;
            OpponentFoundView = Visibility.Collapsed;
            ConnectionRequestedView = Visibility.Collapsed;
            WaitingForResponseView = Visibility.Collapsed;
            SettingLocalPlayerView = Visibility.Collapsed;
        }


        public void DisplayInitView()
        {
            CollapseAllViews();
            InitView = Visibility.Visible;
            Processing = false;
        }


        public void DisplaySearchingView()
        {
            CollapseAllViews();
            SearchingView = Visibility.Visible;
            Processing = true;
        }


        public void DisplayOpponentFoundView()
        {
            CollapseAllViews();
            OpponentFoundView = Visibility.Visible;
            Processing = false;
        }


        public void DisplayConnectionRequestedView()
        {
            CollapseAllViews();
            ConnectionRequestedView = Visibility.Visible;
            Processing = false;
        }


        public void DisplayWaitingForResponseView()
        {
            CollapseAllViews();
            WaitingForResponseView = Visibility.Visible;
            Processing = true;
        }


        public void DisplaySettingPlayerColorView()
        {
            CollapseAllViews();
            SettingLocalPlayerView = Visibility.Visible;

            LocalPlayer = EngineAPI.Side.None;
            IsReady = false;
            RemotePlayerReady = false;
            Processing = false;
        }


        public void ResetViewState()
        {
            CollapseAllViews();
            InitView = Visibility.Visible;

            Processing = false;
            LocalPlayer = EngineAPI.Side.None;
            IsReady = false;
            RemotePlayerReady = false;
            ReadyButtonEnabled = false;
            RemotePlayerName = string.Empty;
            DiscoveredOpponents.Clear();
            SelectedOpponentIndex = -1;
        }

        #endregion


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }
}
