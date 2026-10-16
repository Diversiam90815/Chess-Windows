using Chess.UI.Settings;
using Chess.UI.Wrappers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static Chess.UI.Services.NetLinkAPI;


namespace Chess.UI.Services
{
    public record NetworkPeer(ulong Id, string Name);


    /// <summary>
    /// Finds other players on the local network and exchanges messages with them, through NetLink.
    /// Knows nothing about chess: what the messages mean is up to its users.
    /// </summary>
    public interface INetworkService : IDisposable
    {
        // Adapters can be listed and chosen before Start()
        IReadOnlyList<NetworkAdapter> GetNetworkAdapters();
        ulong ActiveAdapterId { get; }
        bool SetActiveAdapter(ulong adapterId);

        bool IsRunning { get; }
        bool Start(string displayName);
        void Stop();

        bool StartDiscovery();
        void StopDiscovery();

        bool Connect(ulong peer);
        void Accept(ulong peer);
        void Decline(ulong peer);
        void Disconnect(ulong peer);

        bool Send(ulong peer, uint type, byte[] data);

        // Raised on the UI thread
        event Action<NetworkPeer> PeerDiscovered;
        event Action<ulong> PeerLost;
        event Action<NetworkPeer> ConnectionRequested;
        event Action<NetworkPeer> Connected;
        event Action<ulong, DisconnectReason> Disconnected;
        event Action<ulong, uint, byte[]> MessageReceived;
        event Action<NetworkAdapter> ActiveAdapterChanged;
    }


    public sealed class NetworkService : INetworkService
    {
        // Only instances of this application with the same protocol version see each other
        public const string AppId = "com.diversiam.chess";
        public const string ProtocolVersion = "1.0";

        private readonly IDispatcherQueueWrapper _dispatcherQueue;
        private readonly ISettingsService _settingsService;
        private readonly object _lock = new();

        private nint _netlink;
        private bool _isRunning;

        // NetLink calls these from its event thread: they must live as long as the instance
        private readonly PeerCallback _onPeerDiscovered;
        private readonly PeerIdCallback _onPeerLost;
        private readonly PeerCallback _onConnectionRequest;
        private readonly PeerCallback _onConnected;
        private readonly DisconnectedCallback _onDisconnected;
        private readonly MessageCallback _onMessage;
        private readonly AdapterCallback _onAdapterChanged;
        private readonly LogCallback _onLog;


        public NetworkService(IDispatcherQueueWrapper dispatcherQueue, ISettingsService settingsService)
        {
            _dispatcherQueue = dispatcherQueue;
            _settingsService = settingsService;

            _onPeerDiscovered = (_, peer) => Raise(PeerDiscovered, ToPeer(peer));
            _onPeerLost = (_, peer) => Raise(PeerLost, peer);
            _onConnectionRequest = (_, peer) => Raise(ConnectionRequested, ToPeer(peer));
            _onConnected = (_, peer) => Raise(Connected, ToPeer(peer));
            _onDisconnected = (_, peer, reason) => RaiseOnUIThread(() => Disconnected?.Invoke(peer, (DisconnectReason)reason));
            _onMessage = OnMessage;
            _onAdapterChanged = (_, adapter) => Raise(ActiveAdapterChanged, ToAdapter(Marshal.PtrToStructure<NLNetworkAdapter>(adapter)));
            _onLog = OnLog;
        }


        private nint Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_netlink == 0)
                    {
                        _netlink = NetLinkAPI.Create();
                        Logger.LogInfo($"NetLink {NetLinkAPI.Version()} loaded");

                        // The adapter chosen before, if this machine still has it. Otherwise NetLink picks the preferred one.
                        ulong savedAdapter = _settingsService.NetworkAdapterId;
                        if (savedAdapter != 0 && !NetLinkAPI.SetActiveAdapter(_netlink, savedAdapter))
                            Logger.LogInfo("The saved network adapter is not available anymore");
                    }

