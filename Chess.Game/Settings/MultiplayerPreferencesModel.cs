using Chess.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.UI.Settings
{
    public interface IMultiplayerPreferencesModel
    {
        void Init();

        ulong GetSelectedNetworkAdapterID();

        void ChangeNetworkAdapter(ulong adapterID);

        List<NetworkAdapter> GetNetworkAdapters();

        void SetLocalPlayerName(string playerName);

        string GetLocalPlayerName();


        event Action<string> PlayerNameChanged;
    }

    public enum AdapterVisibility
    {
        Hidden = 1,
        Visible = 2,
        Recommended = 3,
    }


    public record NetworkAdapter
    {
        public string AdapterName { get; set; }
        public string NetworkName { get; set; }
        public ulong ID { get; set; }
        public AdapterVisibility Visibility { get; set; }

        public bool IsValid()
        {
            return ID != 0 && !string.IsNullOrWhiteSpace(NetworkName);
        }

        public bool IsVisible()
        {
            return Visibility != AdapterVisibility.Hidden;
        }

        public bool IsRecommended()
        {
            return Visibility == AdapterVisibility.Recommended;
        }
    }


    public class MultiplayerPreferencesModel : IMultiplayerPreferencesModel
    {
        private readonly ISettingsService _settingsService;
        private readonly INetworkService _networkService;
        private readonly List<NetworkAdapter> _adapters = [];


        public MultiplayerPreferencesModel(ISettingsService settingsService, INetworkService networkService)
        {
            _settingsService = settingsService;
            _networkService = networkService;
        }


        public void Init()
        {
            SetNetworkAdapters();
        }


        #region Network

        private bool SetNetworkAdapters()
        {
            _adapters.Clear();

            foreach (NetworkAdapter networkAdapter in _networkService.GetNetworkAdapters())
            {
                if (!networkAdapter.IsValid())
                    continue;

                _adapters.Add(networkAdapter);
            }

            bool result = _adapters.Count > 0;
            return result;
        }


        public List<NetworkAdapter> GetNetworkAdapters()
        {
            return _adapters;
        }


        public ulong GetSelectedNetworkAdapterID()
        {
            return _networkService.ActiveAdapterId;
        }


        public void ChangeNetworkAdapter(ulong ID)
        {
            _networkService.SetActiveAdapter(ID);
        }

        #endregion


        #region Player Name

        public void SetLocalPlayerName(string name)
        {
            // Others see the new name once networking starts again
            _settingsService.PlayerName = name;
            PlayerNameChanged?.Invoke(name);
        }


        public string GetLocalPlayerName()
        {
            return _settingsService.PlayerName;
        }

        #endregion

        public event Action<string> PlayerNameChanged;

    }
}
