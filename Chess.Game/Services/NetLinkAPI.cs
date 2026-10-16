using System;
using System.Runtime.InteropServices;
using System.Text;


namespace Chess.UI.Services
{
    /// <summary>
    /// The plain C API of NetLink (NetLinkC.h). Peers and adapters are identified by their 64-bit ID.
    /// </summary>
    public static class NetLinkAPI
    {
        private const string NETLINK_PATH = @"NetLinkC.dll";

        public const int MaxString = 256;


        #region DLL Defines

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_version")]
        private static extern nint VersionPtr();

        public static string Version() => Marshal.PtrToStringUTF8(VersionPtr()) ?? string.Empty;

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_create")]
        public static extern nint Create();

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_destroy")]
        public static extern void Destroy(nint netlink);

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_start")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Start(nint netlink, ref NLConfig config, ref NLCallbacks callbacks);

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_stop")]
        public static extern void Stop(nint netlink);

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_start_discovery")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool StartDiscovery(nint netlink);

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_stop_discovery")]
        public static extern void StopDiscovery(nint netlink);

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_connect")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Connect(nint netlink, ulong peer);

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_accept")]
        public static extern void Accept(nint netlink, ulong peer);

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_decline")]
        public static extern void Decline(nint netlink, ulong peer);

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_disconnect")]
        public static extern void Disconnect(nint netlink, ulong peer);

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_send")]
        public static extern int Send(nint netlink, ulong peer, uint type, byte[] data, nuint size, int lane, uint timeoutMs);

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_get_adapters")]
        public static extern int GetAdapters(nint netlink, [Out] NLNetworkAdapter[] adapters, int maxAdapters);

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_set_active_adapter")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SetActiveAdapter(nint netlink, ulong adapterId);

        [DllImport(NETLINK_PATH, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nl_get_active_adapter_id")]
        public static extern ulong GetActiveAdapterId(nint netlink);

        #endregion


        #region Callbacks

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PeerCallback(nint context, nint peer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PeerIdCallback(nint context, ulong peer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DisconnectedCallback(nint context, ulong peer, int reason);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MessageCallback(nint context, ulong from, int lane, uint type, nint data, nuint size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AdapterCallback(nint context, nint adapter);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LogCallback(nint context, int level, nint message);

        #endregion


        #region Structures & Enums

        public enum Lane
        {
            Reliable = 0,
            Bulk = 1,
            Media = 2,
        }

        public enum SendResult
        {
            Queued = 0,
            QueueFull = 1,
            TooLarge = 2,
            NotConnected = 3,
            NotRunning = 4,
            Invalid = 5,
        }

        public enum DisconnectReason
        {
            Local = 0,
            Remote = 1,
            Declined = 2,
            Incompatible = 3,
            Lost = 4,
            NetworkError = 5,
            Shutdown = 6,
        }

        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct NLPeerInfo
        {
            public ulong Id;
            public ushort Port;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxString)]
            private byte[] _displayName;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxString)]
            private byte[] _address;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxString)]
            private byte[] _appVersion;

            public readonly string DisplayName => FromUtf8(_displayName);
            public readonly string Address => FromUtf8(_address);
            public readonly string AppVersion => FromUtf8(_appVersion);
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct NLNetworkAdapter
        {
            public ulong Id;
            public int Priority;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxString)]
            private byte[] _adapterName;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxString)]
            private byte[] _networkName;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxString)]
            private byte[] _ipv4;

            public readonly string AdapterName => FromUtf8(_adapterName);
            public readonly string NetworkName => FromUtf8(_networkName);
            public readonly string IPv4 => FromUtf8(_ipv4);
        }


        // Zero for a number keeps NetLink's default
        [StructLayout(LayoutKind.Sequential)]
        public struct NLConfig
        {
            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public string DisplayName;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public string AppId;

            [MarshalAs(UnmanagedType.LPUTF8Str)]
            public string AppVersion;

            public ushort DiscoveryPort;
            public ulong SendQueueBytes;
            public uint MaxSendRate;
            public uint PeerTimeoutMs;
        }


        // Function pointers of the delegates above. NetLink copies them in nl_start; the delegates must stay alive while it runs.
        [StructLayout(LayoutKind.Sequential)]
        public struct NLCallbacks
        {
            public nint Context;
            public nint OnPeerDiscovered;
            public nint OnPeerLost;
            public nint OnConnectionRequest;
            public nint OnConnected;
            public nint OnDisconnected;
            public nint OnMessage;
            public nint OnNetworkAdapterChanged;
            public nint OnLog;
        }


        private static string FromUtf8(byte[] bytes)
        {
            if (bytes == null)
                return string.Empty;

            int length = Array.IndexOf(bytes, (byte)0);
            return Encoding.UTF8.GetString(bytes, 0, length >= 0 ? length : bytes.Length);
        }

        #endregion
    }
}
