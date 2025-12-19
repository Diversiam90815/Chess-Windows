using Chess.UI.Models;
using Chess.UI.Multiplayer;
using Chess.UI.Services;
using Chess.UI.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
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

        public MultiplayerMode MPMode { get; private set; }

        public event Action RequestNavigationToChessboard;
        public event Action RequestCloseChessboard;

        // Events for audio feedback
        public event Action ButtonClicked;

#pragma warning disable CS0067 // Event is never used - planned for future chat feature
        public event Action ChatMessageReceived;    // TODO: Not yet implemented
#pragma warning restore CS0067

        public MultiplayerViewModel(IDispatcherQueueWrapper dispatcher)
        {
            _dispatcherQueue = dispatcher;

            _model = App.Current.Services.GetService<IMultiplayerModel>();
            _model.Init();

            _preferencesModel = App.Current.Services.GetService<IMultiplayerPreferencesModel>();
            _preferencesModel.PlayerNameChanged += HandlePlayerNameChanged; // Subscribe to player name changes
            LocalPlayerName = _preferencesModel.GetLocalPlayerName();       // and also initialize the value at first

            _model.OnConnectionErrorOccured += HandleConnectionError;
            _model.OnConnectionStatusChanged += HandleConnectionStatusUpdated;
            _model.OnPlayerChanged += HandlePlayerChanged;
            _model.OnMultiplayerPlayerSetFromRemote += SelectLocalPlayerFromRemote;
        }


        public void StartMultiplayerSetup()
        {
            _model.StartMultiplayer();
        }


        public void DisconnectMultiplayer()
        {
            _model.DisconnectMultiplayer();
        }


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


        private bool _hostGameButtonEnabled = true;
        public bool HostGameButtonEnabled
        {
            get => _hostGameButtonEnabled;
            set
            {
                if (_hostGameButtonEnabled != value)
                {
                    _hostGameButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _clientGameButtonEnabled = true;
        public bool ClientGameButtonEnabled
        {
            get => _clientGameButtonEnabled;
            set
            {
                if (_clientGameButtonEnabled != value)
                {
                    _clientGameButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }


        private EngineAPI.PlayerColor _localPlayer;
        public EngineAPI.PlayerColor LocalPlayer
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


        private Visibility _clientFoundHostView = Visibility.Collapsed;
        public Visibility ClientFoundHostView
        {
            get => _clientFoundHostView;
            set
            {
                if (_clientFoundHostView != value)
                {
                    _clientFoundHostView = value;
                    OnPropertyChanged();
                }
            }
        }


        private Visibility _clientRequestedConnectionView = Visibility.Collapsed;
        public Visibility ClientRequestedConnectionView
        {
            get => _clientRequestedConnectionView;
            set
            {
                if (_clientRequestedConnectionView != value)
                {
                    _clientRequestedConnectionView = value;
                    OnPropertyChanged();
                }
            }
        }


        private Visibility _clientWaitingForResponseView = Visibility.Collapsed;
        public Visibility ClientWaitingForResponseView
        {
            get => _clientWaitingForResponseView;
            set
            {
                if (_clientWaitingForResponseView != value)
                {
                    _clientWaitingForResponseView = value;
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

        public void OnButtonClicked() => ButtonClicked?.Invoke();


        private void HandlePlayerNameChanged(string newName)
        {
            LocalPlayerName = newName;
        }


        public void HandlePlayerChanged(EngineAPI.PlayerColor player)
        {
            IsLocalPlayersTurn = player == LocalPlayer;
        }


        public void EnterServerMultiplayerMode()
        {
            MPMode = MultiplayerMode.Server;
            _model.StartGameServer();
            DisplayServerView();
        }


        public void EnterClientMultiplayerMode()
        {
            MPMode = MultiplayerMode.Client;
            _model.StartGameClient();
            DisplayClientView();
        }


        public void EnterInitMode()
        {
            _model.ResetToInit();
            DisplayInitView();
        }


        private void EnterMultiplayerGame()
        {
            _model.StartMultiplerGame();
        }


        public void AcceptConnectingToHost()
        {
            //We are the client and accepted a connection to the host
            _model.ConnectToHost();
            DisplayClientWaitingForResponseView();
        }


        public void DeclineConnectingToHost()
        {
            //We are the client and declined a connection to the host
            EnterInitMode();
        }


        public void AcceptClientConnection()
        {
            //We are the host and accepted a connection try from the client
            _model.AnswerConnectionInvitation(true);
        }


        public void DeclineClientConnection()
        {
            //We are the host and declined a connection try from the client
            _model.AnswerConnectionInvitation(false);

            EnterInitMode();
        }


        public void SelectPlayerColor(EngineAPI.PlayerColor color)
        {
            LocalPlayer = color;
            // Send color selection to backend
            _model.SetLocalPlayerColor(color);

            ReadyButtonEnabled = true;
        }


        public void SelectLocalPlayerFromRemote(EngineAPI.PlayerColor local)
        {
            LocalPlayer = local;
            _model.SetLocalPlayerColor(local);
            IsReady = false;

            ReadyButtonEnabled = true;
        }


        public void SetPlayerReady()
        {
            IsReady = true;
        }


        private void HandleConnectionError(string errorMessage)
        {
            // TODO: Show error message. For now we just log
            Logger.LogError($"Connection Error message received: {errorMessage}");
        }


        private void HandleConnectionStatusUpdated(EngineAPI.ConnectionState state, string remotePlayerName)
        {
            switch (state)
            {
                case EngineAPI.ConnectionState.ConnectionRequested:
                    {
                        RemotePlayerName = remotePlayerName;
                        DisplayClientRequestedConnectionView();
                        break;
                    }
                case EngineAPI.ConnectionState.Disconnected:
                    {
                        RequestCloseChessboard?.Invoke();
                        EnterInitMode();
                        break;
                    }
                case EngineAPI.ConnectionState.ClientFoundHost:
                    {
                        RemotePlayerName = remotePlayerName;
                        DisplayClientFoundHostView();
                        break;
                    }
                case EngineAPI.ConnectionState.SetPlayerColor:
                    {
                        DisplaySettingPlayerColorView();
                        break;
                    }
                case EngineAPI.ConnectionState.GameStarted:
                    {
                        EnterMultiplayerGame();
                        RequestNavigationToChessboard?.Invoke();
                        break;
                    }
                default: break;
            }
        }


        public void UpdateMPButtons(MultiplayerMode mode)
        {
            switch (mode)
            {
                case MultiplayerMode.None:
                    {
                        HostGameButtonEnabled = false;
                        ClientGameButtonEnabled = false;
                        break;
                    }
                case MultiplayerMode.Init:
                    {
                        HostGameButtonEnabled = true;
                        ClientGameButtonEnabled = true;
                        break;
                    }
                case MultiplayerMode.Server:
                    {
                        HostGameButtonEnabled = true;
                        ClientGameButtonEnabled = false;
                        break;
                    }
                case MultiplayerMode.Client:
                    {
                        HostGameButtonEnabled = false;
                        ClientGameButtonEnabled = true;
                        break;
                    }
                default:
                    {
                        HostGameButtonEnabled = false;
                        ClientGameButtonEnabled = false;
                        break;
                    }
            }
        }


        public void ResetViewState()
        {
            // Reset all view states to initial values
            InitView = Visibility.Visible;
            ClientFoundHostView = Visibility.Collapsed;
            ClientRequestedConnectionView = Visibility.Collapsed;
            ClientWaitingForResponseView = Visibility.Collapsed;
            SettingLocalPlayerView = Visibility.Collapsed;

            // Reset other properties to defaults
            Processing = false;
            LocalPlayer = EngineAPI.PlayerColor.NoColor;
            IsReady = false;
            RemotePlayerReady = false;
            ReadyButtonEnabled = false;
            RemotePlayerName = string.Empty;

            // Reset to init mode
            MPMode = MultiplayerMode.Init;
            UpdateMPButtons(MultiplayerMode.Init);
        }


        public void DisplayClientView()
        {
            InitView = Visibility.Visible;

            Processing = true;
            UpdateMPButtons(MultiplayerMode.Client);

            ClientFoundHostView = Visibility.Collapsed;
            ClientRequestedConnectionView = Visibility.Collapsed;
            ClientWaitingForResponseView = Visibility.Collapsed;
        }


        public void DisplayServerView()
        {
            InitView = Visibility.Visible;

            Processing = true;
            UpdateMPButtons(MultiplayerMode.Server);

            ClientFoundHostView = Visibility.Collapsed;
            ClientRequestedConnectionView = Visibility.Collapsed;
            ClientWaitingForResponseView = Visibility.Collapsed;
        }


        public void DisplayInitView()
        {
            InitView = Visibility.Visible;

            Processing = false;
            UpdateMPButtons(MultiplayerMode.Init);

            ClientFoundHostView = Visibility.Collapsed;
            ClientRequestedConnectionView = Visibility.Collapsed;
            ClientWaitingForResponseView = Visibility.Collapsed;
        }


        public void DisplayClientWaitingForResponseView()
        {
            InitView = Visibility.Collapsed;

            Processing = false;
            UpdateMPButtons(MultiplayerMode.Client);

            ClientFoundHostView = Visibility.Collapsed;
            ClientRequestedConnectionView = Visibility.Collapsed;
            ClientWaitingForResponseView = Visibility.Visible;
        }


        public void DisplayClientFoundHostView()
        {
            InitView = Visibility.Collapsed;

            Processing = false;
            UpdateMPButtons(MultiplayerMode.Server);

            ClientFoundHostView = Visibility.Visible;
            ClientRequestedConnectionView = Visibility.Collapsed;
            ClientWaitingForResponseView = Visibility.Collapsed;
        }


        public void DisplayClientRequestedConnectionView()
        {
            InitView = Visibility.Collapsed;

            Processing = false;
            UpdateMPButtons(MultiplayerMode.Client);

            ClientFoundHostView = Visibility.Collapsed;
            ClientRequestedConnectionView = Visibility.Visible;
            ClientWaitingForResponseView = Visibility.Collapsed;
        }


        public void DisplaySettingPlayerColorView()
        {
            InitView = Visibility.Collapsed;
            ClientFoundHostView = Visibility.Collapsed;
            ClientRequestedConnectionView = Visibility.Collapsed;
            ClientWaitingForResponseView = Visibility.Collapsed;
            SettingLocalPlayerView = Visibility.Visible;

            // Reset selection state
            LocalPlayer = EngineAPI.PlayerColor.NoColor;
            IsReady = false;
            RemotePlayerReady = false;

            Processing = false;
            UpdateMPButtons(MultiplayerMode.None);
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
