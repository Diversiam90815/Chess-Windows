using Chess.UI.Audio.Modules;
using Chess.UI.Audio.Services;
using Chess.UI.Services;
using Chess.UI.Settings;
using Chess.UI.Styles;
using Chess.UI.Test.Helpers;
using Chess.UI.UI;
using Chess.UI.ViewModels;
using Moq;


namespace Chess.UI.Test
{
    public class AudioPreferencesViewModelTests
    {
        private readonly Mock<IChessAudioService> _audioService = new();


        private AudioPreferencesViewModel CreateViewModel()
        {
            var viewModel = new AudioPreferencesViewModel(new InlineDispatcherQueue(), _audioService.Object);

            // Loading the current values writes them back once, which is of no interest to the tests
            _audioService.Invocations.Clear();

            return viewModel;
        }


        private static bool IsVolume(float actual, float expected) => Math.Abs(actual - expected) < 0.001f;


        [Fact]
        public void CurrentAudioSettings_AreShownInPercent()
        {
            _audioService.Setup(service => service.GetMasterVolume()).Returns(0.7f);
            _audioService.Setup(service => service.GetSFXVolume()).Returns(0.5f);
            _audioService.Setup(service => service.GetAtmosphereVolume()).Returns(1.0f);
            _audioService.Setup(service => service.GetSFXEnabled()).Returns(true);
            _audioService.Setup(service => service.GetAtmosphereEnabled()).Returns(false);
            _audioService.Setup(service => service.GetCurrentAtmosphere()).Returns(AtmosphereScenario.Fireplace);

            AudioPreferencesViewModel viewModel = CreateViewModel();

            Assert.Equal(70, viewModel.MasterVolume);
            Assert.Equal(50, viewModel.SfxVolume);
            Assert.Equal(100, viewModel.AtmosVolume);
            Assert.True(viewModel.SfxEnabled);
            Assert.False(viewModel.AtmosEnabled);
            Assert.Equal(AtmosphereScenario.Fireplace, viewModel.SelectedAtmosphereSoundscape.Scenario);
        }


        [Fact]
        public void EveryAtmosphereScenario_IsOffered()
        {
            AudioPreferencesViewModel viewModel = CreateViewModel();

            Assert.Equal(Enum.GetValues<AtmosphereScenario>(), viewModel.AtmosphereSoundscapes.Select(item => item.Scenario));
            Assert.All(viewModel.AtmosphereSoundscapes, item => Assert.Equal(item.Scenario.ToString(), item.DisplayName));
        }


        [Fact]
        public void VolumeChanges_AreSentToTheAudioServiceAsFractions()
        {
            AudioPreferencesViewModel viewModel = CreateViewModel();

            viewModel.MasterVolume = 40;
            viewModel.SfxVolume = 90;
            viewModel.AtmosVolume = 10;

            _audioService.Verify(service => service.SetMasterVolume(It.Is<float>(volume => IsVolume(volume, 0.4f))), Times.Once);
            _audioService.Verify(service => service.SetSFXVolume(It.Is<float>(volume => IsVolume(volume, 0.9f))), Times.Once);
            _audioService.Verify(service => service.SetAtmosphereVolume(It.Is<float>(volume => IsVolume(volume, 0.1f))), Times.Once);
        }


        [Fact]
        public void UnchangedVolume_IsNotSentAgain()
        {
            AudioPreferencesViewModel viewModel = CreateViewModel();
            viewModel.MasterVolume = 40;
            _audioService.Invocations.Clear();
            List<string?> changedProperties = TestData.RecordPropertyChanges(viewModel);

            viewModel.MasterVolume = 40;

            _audioService.Verify(service => service.SetMasterVolume(It.IsAny<float>()), Times.Never);
            Assert.Empty(changedProperties);
        }


        [Fact]
        public void VolumeChange_IsReported()
        {
            AudioPreferencesViewModel viewModel = CreateViewModel();
            List<string?> changedProperties = TestData.RecordPropertyChanges(viewModel);

            viewModel.SfxVolume = 30;

            Assert.Equal(new[] { nameof(AudioPreferencesViewModel.SfxVolume) }, changedProperties);
        }


