using Chess.UI.Multiplayer;
using Chess.UI.UI;
using Chess.UI.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;


namespace Chess.UI.ViewModels
{
    public partial class MultiplayerPreferencesViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IDispatcherQueueWrapper _dispatcherQueue;

        private readonly IMultiplayerPreferencesModel _model;

        public event Action ItemSelected;
        private bool _userInteractionEnabled = false;



        public MultiplayerPreferencesViewModel(IDispatcherQueueWrapper dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;

            NetworkAdapters = [];

            _model = App.Current.Services.GetService<IMultiplayerPreferencesModel>();
            _model.Init();

            UpdateAdapterBox();
            SetLocalNameFromBackend();
        }


        #region Network
        public ObservableCollection<IComboNode> NetworkAdapterNodes { get; } = new();

        private ObservableCollection<NetworkAdapter> _networkAdapters;
        public ObservableCollection<NetworkAdapter> NetworkAdapters
        {
            get => _networkAdapters;
            set
            {
                if (_networkAdapters != value)
                {
                    _networkAdapters = value;
                    OnPropertyChanged();
                }
            }
        }


        private NetworkAdapter _selectedAdapter;
        public NetworkAdapter SelectedAdapter
        {
            get => _selectedAdapter;
            set
            {
                if (_selectedAdapter != value)
                {
                    _selectedAdapter = value;
                    _model.ChangeNetworkAdapter(SelectedAdapter.ID);
                    OnPropertyChanged();

                    if (_userInteractionEnabled)
                        ItemSelected?.Invoke();
                }
            }
        }


        public NetworkAdapterComboItem SelectedNetworkAdaperNode
        {
            get => NetworkAdapterNodes.OfType<NetworkAdapterComboItem>().FirstOrDefault(node => node.NetworkAdapter.Equals(SelectedAdapter));
            set
            {
                if (value?.NetworkAdapter != null && value.NetworkAdapter != SelectedAdapter)
                {
                    SelectedAdapter = value.NetworkAdapter;
                    OnPropertyChanged();
                }
            }
        }


        public void SelectPresavedNetworkAdapter()
        {
            int savedAdapterID = _model.GetSelectedNetworkAdapterID();

            if (savedAdapterID != 0)
            {
                for (int i = 0; i < NetworkAdapters.Count; i++)
                {
                    Multiplayer.NetworkAdapter adapter = NetworkAdapters[i];
                    if (adapter != null && adapter.ID == savedAdapterID)
                    {
                        SelectedAdapter = adapter;
                        _userInteractionEnabled = true;
                        return;
                    }
                }
            }
        }


        public void UpdateAdapterBox()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                NetworkAdapters.Clear();
                var adapters = _model.GetNetworkAdapters();

                foreach (var adapter in adapters)
                {
                    if (!NetworkAdapters.Contains(adapter))
                    {
                        NetworkAdapters.Add(adapter);
                    }
                }

                BuildNetworkAdaperNodes(adapters);

                SelectPresavedNetworkAdapter();
            });
        }


        private void BuildNetworkAdaperNodes(IEnumerable<NetworkAdapter> adapters)
        {
            NetworkAdapterNodes.Clear();

            // Add recommended adapters
            foreach (NetworkAdapter adapter in adapters.OrderBy(adapter => adapter.NetworkName, StringComparer.OrdinalIgnoreCase))
            {
                if (adapter.IsRecommended() && adapter.IsVisible())
                    NetworkAdapterNodes.Add(new NetworkAdapterComboItem(adapter));
            }

            // Add Divider and Heading if there are non-recommended adapters
            var nonRecommendedAdapters = adapters.Where(adapter => !adapter.IsRecommended() && adapter.IsVisible()).ToList();
            if (nonRecommendedAdapters.Any())
            {
                NetworkAdapterNodes.Add(new ComboDivider());
                NetworkAdapterNodes.Add(new ComboHeader("Not recommended:"));

                // Add not recommended but visible adapters
                foreach (NetworkAdapter adapter in nonRecommendedAdapters.OrderBy(adapter => adapter.NetworkName, StringComparer.OrdinalIgnoreCase))
                {
                    NetworkAdapterNodes.Add(new NetworkAdapterComboItem(adapter));
                }
            }
        }

        #endregion


        #region Player Name

        private string _localPlayerName;
        public string LocalPlayerName
        {
            get => _localPlayerName;
            set
            {
                if (_localPlayerName != value)
                {
                    _localPlayerName = value;
                    _model.SetLocalPlayerName(value);
                    OnPropertyChanged();
                }
            }
        }


        private void SetLocalNameFromBackend()
        {
            LocalPlayerName = _model.GetLocalPlayerName();
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
