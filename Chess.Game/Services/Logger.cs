using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace Chess.UI.Services
{
    /// <summary>
    /// Severity of a log line. The values match the engine's and NetLink's, and Serilog's order.
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
    }


    /// <summary>
    /// The app's log: Logs/Chess.log in the app data folder, a new file per session. The engine and NetLink
    /// write no files themselves, their lines come in through <see cref="Write"/>.
    /// Before <see cref="Initialize"/> and after <see cref="Shutdown"/> nothing is written.
    /// </summary>
    public static class Logger
    {
        public const string LogFolderName = "Logs";
        public const string LogFileName = "Chess";
        public const int SessionsKept = 10;
        public const long MaxFileSize = 10L * 1024 * 1024;

        // The layout the log has always had: time, thread, level, component, function, message
        private const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {ThreadId,10} {LevelName,-8} {Component,-20} {Function,-45} {Text:l}{NewLine}";

        private static readonly MessageTemplate TextTemplate = new MessageTemplateParser().Parse("{Text}");
        private static readonly string[] LevelNames = ["trace", "debug", "info", "warning", "error", "critical"];

        private static readonly LoggingLevelSwitch _levelSwitch = new(LogEventLevel.Information);
        private static readonly object _lifecycleLock = new();
        private static volatile Serilog.Core.Logger _logger;


        /// <summary>
        /// Raised when the level changes, so the native libraries can be told.
        /// </summary>
        public static event Action<LogLevel> LevelChanged;

        public static LogLevel Level => (LogLevel)_levelSwitch.MinimumLevel;

        public static string LogFilePath { get; private set; }


        #region Lifecycle

        /// <summary>
        /// Starts a new log session in &lt;appDataDirectory&gt;/Logs. The files of earlier sessions are kept as Chess.1.log, Chess.2.log, ...
        /// </summary>
        public static void Initialize(string appDataDirectory, bool writeToDebugOutput = true)
        {
            lock (_lifecycleLock)
            {
                if (_logger != null)
                    return;

                string folder = Path.Combine(appDataDirectory, LogFolderName);
                Directory.CreateDirectory(folder);

                RotateSessionFiles(folder);
                LogFilePath = Path.Combine(folder, LogFileName + ".log");

                var configuration = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(_levelSwitch)
                    // The engine logs from its own threads: they only queue the line
                    .WriteTo.Async(sink => sink.File(LogFilePath, outputTemplate: OutputTemplate, fileSizeLimitBytes: MaxFileSize, rollOnFileSizeLimit: true, retainedFileCountLimit: SessionsKept));

                if (writeToDebugOutput)
                    configuration = configuration.WriteTo.Debug(outputTemplate: OutputTemplate);

                _logger = configuration.CreateLogger();
            }
        }


        /// <summary>
        /// Writes what is still queued and closes the file.
        /// </summary>
        public static void Shutdown()
        {
            lock (_lifecycleLock)
            {
                var logger = _logger;
                _logger = null;
                logger?.Dispose();
            }
        }


        public static void SetLevel(LogLevel level)
        {
            _levelSwitch.MinimumLevel = (LogEventLevel)level;
            LevelChanged?.Invoke(level);
        }


        public static bool IsEnabled(LogLevel level)
        {
            return _logger?.IsEnabled((LogEventLevel)level) ?? false;
        }

        #endregion


        #region App code

        public static void LogInfo(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Write(LogLevel.Info, GetClassName(filePath), GetMethodName(memberName, filePath), message);
        }


        public static void LogError(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Write(LogLevel.Error, GetClassName(filePath), GetMethodName(memberName, filePath), message);
        }


        public static void LogWarning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Write(LogLevel.Warning, GetClassName(filePath), GetMethodName(memberName, filePath), message);
        }


        public static void LogDebug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Write(LogLevel.Debug, GetClassName(filePath), GetMethodName(memberName, filePath), message);
        }

        #endregion


        /// <summary>
        /// Writes one line. Used by the app code above and for the lines of the native libraries. Never throws.
        /// </summary>
        public static void Write(LogLevel level, string component, string function, string message)
        {
            var logger = _logger;

            if (logger == null || !logger.IsEnabled((LogEventLevel)level))
                return;

            try
            {
                LogEventProperty[] properties =
                [
                    new("Text", new ScalarValue(message ?? string.Empty)),
                    // The id of the operating system, so app and engine threads can be told apart and matched
                    new("ThreadId", new ScalarValue(GetCurrentThreadId())),
                    new("LevelName", new ScalarValue(LevelNames[Math.Clamp((int)level, 0, LevelNames.Length - 1)])),
                    new("Component", new ScalarValue(component ?? string.Empty)),
                    new("Function", new ScalarValue(function ?? string.Empty)),
                ];

                logger.Write(new LogEvent(DateTimeOffset.Now, (LogEventLevel)level, null, TextTemplate, properties));
            }
            catch
            {
                // Logging must never take the app down, and never throw back into native code
            }
        }


        // Chess.log becomes Chess.1.log, Chess.1.log becomes Chess.2.log, ... The oldest one is deleted.
        private static void RotateSessionFiles(string folder)
        {
            try
            {
                string SessionFile(int session) => Path.Combine(folder, session == 0 ? $"{LogFileName}.log" : $"{LogFileName}.{session}.log");

                File.Delete(SessionFile(SessionsKept - 1));

                for (int session = SessionsKept - 2; session >= 0; --session)
                {
                    if (File.Exists(SessionFile(session)))
                        File.Move(SessionFile(session), SessionFile(session + 1));
                }
            }
            catch (IOException)
            {
                // Another instance has the file open: this session appends to it
            }
            catch (UnauthorizedAccessException)
            {
            }
        }


        private static string GetClassName(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }


        private static string GetMethodName(string memberName, string filePath)
        {
            string className = GetClassName(filePath);
            string methodName = memberName == ".ctor" ? className : memberName;     // .ctor is the constructor in C#

            return $"{className}.{methodName}";
        }


        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();
    }
}