        [Fact]
        public void Switches_EnableAndDisableTheModules()
        {
            AudioPreferencesViewModel viewModel = CreateViewModel();

            viewModel.SfxEnabled = true;
            viewModel.AtmosEnabled = true;
            viewModel.AtmosEnabled = false;

            _audioService.Verify(service => service.SetSFXEnabled(true), Times.Once);
            _audioService.Verify(service => service.SetAtmosphereEnabled(true), Times.Once);
            _audioService.Verify(service => service.SetAtmosphereEnabled(false), Times.Once);
        }


        [Fact]
        public void SelectingAScenario_SwitchesTheAtmosphereAndAnnouncesTheSelection()
        {
            _audioService.Setup(service => service.GetCurrentAtmosphere()).Returns(AtmosphereScenario.Forest);
            AudioPreferencesViewModel viewModel = CreateViewModel();
            int selections = 0;
            viewModel.ItemSelected += () => selections++;
            AtmosphereScenarioItem fireplace = viewModel.AtmosphereSoundscapes.Single(item => item.Scenario == AtmosphereScenario.Fireplace);

            viewModel.SelectedAtmosphereSoundscape = fireplace;
            viewModel.SelectedAtmosphereSoundscape = fireplace;

            _audioService.Verify(service => service.SetAtmosphereAsync(AtmosphereScenario.Fireplace), Times.Once);
            Assert.Equal(1, selections);
        }
    }


    public class MultiplayerPreferencesViewModelTests
    {
        private static readonly NetworkAdapter Ethernet = new() { ID = 1, AdapterName = "Ethernet", NetworkName = "Office", Visibility = AdapterVisibility.Recommended };
        private static readonly NetworkAdapter WiFi = new() { ID = 2, AdapterName = "Wi-Fi", NetworkName = "Home", Visibility = AdapterVisibility.Recommended };
        private static readonly NetworkAdapter Vpn = new() { ID = 3, AdapterName = "VPN", NetworkName = "Tunnel", Visibility = AdapterVisibility.Visible };
        private static readonly NetworkAdapter Loopback = new() { ID = 4, AdapterName = "Loopback", NetworkName = "Local", Visibility = AdapterVisibility.Hidden };

        private readonly Mock<IMultiplayerPreferencesModel> _model = new();


        private MultiplayerPreferencesViewModel CreateViewModel(int selectedAdapterID, params NetworkAdapter[] adapters)
        {
            _model.Setup(model => model.GetNetworkAdapters()).Returns(adapters.ToList());
            _model.Setup(model => model.GetSelectedNetworkAdapterID()).Returns(selectedAdapterID);
            _model.Setup(model => model.GetLocalPlayerName()).Returns("Jens");

            return new MultiplayerPreferencesViewModel(new InlineDispatcherQueue(), _model.Object);
        }


        [Fact]
        public void Creation_LoadsAdaptersAndPlayerNameFromTheModel()
        {
            MultiplayerPreferencesViewModel viewModel = CreateViewModel(0, Ethernet, WiFi);

            _model.Verify(model => model.Init(), Times.Once);
            Assert.Equal(new[] { Ethernet, WiFi }, viewModel.NetworkAdapters);
            Assert.Equal("Jens", viewModel.LocalPlayerName);
        }


        [Fact]
        public void AdapterList_ShowsRecommendedOnesFirst_ThenTheRestUnderAHeading()
        {
            MultiplayerPreferencesViewModel viewModel = CreateViewModel(0, Ethernet, Vpn, WiFi, Loopback);

            Assert.Collection(viewModel.NetworkAdapterNodes,
                node => Assert.Equal(WiFi, Assert.IsType<NetworkAdapterComboItem>(node).NetworkAdapter),
                node => Assert.Equal(Ethernet, Assert.IsType<NetworkAdapterComboItem>(node).NetworkAdapter),
                node => Assert.IsType<ComboDivider>(node),
                node => Assert.Equal("Not recommended:", Assert.IsType<ComboHeader>(node).HeadingText),
                node => Assert.Equal(Vpn, Assert.IsType<NetworkAdapterComboItem>(node).NetworkAdapter));
        }


