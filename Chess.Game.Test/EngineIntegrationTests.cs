using Chess.UI.Board;
using Chess.UI.Models;
using Chess.UI.Services;
using Chess.UI.Settings;
using Chess.UI.Test.Helpers;
using Chess.UI.ViewModels;
using Moq;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Test
{
    /// <summary>
    /// Starts the native engine once for all tests that talk to it. The engine is a process-wide
    /// instance, so these tests share one collection and never run in parallel.
    /// </summary>
    public sealed class EngineFixture : IDisposable
    {
        private int _executedMoves;
        private int _undoneMoves;
        private int _selections;
        private int _resets;
        private int _phase = (int)GamePhase.Initializing;

        public CommunicationLayer Communication { get; }
        public ChessGameService GameService { get; }
        public ConcurrentQueue<PlayerCapturedPiece> Captures { get; } = new();
        public ConcurrentQueue<EndGameStateEvent> EndGameEvents { get; } = new();

        public int ExecutedMoves => Volatile.Read(ref _executedMoves);
        public int UndoneMoves => Volatile.Read(ref _undoneMoves);
        public int Selections => Volatile.Read(ref _selections);
        public int Resets => Volatile.Read(ref _resets);
        public GamePhase Phase => (GamePhase)Volatile.Read(ref _phase);


        public EngineFixture()
        {
            Communication = new CommunicationLayer(new InlineDispatcherQueue());
            Communication.MoveExecuted += (_, _) => Interlocked.Increment(ref _executedMoves);
            Communication.MoveUndone += () => Interlocked.Increment(ref _undoneMoves);
            Communication.LegalMovesCalculated += _ => Interlocked.Increment(ref _selections);
            Communication.GameStateChanged += phase => Volatile.Write(ref _phase, (int)phase);
            Communication.PlayerCapturedPieceEvent += Captures.Enqueue;
            Communication.EndGameStateEvent += EndGameEvents.Enqueue;

            // One game service for the engine, like in the app
            GameService = new ChessGameService(Communication);
            GameService.GameReset += () => Interlocked.Increment(ref _resets);

            // Like the app: log callback, engine, event delegate
            Communication.Init();
        }


        public void ClearEvents()
        {
            Captures.Clear();
            EndGameEvents.Clear();
        }


        public void Dispose()
        {
            Communication.Deinit();
        }
    }


    [CollectionDefinition(Name)]
    public class EngineCollection : ICollectionFixture<EngineFixture>
    {
        public const string Name = "Engine";
    }


    [Collection(EngineCollection.Name)]
    public class EngineIntegrationTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        private readonly EngineFixture _engine;
        private readonly ChessGameService _gameService;


        public EngineIntegrationTests(EngineFixture engine)
        {
            _engine = engine;
            _engine.ClearEvents();
            _gameService = engine.GameService;
        }


        // Fails a test instead of hanging when an engine event does not arrive
        private async Task WaitUntilAsync(Func<bool> condition, string description)
        {
            var stopwatch = Stopwatch.StartNew();

            while (!condition())
            {
                if (stopwatch.Elapsed > Timeout)
                    throw new TimeoutException($"Timed out waiting for {description} (engine phase: {_engine.Phase})");

                await Task.Delay(10);
            }
        }


        private Task<bool> StartGameAsync()
        {
            return _gameService.StartGameAsync(GameConfiguration.CreateLocalCoop()).WaitAsync(Timeout);
        }


        // Plays a move the way the board does: select the piece, then the target square
        private async Task PlayMoveAsync(Square from, Square to)
        {
            int selectionsBefore = _engine.Selections;
            int movesBefore = _engine.ExecutedMoves;

            EngineAPI.OnSquareSelected((int)from);
            await WaitUntilAsync(() => _engine.Selections > selectionsBefore, $"{from} to be selected");

            EngineAPI.OnSquareSelected((int)to);
            await WaitUntilAsync(() => _engine.ExecutedMoves > movesBefore, $"{from}-{to} to be played");
        }


        private static PieceType[] ReadBoard()
        {
            int[] rawBoard = new int[64];
            Assert.True(EngineAPI.GetBoardState(rawBoard));

            return Array.ConvertAll(rawBoard, piece => (PieceType)piece);
        }


        // Plays a move the way a move from the network arrives: complete, in one call
        private async Task SubmitMoveAsync(Square from, Square to)
        {
            int movesBefore = _engine.ExecutedMoves;

            _engine.Communication.SubmitMove(from, to, PieceType.None);
            await WaitUntilAsync(() => _engine.ExecutedMoves > movesBefore, $"{from}-{to} to be played");
        }


        [Fact]
        public async Task MultiplayerGame_TakesMovesOfBothSidesThroughSubmitMove()
        {
            Assert.True(await _gameService.StartGameAsync(GameConfiguration.CreateMultiplayer(Side.Black)).WaitAsync(Timeout));

            await SubmitMoveAsync(Square.e2, Square.e4);
            await SubmitMoveAsync(Square.e7, Square.e5);

            PieceType[] board = ReadBoard();

            Assert.Equal(GameModeSelection.MultiPlayer, _gameService.CurrentConfiguration?.Mode);
            Assert.Equal(PieceType.WPawn, board[(int)Square.e4]);
            Assert.Equal(PieceType.BPawn, board[(int)Square.e5]);
            Assert.Equal(GamePhase.PlayerTurn, _engine.Phase);
        }


        [Fact]
        public async Task SubmittedIllegalMove_IsNotPlayed()
        {
            Assert.True(await StartGameAsync());
            int movesBefore = _engine.ExecutedMoves;

            // Black is not to move, and a pawn cannot jump three squares
            _engine.Communication.SubmitMove(Square.e7, Square.e5, PieceType.None);
            _engine.Communication.SubmitMove(Square.e2, Square.e5, PieceType.None);
            await SubmitMoveAsync(Square.d2, Square.d4);

            PieceType[] board = ReadBoard();

            Assert.Equal(movesBefore + 1, _engine.ExecutedMoves);
            Assert.Equal(PieceType.BPawn, board[(int)Square.e7]);
            Assert.Equal(PieceType.WPawn, board[(int)Square.e2]);
            Assert.Equal(PieceType.WPawn, board[(int)Square.d4]);
        }


        [Fact]
        public async Task StartGame_SetsUpTheBoard()
        {
            Assert.True(await StartGameAsync());

            PieceType[] board = ReadBoard();

            Assert.Equal(GamePhase.PlayerTurn, _engine.Phase);
            Assert.True(_gameService.IsGameActive);
            Assert.Equal(GameModeSelection.LocalCoop, _gameService.CurrentConfiguration?.Mode);
            Assert.Equal(PieceType.WKing, board[(int)Square.e1]);
            Assert.Equal(PieceType.BKing, board[(int)Square.e8]);
            Assert.Equal(PieceType.None, board[(int)Square.e4]);
        }


        [Fact]
        public async Task StartGame_WhileAGameIsRunning_ResetsItFirst()
        {
            Assert.True(await StartGameAsync());
            await PlayMoveAsync(Square.e2, Square.e4);

            int resetsBefore = _engine.Resets;

            Assert.True(await StartGameAsync());

            PieceType[] board = ReadBoard();

            Assert.Equal(resetsBefore + 1, _engine.Resets);
            Assert.True(_gameService.IsGameActive);
            Assert.Equal(PieceType.WPawn, board[(int)Square.e2]);
            Assert.Equal(PieceType.None, board[(int)Square.e4]);
        }


        [Fact]
        public async Task ResetGame_IsReportedOnceTheEngineConfirmsIt()
        {
            Assert.True(await StartGameAsync());

            Assert.True(await _gameService.ResetGameAsync().WaitAsync(Timeout));

            Assert.Equal(GamePhase.Initializing, _engine.Phase);
            Assert.False(_gameService.IsGameActive);
        }


        [Fact]
        public async Task StartGame_AfterAFinishedGameWasLeft_StartsAFreshGame()
        {
            Assert.True(await StartGameAsync());

            // Fool's mate
            await PlayMoveAsync(Square.f2, Square.f3);
            await PlayMoveAsync(Square.e7, Square.e5);
            await PlayMoveAsync(Square.g2, Square.g4);
            await PlayMoveAsync(Square.d8, Square.h4);

            await WaitUntilAsync(() => _engine.Phase == GamePhase.GameEnded && !_engine.EndGameEvents.IsEmpty, "the game to end");

            Assert.True(_engine.EndGameEvents.TryPeek(out EndGameStateEvent endGame));
            Assert.Equal(EndGameState.Checkmate, endGame.State);
            Assert.Equal(Side.Black, endGame.winner);

            // Leaving through the end game dialog only ends the game
            await _gameService.EndGameAsync();
            await WaitUntilAsync(() => _engine.Phase == GamePhase.Initializing, "the engine to reset");

            Assert.True(await StartGameAsync());

            PieceType[] board = ReadBoard();

            Assert.Equal(GamePhase.PlayerTurn, _engine.Phase);
            Assert.Equal(PieceType.WPawn, board[(int)Square.f2]);
            Assert.Equal(PieceType.BQueen, board[(int)Square.d8]);
            Assert.Equal(PieceType.None, board[(int)Square.h4]);
        }


        [Fact]
        public async Task Capture_IsCountedForTheCapturingSide_AndTakenBackOnUndo()
        {
            var capturedPieces = new CapturedPiecesViewModel(
                new InlineDispatcherQueue(),
                new CapturedPiecesModel(_engine.Communication),
                Mock.Of<IImageService>());

            Assert.True(await StartGameAsync());

            await PlayMoveAsync(Square.e2, Square.e4);
            await PlayMoveAsync(Square.d7, Square.d5);
            await PlayMoveAsync(Square.e4, Square.d5);

            await WaitUntilAsync(() => !_engine.Captures.IsEmpty, "the capture to be reported");

            Assert.True(_engine.Captures.TryPeek(out PlayerCapturedPiece capture));
            Assert.Equal(Side.White, capture.playerColor);
            Assert.Equal(PieceType.BPawn, capture.pieceType);
            Assert.True(capture.captured);

            Assert.Equal(1, capturedPieces.WhiteCapturedPawn);
            Assert.Equal(0, capturedPieces.BlackCapturedPawn);

            int undoneBefore = _engine.UndoneMoves;
            EngineAPI.UndoMove();
            await WaitUntilAsync(() => _engine.UndoneMoves > undoneBefore && _engine.Captures.Count == 2, "the capture to be taken back");

            Assert.False(_engine.Captures.Last().captured);
            Assert.Equal(0, capturedPieces.WhiteCapturedPawn);
            Assert.Equal(PieceType.BPawn, ReadBoard()[(int)Square.d5]);
        }


        [Fact]
        public void PlayerCapturedPiece_HasTheEngineLayout()
        {
            // int8 side, padding, int piece, bool captured, padding
            Assert.Equal(12, Marshal.SizeOf<PlayerCapturedPiece>());
            Assert.Equal(4, (int)Marshal.OffsetOf<PlayerCapturedPiece>(nameof(PlayerCapturedPiece.pieceType)));
        }
    }
}