                    return _netlink;
                }
            }
        }


        #region Adapters

        public IReadOnlyList<NetworkAdapter> GetNetworkAdapters()
        {
            // NetLink reports how many there are: the list may grow between the calls, so ask until it fits
            int count = NetLinkAPI.GetAdapters(Instance, null, 0);
            var buffer = new NLNetworkAdapter[Math.Max(count, 1)];

            while ((count = NetLinkAPI.GetAdapters(Instance, buffer, buffer.Length)) > buffer.Length)
                buffer = new NLNetworkAdapter[count];

            var adapters = new List<NetworkAdapter>(count);

            for (int i = 0; i < count; ++i)
                adapters.Add(ToAdapter(buffer[i]));

            return adapters;
        }


        public ulong ActiveAdapterId => NetLinkAPI.GetActiveAdapterId(Instance);


        public bool SetActiveAdapter(ulong adapterId)
        {
            if (!NetLinkAPI.SetActiveAdapter(Instance, adapterId))
            {
                Logger.LogWarning($"Network adapter {adapterId} is not available");
                return false;
            }

            _settingsService.NetworkAdapterId = adapterId;
            return true;
        }

        #endregion


        #region Lifecycle

        public bool IsRunning => _isRunning;


        public bool Start(string displayName)
        {
            if (_isRunning)
                return true;

            var config = new NLConfig
            {
                DisplayName = displayName ?? string.Empty,
                AppId = AppId,
                AppVersion = ProtocolVersion,
                DiscoveryPort = (ushort)_settingsService.DiscoveryUDPPort,
            };

            var callbacks = new NLCallbacks
            {
                OnPeerDiscovered = Marshal.GetFunctionPointerForDelegate(_onPeerDiscovered),
                OnPeerLost = Marshal.GetFunctionPointerForDelegate(_onPeerLost),
                OnConnectionRequest = Marshal.GetFunctionPointerForDelegate(_onConnectionRequest),
                OnConnected = Marshal.GetFunctionPointerForDelegate(_onConnected),
                OnDisconnected = Marshal.GetFunctionPointerForDelegate(_onDisconnected),
                OnMessage = Marshal.GetFunctionPointerForDelegate(_onMessage),
                OnNetworkAdapterChanged = Marshal.GetFunctionPointerForDelegate(_onAdapterChanged),
                OnLog = Marshal.GetFunctionPointerForDelegate(_onLog),
            };

            _isRunning = NetLinkAPI.Start(Instance, ref config, ref callbacks);

            if (_isRunning)
                Logger.LogInfo($"Networking started as '{config.DisplayName}' on port {config.DiscoveryPort}");
            else
                Logger.LogError("Networking could not be started");

            return _isRunning;
        }


        public void Stop()
        {
            if (!_isRunning)
                return;

            NetLinkAPI.Stop(Instance);
            _isRunning = false;

            Logger.LogInfo("Networking stopped");
        }


        public void Dispose()
        {
            lock (_lock)
            {
                if (_netlink == 0)
                    return;

                // Stops it as well: the connected peer is told
                NetLinkAPI.Destroy(_netlink);
                _netlink = 0;
                _isRunning = false;
            }
        }

        #endregion


        #region Discovery & Sessions

        public bool StartDiscovery() => NetLinkAPI.StartDiscovery(Instance);

        public void StopDiscovery() => NetLinkAPI.StopDiscovery(Instance);

        public bool Connect(ulong peer) => NetLinkAPI.Connect(Instance, peer);

        public void Accept(ulong peer) => NetLinkAPI.Accept(Instance, peer);

        public void Decline(ulong peer) => NetLinkAPI.Decline(Instance, peer);

        public void Disconnect(ulong peer) => NetLinkAPI.Disconnect(Instance, peer);


        public bool Send(ulong peer, uint type, byte[] data)
        {
            data ??= [];

            var result = (SendResult)NetLinkAPI.Send(Instance, peer, type, data, (nuint)data.Length, (int)Lane.Reliable, 0);

            if (result != SendResult.Queued)
                Logger.LogWarning($"Message {type} to peer {peer} not sent: {result}");

            return result == SendResult.Queued;
        }

        #endregion


        #region NetLink Callbacks

        private void OnMessage(nint context, ulong from, int lane, uint type, nint data, nuint size)
        {
            // Only valid during the call
            byte[] payload = new byte[(int)size];

            if (size > 0)
                Marshal.Copy(data, payload, 0, payload.Length);

            RaiseOnUIThread(() => MessageReceived?.Invoke(from, type, payload));
        }


        private void OnLog(nint context, int level, nint message)
        {
            string text = $"NetLink: {Marshal.PtrToStringUTF8(message)}";

            switch ((LogLevel)level)
            {
                case LogLevel.Error: Logger.LogError(text); break;
                case LogLevel.Warning: Logger.LogWarning(text); break;
                case LogLevel.Info: Logger.LogInfo(text); break;
                default: Logger.LogDebug(text); break;
            }
        }


        private static NetworkPeer ToPeer(nint peer)
        {
            var info = Marshal.PtrToStructure<NLPeerInfo>(peer);
            return new NetworkPeer(info.Id, info.DisplayName);
        }


        private static NetworkAdapter ToAdapter(NLNetworkAdapter adapter)
        {
            return new NetworkAdapter
            {
                ID = adapter.Id,
                AdapterName = adapter.AdapterName,
                NetworkName = adapter.NetworkName,
                Visibility = (AdapterVisibility)adapter.Priority,
            };
        }


        private void Raise<T>(Action<T> handler, T value) => RaiseOnUIThread(() => handler?.Invoke(value));


        private void RaiseOnUIThread(Action raiseEvent)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    raiseEvent();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error in a handler of a network event: {ex.Message}");
                }
            });
        }

        #endregion


        public event Action<NetworkPeer> PeerDiscovered;
        public event Action<ulong> PeerLost;
        public event Action<NetworkPeer> ConnectionRequested;
        public event Action<NetworkPeer> Connected;
        public event Action<ulong, DisconnectReason> Disconnected;
        public event Action<ulong, uint, byte[]> MessageReceived;
        public event Action<NetworkAdapter> ActiveAdapterChanged;
    }
}