        [Fact]
        public void AdapterList_WithOnlyRecommendedAdapters_HasNoHeading()
        {
            MultiplayerPreferencesViewModel viewModel = CreateViewModel(0, Ethernet, WiFi);

            Assert.All(viewModel.NetworkAdapterNodes, node => Assert.IsType<NetworkAdapterComboItem>(node));
        }


        [Fact]
        public void AdapterInUse_IsPreselected()
        {
            MultiplayerPreferencesViewModel viewModel = CreateViewModel(2, Ethernet, WiFi);

            Assert.Equal(WiFi, viewModel.SelectedAdapter);
            Assert.Equal(WiFi, viewModel.SelectedNetworkAdaperNode.NetworkAdapter);
        }


        [Fact]
        public void WithoutAnAdapterInUse_NothingIsSelected()
        {
            MultiplayerPreferencesViewModel viewModel = CreateViewModel(0, Ethernet, WiFi);

            Assert.Null(viewModel.SelectedAdapter);
            Assert.Null(viewModel.SelectedNetworkAdaperNode);
        }


        [Fact]
        public void SelectingAnotherAdapter_SwitchesItInTheEngineAndAnnouncesTheSelection()
        {
            MultiplayerPreferencesViewModel viewModel = CreateViewModel(2, Ethernet, WiFi);
            int selections = 0;
            viewModel.ItemSelected += () => selections++;
            NetworkAdapterComboItem ethernetNode = viewModel.NetworkAdapterNodes.OfType<NetworkAdapterComboItem>().Single(node => node.NetworkAdapter == Ethernet);

            viewModel.SelectedNetworkAdaperNode = ethernetNode;
            viewModel.SelectedNetworkAdaperNode = ethernetNode;

            Assert.Equal(Ethernet, viewModel.SelectedAdapter);
            _model.Verify(model => model.ChangeNetworkAdapter(Ethernet.ID), Times.Once);
            Assert.Equal(1, selections);
        }


        [Fact]
        public void SelectingAPlaceholder_ChangesNothing()
        {
            MultiplayerPreferencesViewModel viewModel = CreateViewModel(2, Ethernet, WiFi);

            viewModel.SelectedNetworkAdaperNode = null;

            Assert.Equal(WiFi, viewModel.SelectedAdapter);
        }


        [Fact]
        public void ChangedPlayerName_IsPassedToTheModel()
        {
            MultiplayerPreferencesViewModel viewModel = CreateViewModel(0);
            List<string?> changedProperties = TestData.RecordPropertyChanges(viewModel);

            viewModel.LocalPlayerName = "Magnus";
            viewModel.LocalPlayerName = "Magnus";

            _model.Verify(model => model.SetLocalPlayerName("Magnus"), Times.Once);
            Assert.Equal(new[] { nameof(MultiplayerPreferencesViewModel.LocalPlayerName) }, changedProperties);
        }
    }


    public class MultiplayerPreferencesModelTests
    {
        private readonly Mock<ISettingsService> _settings = new();
        private readonly Mock<ICommunicationLayer> _engine = new();
        private readonly MultiplayerPreferencesModel _model;


        public MultiplayerPreferencesModelTests()
        {
            _settings.SetupProperty(settings => settings.PlayerName, "Jens");
            _model = new MultiplayerPreferencesModel(_settings.Object, _engine.Object);
        }


        [Fact]
        public void Init_KeepsOnlyUsableAdapters()
        {
            var usable = new NetworkAdapter { ID = 1, NetworkName = "Home", Visibility = AdapterVisibility.Recommended };
            var withoutID = new NetworkAdapter { ID = 0, NetworkName = "Ghost" };
            var withoutNetwork = new NetworkAdapter { ID = 7, NetworkName = "" };
            _engine.Setup(engine => engine.GetNetworkAdapters()).Returns(new[] { usable, withoutID, withoutNetwork });

            _model.Init();

            Assert.Equal(new[] { usable }, _model.GetNetworkAdapters());
        }


