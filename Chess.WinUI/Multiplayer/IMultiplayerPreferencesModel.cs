using System;
using System.Collections.Generic;


namespace Chess.UI.Multiplayer
{
    public interface IMultiplayerPreferencesModel
    {
        void Init();

        int GetSelectedNetworkAdapterID();

        void ChangeNetworkAdapter(int adapterID);

        List<Multiplayer.NetworkAdapter> GetNetworkAdapters();

        void SetLocalPlayerName(string playerName);

        string GetLocalPlayerName();


        event Action<string> PlayerNameChanged;
    }
}
