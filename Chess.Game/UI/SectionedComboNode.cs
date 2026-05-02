using Chess.UI.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.UI.UI
{
    public enum ComboNodeKind
    {
        Item, Header, Divider
    }


    public interface IComboNode
    {
        ComboNodeKind Kind { get; }
        bool IsPlaceHolderItem { get; }
    }


    public sealed class ComboHeader : IComboNode
    {
        public ComboHeader(string text) { HeadingText = text; }

        public string HeadingText { get; }
        public ComboNodeKind Kind => ComboNodeKind.Header;
        public bool IsPlaceHolderItem => true;
    }

    public sealed class ComboDivider : IComboNode
    {
        public ComboNodeKind Kind => ComboNodeKind.Divider;
        public bool IsPlaceHolderItem => true;
    }

    public sealed class NetworkAdapterComboItem : IComboNode
    {
        public NetworkAdapterComboItem(NetworkAdapter adapter) { NetworkAdapter = adapter; }

        public NetworkAdapter NetworkAdapter { get; }
        public ComboNodeKind Kind => ComboNodeKind.Item;
        public bool IsPlaceHolderItem => false;
    }
}