        [Fact]
        public void Init_ReplacesTheAdaptersFromBefore()
        {
            var first = new NetworkAdapter { ID = 1, NetworkName = "Home" };
            var second = new NetworkAdapter { ID = 2, NetworkName = "Office" };
            _engine.Setup(engine => engine.GetNetworkAdapters()).Returns(new[] { first });
            _model.Init();

            _engine.Setup(engine => engine.GetNetworkAdapters()).Returns(new[] { second });
            _model.Init();

            Assert.Equal(new[] { second }, _model.GetNetworkAdapters());
        }


        [Fact]
        public void BeforeInit_ThereAreNoAdapters()
        {
            Assert.Empty(_model.GetNetworkAdapters());
        }


        [Fact]
        public void SelectedAdapter_IsReadFromAndWrittenToTheEngine()
        {
            _engine.Setup(engine => engine.GetCurrentNetworkAdapterID()).Returns(5);

            Assert.Equal(5, _model.GetSelectedNetworkAdapterID());

            _model.ChangeNetworkAdapter(9);

            _engine.Verify(engine => engine.ChangeNetworkAdapter(9), Times.Once);
        }


        [Fact]
        public void PlayerName_IsSavedSentToTheEngineAndAnnounced()
        {
            string? announcedName = null;
            _model.PlayerNameChanged += name => announcedName = name;

            _model.SetLocalPlayerName("Magnus");

            Assert.Equal("Magnus", _settings.Object.PlayerName);
            Assert.Equal("Magnus", _model.GetLocalPlayerName());
            Assert.Equal("Magnus", announcedName);
            _engine.Verify(engine => engine.SetLocalPlayerName("Magnus"), Times.Once);
        }
    }


    public class StylesPreferencesViewModelTests
    {
        private readonly Mock<IStyleManager> _styleManager = new();
        private readonly Mock<ISettingsService> _settings = new();


        private async Task<StylesPreferencesViewModel> CreateViewModelAsync(string boardStyle, string pieceStyle, IStyleLoader? styleLoader = null)
        {
            _settings.SetupProperty(settings => settings.BoardStyle, boardStyle);
            _settings.SetupProperty(settings => settings.ChessPieceStyle, pieceStyle);
            _styleManager.SetupAllProperties();

            var viewModel = new StylesPreferencesViewModel(new InlineDispatcherQueue(), _styleManager.Object, _settings.Object, styleLoader ?? new MockStlyeLoader());
            await viewModel.Initialization.WaitAsync(TimeSpan.FromSeconds(5));

            return viewModel;
        }


        [Fact]
        public async Task Initialization_LoadsTheAvailableStyles()
        {
            StylesPreferencesViewModel viewModel = await CreateViewModelAsync("Wood", "Basic");

            Assert.Equal(new[] { "Wood", "Glass" }, viewModel.BoardThemes.Select(theme => theme.Name));
            Assert.Equal(new[] { "Basic", "Standard" }, viewModel.PieceThemes.Select(theme => theme.Name));
        }


        [Fact]
        public async Task Initialization_SelectsTheSavedStyles()
        {
            StylesPreferencesViewModel viewModel = await CreateViewModelAsync("glass", "Standard");

            Assert.Equal(BoardStyle.Glass, viewModel.SelectedBoardTheme.Style);
            Assert.Equal(PieceStyle.Standard, viewModel.SelectedPieceTheme.Style);
            Assert.Equal(BoardStyle.Glass, _styleManager.Object.CurrentBoardStyle);
            Assert.Equal(PieceStyle.Standard, _styleManager.Object.CurrentPieceStyle);
        }


        [Fact]
        public async Task Initialization_WithUnknownSavedStyles_SelectsNothing()
        {
            StylesPreferencesViewModel viewModel = await CreateViewModelAsync("Granite", "Lego");

            Assert.Null(viewModel.SelectedBoardTheme);
            Assert.Null(viewModel.SelectedPieceTheme);
            Assert.Equal("Granite", _settings.Object.BoardStyle);
        }


