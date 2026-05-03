using Chess.UI.Services;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Chess.UI.Settings
{
    public interface ISettingsService
    {
        // Visual
        string BoardStyle { get; set; }
        string ChessPieceStyle { get; set; }

        // Player
        string PlayerName { get; set; }

        // Audio
        bool AudioSFXEnabled { get; set; }
        float AudioSFXVolume { get; set; }
        bool AudioAtmosEnabled { get; set; }
        float AudioAtmosVolume { get; set; }
        string AudioAtmosScenario { get; set; }
        float AudioMasterVolume { get; set; }

        // Network
        int DiscoveryUDPPort { get; set; }
    }


    internal class SettingsData
    {
        [JsonPropertyName("BoardStyle")]
        public string BoardStyle { get; set; } = "Wood";

        [JsonPropertyName("PieceStyle")]
        public string PieceStyle { get; set; } = "Basic";

        [JsonPropertyName("PlayerName")]
        public string PlayerName { get; set; } = "";

        [JsonPropertyName("Audio_SFX_Enabled")]
        public bool AudioSFXEnabled { get; set; } = true;

        [JsonPropertyName("Audio_SFX_Volume")]
        public float AudioSFXVolume { get; set; } = 1.0f;

        [JsonPropertyName("Audio_Atmos_Enabled")]
        public bool AudioAtmosEnabled { get; set; } = true;

        [JsonPropertyName("Audio_Atmos_Volume")]
        public float AudioAtmosVolume { get; set; } = 1.0f;

        [JsonPropertyName("Audio_Atmos_Scenario")]
        public string AudioAtmosScenario { get; set; } = "Forest";

        [JsonPropertyName("Audio_Master_Volume")]
        public float AudioMasterVolume { get; set; } = 1.0f;

        [JsonPropertyName("Discovery_UDP_Port")]
        public int DiscoveryUDPPort { get; set; } = 5555;
    }


    public class SettingsService : ISettingsService
    {
        private readonly string _configFilePath;
        private readonly object _fileLock = new();
        private SettingsData _data;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };


        public SettingsService()
        {
            string settingsFolder = Path.Combine(Project.AppDataDirectory, "Settings");
            _configFilePath = Path.Combine(settingsFolder, "Config.json");

            Directory.CreateDirectory(settingsFolder);

            _data = LoadOrCreateDefaults();
        }


        public string BoardStyle
        {
            get => _data.BoardStyle;
            set { _data.BoardStyle = value; Save(); }
        }

        public string ChessPieceStyle
        {
            get => _data.PieceStyle;
            set { _data.PieceStyle = value; Save(); }
        }

        public string PlayerName
        {
            get => _data.PlayerName;
            set
            {
                _data.PlayerName = value;
                Save();
                EngineAPI.SetLocalPlayerName(value ?? "");
            }
        }

        public bool AudioSFXEnabled
        {
            get => _data.AudioSFXEnabled;
            set { _data.AudioSFXEnabled = value; Save(); }
        }

        public float AudioSFXVolume
        {
            get => _data.AudioSFXVolume;
            set { _data.AudioSFXVolume = MathF.Round(value, 1); Save(); }
        }

        public bool AudioAtmosEnabled
        {
            get => _data.AudioAtmosEnabled;
            set { _data.AudioAtmosEnabled = value; Save(); }
        }

        public float AudioAtmosVolume
        {
            get => _data.AudioAtmosVolume;
            set { _data.AudioAtmosVolume = MathF.Round(value, 1); Save(); }
        }

        public string AudioAtmosScenario
        {
            get => _data.AudioAtmosScenario;
            set { _data.AudioAtmosScenario = value; Save(); }
        }

        public float AudioMasterVolume
        {
            get => _data.AudioMasterVolume;
            set { _data.AudioMasterVolume = MathF.Round(value, 1); Save(); }
        }

        public int DiscoveryUDPPort
        {
            get => _data.DiscoveryUDPPort;
            set
            {
                _data.DiscoveryUDPPort = value;
                Save();
                EngineAPI.SetDiscoveryPort(value);
            }
        }


        private SettingsData LoadOrCreateDefaults()
        {
            lock (_fileLock)
            {
                if (File.Exists(_configFilePath))
                {
                    try
                    {
                        string json = File.ReadAllText(_configFilePath);
                        var data = JsonSerializer.Deserialize<SettingsData>(json);
                        if (data != null)
                        {
                            Logger.LogInfo("User settings loaded from config file");
                            return data;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning($"Failed to load settings, using defaults: {ex.Message}");
                    }
                }

                Logger.LogInfo("No config file found, creating defaults");
                var defaults = new SettingsData();
                SaveData(defaults);
                return defaults;
            }
        }


        private void Save()
        {
            lock (_fileLock)
            {
                SaveData(_data);
            }
        }


        private void SaveData(SettingsData data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data, JsonOptions);
                File.WriteAllText(_configFilePath, json);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to save settings: {ex.Message}");
            }
        }
    }
}
