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
}