        [Fact]
        public async Task Initialization_DoesNotCountAsASelectionByTheUser()
        {
            int selections = 0;
            _settings.SetupProperty(settings => settings.BoardStyle, "Wood");
            _settings.SetupProperty(settings => settings.ChessPieceStyle, "Basic");
            var viewModel = new StylesPreferencesViewModel(new InlineDispatcherQueue(), _styleManager.Object, _settings.Object, new MockStlyeLoader());
            viewModel.ItemSelected += () => selections++;

            await viewModel.Initialization.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.Equal(0, selections);
        }


        [Fact]
        public async Task SelectingABoardStyle_SavesAppliesAndAnnouncesIt()
        {
            StylesPreferencesViewModel viewModel = await CreateViewModelAsync("Wood", "Basic");
            List<string?> changedProperties = TestData.RecordPropertyChanges(viewModel);
            int selections = 0;
            viewModel.ItemSelected += () => selections++;

            viewModel.SelectedBoardTheme = viewModel.BoardThemes.Single(theme => theme.Style == BoardStyle.Glass);

            Assert.Equal("Glass", _settings.Object.BoardStyle);
            Assert.Equal(BoardStyle.Glass, _styleManager.Object.CurrentBoardStyle);
            Assert.Equal(new[] { nameof(StylesPreferencesViewModel.SelectedBoardTheme) }, changedProperties);
            Assert.Equal(1, selections);
        }


        [Fact]
        public async Task SelectingAPieceStyle_SavesAppliesAndAnnouncesIt()
        {
            StylesPreferencesViewModel viewModel = await CreateViewModelAsync("Wood", "Basic");
            List<string?> changedProperties = TestData.RecordPropertyChanges(viewModel);
            int selections = 0;
            viewModel.ItemSelected += () => selections++;

            viewModel.SelectedPieceTheme = viewModel.PieceThemes.Single(theme => theme.Style == PieceStyle.Standard);

            Assert.Equal("Standard", _settings.Object.ChessPieceStyle);
            Assert.Equal(PieceStyle.Standard, _styleManager.Object.CurrentPieceStyle);
            Assert.Equal(new[] { nameof(StylesPreferencesViewModel.SelectedPieceTheme) }, changedProperties);
            Assert.Equal(1, selections);
        }


        [Fact]
        public async Task SelectingTheSameStyleAgain_ChangesNothing()
        {
            StylesPreferencesViewModel viewModel = await CreateViewModelAsync("Wood", "Basic");
            int selections = 0;
            viewModel.ItemSelected += () => selections++;

            viewModel.SelectedBoardTheme = viewModel.SelectedBoardTheme;

            Assert.Equal(0, selections);
        }


        [Fact]
        public async Task ClearedSelection_KeepsTheStyleThatIsInUse()
        {
            StylesPreferencesViewModel viewModel = await CreateViewModelAsync("Glass", "Standard");

            viewModel.SelectedBoardTheme = null;
            viewModel.SelectedPieceTheme = null;

            Assert.Equal("Glass", _settings.Object.BoardStyle);
            Assert.Equal(BoardStyle.Glass, _styleManager.Object.CurrentBoardStyle);
            Assert.Equal(PieceStyle.Standard, _styleManager.Object.CurrentPieceStyle);
        }


        [Fact]
        public async Task StylesThatCannotBeLoaded_LeaveTheListsEmpty()
        {
            var brokenLoader = new Mock<IStyleLoader>();
            brokenLoader.Setup(loader => loader.LoadBoardStyles()).Throws<DirectoryNotFoundException>();

            StylesPreferencesViewModel viewModel = await CreateViewModelAsync("Wood", "Basic", brokenLoader.Object);

            Assert.Empty(viewModel.BoardThemes);
            Assert.Empty(viewModel.PieceThemes);
        }
    }


    public sealed class SettingsServiceTests : IDisposable
    {
        private readonly string _appDataDirectory = Path.Combine(Path.GetTempPath(), "Chess.Game.Test", Guid.NewGuid().ToString("N"));

        private string ConfigFile => Path.Combine(_appDataDirectory, "Settings", "Config.json");


        public void Dispose()
        {
            if (Directory.Exists(_appDataDirectory))
                Directory.Delete(_appDataDirectory, recursive: true);
        }


