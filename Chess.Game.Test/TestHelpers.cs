using Chess.UI.Styles;
using Chess.UI.Wrappers;
using Microsoft.UI.Dispatching;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static Chess.UI.Services.EngineAPI;

// The engine is one instance per process, and some view models call into it directly (a click on a square).
// Those tests must not run while the engine integration tests play a game, and the Logger is static as well,
// so test classes run one after another.
[assembly: CollectionBehavior(DisableTestParallelization = true)]


namespace Chess.UI.Test.Helpers
{
    public class MockStlyeLoader : IStyleLoader
    {
        public List<BoardStyleInfo> LoadBoardStyles()
        {
            return new List<BoardStyleInfo>
            {
                new BoardStyleInfo { Name = "Wood", Style = BoardStyle.Wood },
                new BoardStyleInfo { Name = "Glass", Style = BoardStyle.Glass }
            };
        }

        public List<PieceStyleInfo> LoadPieceStyles()
        {
            return new List<PieceStyleInfo>
            {
                new PieceStyleInfo { Name = "Basic", Style = PieceStyle.Basic },
                new PieceStyleInfo { Name = "Standard", Style = PieceStyle.Standard }
            };
        }
    }

    // This is a mock for the static Settings class
    public static class MockSettings
    {
        public static string CurrentBoardStyle { get; set; } = "Wood";
        public static string CurrentPieceStyle { get; set; } = "Basic";
    }


    // Runs everything inline, there is no UI thread in the tests
    public class InlineDispatcherQueue : IDispatcherQueueWrapper
    {
        public bool TryEnqueue(DispatcherQueueHandler callback)
        {
            callback();
            return true;
        }

        public bool TryDequeue(DispatcherQueuePriority priority, DispatcherQueueHandler callback)
        {
            callback();
            return true;
        }

        public Task EnqueueAsync(Action callback)
        {
            callback();
            return Task.CompletedTask;
        }
    }


    public static class TestData
    {
        // PlayerCapturedPiece mirrors a native struct and can only be filled the way the engine does it
        public static PlayerCapturedPiece Capture(Side capturingSide, PieceType capturedPiece, bool captured = true)
        {
            nint buffer = Marshal.AllocHGlobal(Marshal.SizeOf<PlayerCapturedPiece>());

            try
            {
                Marshal.WriteByte(buffer, 0, unchecked((byte)(sbyte)capturingSide));
                Marshal.WriteInt32(buffer, 4, (int)capturedPiece);
                Marshal.WriteByte(buffer, 8, captured ? (byte)1 : (byte)0);

                return Marshal.PtrToStructure<PlayerCapturedPiece>(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }


        // Records the names of the properties an object reports as changed
        public static List<string?> RecordPropertyChanges(INotifyPropertyChanged source)
        {
            var changedProperties = new List<string?>();
            source.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

            return changedProperties;
        }
    }
}
