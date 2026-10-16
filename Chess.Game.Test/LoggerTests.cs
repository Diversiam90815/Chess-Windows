using Chess.UI.Services;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Test
{
    /// <summary>
    /// The Logger is static: every test starts and ends its own log session in a folder of its own.
    /// </summary>
    public sealed class LoggerTests : IDisposable
    {
        private readonly string _appDataDirectory = Path.Combine(Path.GetTempPath(), "Chess.Game.Test", "Logger", Guid.NewGuid().ToString("N"));

        private string LogFolder => Path.Combine(_appDataDirectory, Logger.LogFolderName);


        public LoggerTests()
        {
            Logger.SetLevel(LogLevel.Info);
        }


        public void Dispose()
        {
            Logger.Shutdown();
            Logger.SetLevel(LogLevel.Info);

            try { Directory.Delete(_appDataDirectory, recursive: true); } catch (IOException) { }
        }


        private string[] FinishAndReadLines()
        {
            string path = Logger.LogFilePath;
            Logger.Shutdown();

            return File.Exists(path) ? File.ReadAllLines(path) : [];
        }


        [Fact]
        public void Line_HasTheColumnsOfTheLog()
        {
            Logger.Initialize(_appDataDirectory, writeToDebugOutput: false);

            Logger.LogWarning("Something to look at");

            string line = Assert.Single(FinishAndReadLines());
            Assert.Matches(new Regex(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} +\d+ warning +LoggerTests +LoggerTests\.Line_HasTheColumnsOfTheLog +Something to look at$"), line);
        }


        [Fact]
        public void LinesBelowTheLevel_AreNotWritten()
        {
            Logger.Initialize(_appDataDirectory, writeToDebugOutput: false);
            Logger.SetLevel(LogLevel.Warning);

            Logger.LogInfo("dropped");
            Logger.LogDebug("dropped");
            Logger.LogError("kept");

            Assert.Contains("kept", Assert.Single(FinishAndReadLines()));
            Assert.False(Logger.IsEnabled(LogLevel.Critical), "nothing is enabled once the log is closed");
        }


        [Fact]
        public void LevelChange_IsAnnounced()
        {
            LogLevel? announced = null;
            Action<LogLevel> handler = level => announced = level;
            Logger.LevelChanged += handler;

            Logger.SetLevel(LogLevel.Debug);
            Logger.LevelChanged -= handler;

            Assert.Equal(LogLevel.Debug, announced);
            Assert.Equal(LogLevel.Debug, Logger.Level);
        }


        [Fact]
        public void LinesOfOtherComponents_KeepTheirComponentAndText()
        {
            Logger.Initialize(_appDataDirectory, writeToDebugOutput: false);

            Logger.Write(LogLevel.Error, "NetLink", "NetLink", "Socket {0} closed with {braces}");

            string line = Assert.Single(FinishAndReadLines());
            Assert.Contains(" error ", line);
            Assert.Contains("NetLink", line);
            Assert.EndsWith("Socket {0} closed with {braces}", line);
        }


        [Fact]
        public void BeforeInitialize_NothingIsWrittenAndNothingThrows()
        {
            Logger.LogError("nowhere to go");
            Logger.Write(LogLevel.Critical, null!, null!, null!);

            Assert.False(Directory.Exists(LogFolder));
        }


        [Fact]
        public void EachSession_StartsANewFile_AndKeepsTheEarlierOnes()
        {
            for (int session = 1; session <= Logger.SessionsKept + 2; ++session)
            {
                Logger.Initialize(_appDataDirectory, writeToDebugOutput: false);
                Logger.LogInfo($"session {session}");
                Logger.Shutdown();
            }

            int last = Logger.SessionsKept + 2;

            Assert.Contains($"session {last}", File.ReadAllText(Path.Combine(LogFolder, "Chess.log")));
            Assert.Contains($"session {last - 1}", File.ReadAllText(Path.Combine(LogFolder, "Chess.1.log")));
            Assert.Contains($"session {last - (Logger.SessionsKept - 1)}", File.ReadAllText(Path.Combine(LogFolder, $"Chess.{Logger.SessionsKept - 1}.log")));
            Assert.Equal(Logger.SessionsKept, Directory.GetFiles(LogFolder, "Chess*.log").Length);
        }
    }


    [Collection(EngineCollection.Name)]
    public sealed class EngineLoggingTests : IDisposable
    {
        private readonly EngineFixture _engine;
        private readonly string _appDataDirectory = Path.Combine(Path.GetTempPath(), "Chess.Game.Test", "EngineLog", Guid.NewGuid().ToString("N"));


        public EngineLoggingTests(EngineFixture engine)
        {
            _engine = engine;
            Logger.Initialize(_appDataDirectory, writeToDebugOutput: false);
        }


        public void Dispose()
        {
            Logger.Shutdown();
            Logger.SetLevel(LogLevel.Info);

            try { Directory.Delete(_appDataDirectory, recursive: true); } catch (IOException) { }
        }


        private async Task StartGameAsync()
        {
            Assert.True(await _engine.GameService.StartGameAsync(GameConfiguration.CreateLocalCoop()).WaitAsync(TimeSpan.FromSeconds(5)));
        }


        private string[] FinishAndReadLines()
        {
            string path = Logger.LogFilePath;
            Logger.Shutdown();

            return File.ReadAllLines(path);
        }


        [Fact]
        public async Task EngineLines_ReachTheAppsLog_WithTheirComponentAndThread()
        {
            await StartGameAsync();

            string[] lines = FinishAndReadLines();

            string startLine = Assert.Single(lines, line => line.Contains("Starting game"));
            Assert.Contains(" info ", startLine);
            Assert.Contains("GameManager", startLine);
            Assert.Contains("GameManager::startGame", startLine);

            // The state machine logs on its own thread
            string transition = lines.First(line => line.Contains("State transition"));
            string Thread(string line) => Regex.Match(line, @"^\S+ \S+ +(\d+) ").Groups[1].Value;
            Assert.NotEqual(Thread(startLine), Thread(transition));
        }


        [Fact]
        public async Task AppLogLevel_AppliesToTheEngine()
        {
            Logger.SetLevel(LogLevel.Warning);

            await StartGameAsync();

            Assert.DoesNotContain(FinishAndReadLines(), line => line.Contains("Starting game"));
        }
    }
}