        [Fact]
        public void FirstStart_CreatesTheConfigFileWithDefaults()
        {
            var settings = new SettingsService(_appDataDirectory);

            Assert.True(File.Exists(ConfigFile));
            Assert.Equal("Wood", settings.BoardStyle);
            Assert.Equal("Basic", settings.ChessPieceStyle);
            Assert.Equal("", settings.PlayerName);
            Assert.True(settings.AudioSFXEnabled);
            Assert.True(settings.AudioAtmosEnabled);
            Assert.Equal(1.0f, settings.AudioSFXVolume);
            Assert.Equal(1.0f, settings.AudioAtmosVolume);
            Assert.Equal(1.0f, settings.AudioMasterVolume);
            Assert.Equal("Forest", settings.AudioAtmosScenario);
            Assert.Equal(5555, settings.DiscoveryUDPPort);
        }


        [Fact]
        public void ChangedSettings_AreStillThereAfterARestart()
        {
            var settings = new SettingsService(_appDataDirectory)
            {
                BoardStyle = "Glass",
                ChessPieceStyle = "Standard",
                PlayerName = "Jens",
                AudioSFXEnabled = false,
                AudioAtmosEnabled = false,
                AudioSFXVolume = 0.4f,
                AudioAtmosVolume = 0.2f,
                AudioMasterVolume = 0.7f,
                AudioAtmosScenario = "Fireplace",
                DiscoveryUDPPort = 6000,
            };

            var restarted = new SettingsService(_appDataDirectory);

            Assert.Equal("Glass", restarted.BoardStyle);
            Assert.Equal("Standard", restarted.ChessPieceStyle);
            Assert.Equal("Jens", restarted.PlayerName);
            Assert.False(restarted.AudioSFXEnabled);
            Assert.False(restarted.AudioAtmosEnabled);
            Assert.Equal(0.4f, restarted.AudioSFXVolume);
            Assert.Equal(0.2f, restarted.AudioAtmosVolume);
            Assert.Equal(0.7f, restarted.AudioMasterVolume);
            Assert.Equal("Fireplace", restarted.AudioAtmosScenario);
            Assert.Equal(6000, restarted.DiscoveryUDPPort);
            Assert.Equal(settings.BoardStyle, restarted.BoardStyle);
        }


        [Fact]
        public void Volumes_AreRoundedToOneDecimal()
        {
            var settings = new SettingsService(_appDataDirectory)
            {
                AudioSFXVolume = 0.26f,
                AudioAtmosVolume = 0.74f,
                AudioMasterVolume = 0.449f,
            };

            Assert.Equal(0.3f, settings.AudioSFXVolume);
            Assert.Equal(0.7f, settings.AudioAtmosVolume);
            Assert.Equal(0.4f, settings.AudioMasterVolume);
        }


        [Fact]
        public void ConfigFile_UsesTheDocumentedKeys()
        {
            _ = new SettingsService(_appDataDirectory) { PlayerName = "Jens" };

            string json = File.ReadAllText(ConfigFile);

            Assert.Contains("\"PlayerName\": \"Jens\"", json);
            Assert.Contains("\"PieceStyle\"", json);
            Assert.Contains("\"Audio_Master_Volume\"", json);
            Assert.Contains("\"Audio_Atmos_Scenario\"", json);
            Assert.Contains("\"Discovery_UDP_Port\"", json);
        }


        [Fact]
        public void BrokenConfigFile_IsReplacedByDefaults()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigFile)!);
            File.WriteAllText(ConfigFile, "{ this is not json");

            var settings = new SettingsService(_appDataDirectory);

            Assert.Equal("Wood", settings.BoardStyle);
            Assert.Contains("\"BoardStyle\": \"Wood\"", File.ReadAllText(ConfigFile));
        }


        [Fact]
        public void ConfigFileWithMissingValues_FillsThemWithDefaults()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigFile)!);
            File.WriteAllText(ConfigFile, "{ \"BoardStyle\": \"Marble\" }");

            var settings = new SettingsService(_appDataDirectory);

            Assert.Equal("Marble", settings.BoardStyle);
            Assert.Equal("Basic", settings.ChessPieceStyle);
            Assert.Equal(5555, settings.DiscoveryUDPPort);
        }
    }
}
