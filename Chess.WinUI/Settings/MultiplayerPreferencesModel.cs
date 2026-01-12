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

        int GetSelectedNetworkAdapterID();

        void ChangeNetworkAdapter(int adapterID);

        List<NetworkAdapter> GetNetworkAdapters();

        void SetLocalPlayerName(string playerName);

        string GetLocalPlayerName();


        event Action<string> PlayerNameChanged;
    }

    public enum AdapterType
    {
        Ethernet = 1,
        WiFi = 2,
        Loopback = 3,
        Virtual = 4,
        Other = 5,
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
        public int ID { get; set; }
        public AdapterVisibility Visibility { get; set; }
        public AdapterType Type { get; set; }

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
        private readonly List<NetworkAdapter> _adapters = [];


        public void Init()
        {
            SetNetworkAdapters();
        }


        #region Network

        private bool SetNetworkAdapters()
        {
            _adapters.Clear();
            int adapterCount = EngineAPI.GetNetworkAdapterCount();

            for (uint i = 0; i < adapterCount; ++i)
            {
                EngineAPI.GetNetworkAdapterAtIndex(i, out EngineAPI.NetworkAdapterInstance adapter);

                NetworkAdapter networkAdapter = new()
                {
                    AdapterName = adapter.AdapterName,
                    NetworkName = adapter.NetworkName,
                    ID = adapter.ID,
                    Visibility = (AdapterVisibility)adapter.Visibility,
                    Type = (AdapterType)adapter.Type,

                };

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


        public int GetSelectedNetworkAdapterID()
        {
            return EngineAPI.GetSavedAdapterID();
        }


        public void ChangeNetworkAdapter(int ID)
        {
            EngineAPI.ChangeCurrentAdapter(ID);
        }

        #endregion


        #region Player Name

        public void SetLocalPlayerName(string name)
        {
            EngineAPI.SetLocalPlayerName(name);
            PlayerNameChanged?.Invoke(name);
        }


        public string GetLocalPlayerName()
        {
            string localPlayerName = EngineAPI.GetLocalPlayerName();
            return localPlayerName;
        }

        #endregion

        public event Action<string> PlayerNameChanged;

    }
}
