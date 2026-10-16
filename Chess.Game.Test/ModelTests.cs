using Chess.UI.Board;
using Chess.UI.Models;
using Chess.UI.Services;
using Chess.UI.Settings;
using Chess.UI.Test.Helpers;
using Moq;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Test
{
    public class MoveModelTests
    {
        private readonly Mock<ICommunicationLayer> _communication = new();
        private readonly MoveModel _model;


        public MoveModelTests()
        {
            _model = new MoveModel(_communication.Object);
        }


        [Fact]
        public void LegalMovesFromEngine_ReplaceThePreviousOnes()
        {
            int notifications = 0;
            _model.LegalMovesCalculated += () => notifications++;

            var pawnMoves = new List<Move> { new(Square.e2, Square.e3), new(Square.e2, Square.e4, MoveFlag.DoublePawnPush) };
            var knightMoves = new List<Move> { new(Square.g1, Square.f3) };

            _communication.Raise(layer => layer.LegalMovesCalculated += null, pawnMoves);
            _communication.Raise(layer => layer.LegalMovesCalculated += null, knightMoves);

            Assert.Equal(knightMoves, _model.LegalMoves);
            Assert.Equal(2, notifications);
        }


        [Fact]
        public void LegalMovesFromEngine_AnnounceTheSelectionBeforeTheMoves()
        {
            var raisedEvents = new List<string>();
            _model.ChesspieceSelected += () => raisedEvents.Add("Selected");
            _model.LegalMovesCalculated += () => raisedEvents.Add("Moves");

            _communication.Raise(layer => layer.LegalMovesCalculated += null, new List<Move> { new(Square.g1, Square.f3) });

            Assert.Equal(new[] { "Selected", "Moves" }, raisedEvents);
        }


        [Theory]
        [InlineData(GamePhase.Initializing, 1)]
        [InlineData(GamePhase.PlayerTurn, 0)]
        [InlineData(GamePhase.OpponentTurn, 0)]
        [InlineData(GamePhase.PromotionDialog, 0)]
        [InlineData(GamePhase.GameEnded, 0)]
        public void OnlyTheInitialPhase_IsReportedAsInitialized(GamePhase phase, int expectedNotifications)
        {
            int notifications = 0;
            _model.GameStateInitSucceeded += () => notifications++;

            _communication.Raise(layer => layer.GameStateChanged += null, phase);

            Assert.Equal(expectedNotifications, notifications);
        }


        [Fact]
        public void EndOfGame_IsForwardedWithStateAndWinner()
        {
            (EndGameState State, Side Winner)? gameOver = null;
            _model.GameOverEvent += (state, winner) => gameOver = (state, winner);

            _communication.Raise(layer => layer.EndGameStateEvent += null, new EndGameStateEvent { State = EndGameState.Checkmate, winner = Side.Black });

            Assert.Equal((EndGameState.Checkmate, Side.Black), gameOver);
        }


        [Fact]
        public void EngineEvents_AreForwarded()
        {
            Side? player = null;
            PieceType[]? board = null;
            int promotions = 0;
            _model.PlayerChanged += side => player = side;
            _model.NewBoardFromBackendEvent += newBoard => board = newBoard;
            _model.PawnPromotionEvent += () => promotions++;

            var engineBoard = new PieceType[64];

            _communication.Raise(layer => layer.PlayerChanged += null, Side.Black);
            _communication.Raise(layer => layer.BoardStateChanged += null, engineBoard);
            _communication.Raise(layer => layer.PawnPromotionRequired += null);

            Assert.Equal(Side.Black, player);
            Assert.Same(engineBoard, board);
            Assert.Equal(1, promotions);
        }
    }


    public class BoardModelTests
    {
        private readonly BoardModel _model = new();


        private static PieceType[] EmptyBoard()
        {
            var board = new PieceType[64];
            Array.Fill(board, PieceType.None);

            return board;
        }


        [Fact]
        public void NewBoard_HasAnEmptySquareForEveryPosition()
        {
            BoardSquare[] squares = _model.GetAllSquares();

            Assert.Equal(64, squares.Length);
            Assert.All(squares, square => Assert.Equal(PieceType.None, square.Piece));
            Assert.Equal(Square.a8, squares[0].Position);
            Assert.Equal(Square.h1, squares[63].Position);
        }


        [Fact]
        public void UpdateBoardState_ReportsOnlyTheChangedSquares()
        {
            PieceType[] board = EmptyBoard();
            _model.UpdateBoardState(board);

            board[(int)Square.e4] = PieceType.WPawn;
            Dictionary<Square, PieceType> changedSquares = _model.UpdateBoardState(board);

            Assert.Equal(new Dictionary<Square, PieceType> { [Square.e4] = PieceType.WPawn }, changedSquares);
            Assert.Equal(PieceType.WPawn, _model.GetSquare(Square.e4).Piece);
            Assert.Empty(_model.UpdateBoardState(board));
        }


        [Fact]
        public void UpdateBoardState_ReportsBothSquaresOfAMove()
        {
            PieceType[] board = EmptyBoard();
            board[(int)Square.g1] = PieceType.WKnight;
            _model.UpdateBoardState(board);

            board[(int)Square.g1] = PieceType.None;
            board[(int)Square.f3] = PieceType.WKnight;
            Dictionary<Square, PieceType> changedSquares = _model.UpdateBoardState(board);

            Assert.Equal(2, changedSquares.Count);
            Assert.Equal(PieceType.None, changedSquares[Square.g1]);
            Assert.Equal(PieceType.WKnight, changedSquares[Square.f3]);
        }


        [Fact]
        public void GetSquare_OutsideTheBoard_ReturnsNothing()
        {
            Assert.Null(_model.GetSquare(Square.None));
        }


        [Fact]
        public void GetSquare_ReturnsTheSameSquareEveryTime()
        {
            Assert.Same(_model.GetSquare(Square.d5), _model.GetSquare(Square.d5));
            Assert.Equal(Square.d5, _model.GetSquare(Square.d5).Position);
        }
    }


    public class BoardSquareTests
    {
        [Fact]
        public void Square_KnowsItsDisplayPosition()
        {
            var square = new BoardSquare(Square.c6, PieceType.BBishop);

            Assert.Equal(2, square.DisplayX);
            Assert.Equal(2, square.DisplayY);
            Assert.Equal(PieceType.BBishop, square.Piece);
        }


        [Fact]
        public void DefaultSquare_IsOffTheBoardAndEmpty()
        {
            var square = new BoardSquare();

            Assert.Equal(Square.None, square.Position);
            Assert.Equal(PieceType.None, square.Piece);
            Assert.False(square.IsHighlighted);
            Assert.Null(square.PieceImage);
        }
    }


    public class MoveHistoryModelTests
    {
        private readonly Mock<ICommunicationLayer> _communication = new();


        [Fact]
        public void TracksExecutedAndUndoneMoves()
        {
            var model = new MoveHistoryModel(_communication.Object);
            model.ClearHistory();

            int notifications = 0;
            model.MoveHistoryUpdated += () => notifications++;

            _communication.Raise(layer => layer.MoveExecuted += null, new Move(Square.e2, Square.e4), "e4");
            _communication.Raise(layer => layer.MoveExecuted += null, new Move(Square.e7, Square.e5), "e5");
            _communication.Raise(layer => layer.MoveUndone += null);

            Assert.Equal(new[] { "e4" }, model.MoveHistory);
            Assert.Equal(3, notifications);

            // Nothing is left to undo after the last move is taken back
            _communication.Raise(layer => layer.MoveUndone += null);
            _communication.Raise(layer => layer.MoveUndone += null);

            Assert.Empty(model.MoveHistory);
            Assert.Equal(4, notifications);
        }
    }


    public class CapturedPiecesModelTests
    {
        private readonly Mock<ICommunicationLayer> _communication = new();
        private readonly CapturedPiecesModel _model;


        public CapturedPiecesModelTests()
        {
            _model = new CapturedPiecesModel(_communication.Object);
        }


        private void EngineReports(PlayerCapturedPiece capture)
        {
            _communication.Raise(layer => layer.PlayerCapturedPieceEvent += null, capture);
        }


        [Fact]
        public void Captures_AreCountedByTheCapturedPiece()
        {
            EngineReports(TestData.Capture(Side.White, PieceType.BPawn));
            EngineReports(TestData.Capture(Side.White, PieceType.BPawn));
            EngineReports(TestData.Capture(Side.Black, PieceType.WQueen));

            Assert.Equal(2, _model.CapturedPieces[PieceType.BPawn]);
            Assert.Equal(1, _model.CapturedPieces[PieceType.WQueen]);
            Assert.Equal(0, _model.CapturedPieces[PieceType.WPawn]);
        }


        [Fact]
        public void UndoneCapture_IsTakenOffTheCount()
        {
            EngineReports(TestData.Capture(Side.White, PieceType.BRook));
            EngineReports(TestData.Capture(Side.White, PieceType.BRook, captured: false));

            Assert.Equal(0, _model.CapturedPieces[PieceType.BRook]);
        }


        [Fact]
        public void Count_NeverDropsBelowZero()
        {
            EngineReports(TestData.Capture(Side.White, PieceType.BKnight, captured: false));

            Assert.Equal(0, _model.CapturedPieces[PieceType.BKnight]);
        }


        [Fact]
        public void Captures_AreAnnounced_ExceptForPiecesThatCannotBeCaptured()
        {
            var announced = new List<PieceType>();
            _model.PieceCaptured += capture => announced.Add(capture.pieceType);

            EngineReports(TestData.Capture(Side.White, PieceType.BBishop));
            EngineReports(TestData.Capture(Side.White, PieceType.BKing));

            Assert.Equal(new[] { PieceType.BBishop }, announced);
        }


        [Fact]
        public void Clear_ResetsEveryCount()
        {
            EngineReports(TestData.Capture(Side.White, PieceType.BPawn));
            EngineReports(TestData.Capture(Side.Black, PieceType.WRook));

            _model.Clear();

            Assert.All(_model.CapturedPieces.Values, count => Assert.Equal(0, count));
            Assert.Equal(10, _model.CapturedPieces.Count);
        }
    }


    public class MultiplayerModelTests
    {
        private readonly Mock<ICommunicationLayer> _communication = new();
        private readonly MultiplayerModel _model;


        public MultiplayerModelTests()
        {
            _model = new MultiplayerModel(_communication.Object);
        }


        [Fact]
        public void StateChange_IsForwardedWithTheRemoteName()
        {
            (MultiplayerState State, string Name)? stateChange = null;
            _model.OnMultiplayerStateChanged += (state, name) => stateChange = (state, name);

            _communication.Raise(layer => layer.MultiplayerStateChanged += null,
                new MultiplayerStateEvent { State = MultiplayerState.ConnectionRequested, remoteName = "Anna" });

            Assert.Equal((MultiplayerState.ConnectionRequested, "Anna"), stateChange);
        }


        [Fact]
        public void ErrorState_IsReportedAsAnErrorOnly()
        {
            string? error = null;
            int stateChanges = 0;
            _model.OnConnectionErrorOccured += message => error = message;
            _model.OnMultiplayerStateChanged += (_, _) => stateChanges++;

            _communication.Raise(layer => layer.MultiplayerStateChanged += null,
                new MultiplayerStateEvent { State = MultiplayerState.Error, errorMessage = "Connection lost" });

            Assert.Equal("Connection lost", error);
            Assert.Equal(0, stateChanges);
        }


        [Fact]
        public void EngineEvents_AreForwarded()
        {
            string? opponent = null;
            Side? currentPlayer = null;
            Side? chosenByRemote = null;
            _model.OnOpponentDiscovered += name => opponent = name;
            _model.OnPlayerChanged += side => currentPlayer = side;
            _model.OnMultiplayerPlayerSetFromRemote += side => chosenByRemote = side;

            _communication.Raise(layer => layer.OpponentDiscoveredEvent += null, new MultiplayerStateEvent { remoteName = "Ben" });
            _communication.Raise(layer => layer.PlayerChanged += null, Side.Black);
            _communication.Raise(layer => layer.MultiPlayerChosenByRemote += null, Side.White);

            Assert.Equal("Ben", opponent);
            Assert.Equal(Side.Black, currentPlayer);
            Assert.Equal(Side.White, chosenByRemote);
        }
    }


    public class NetworkAdapterTests
    {
        [Fact]
        public void Adapter_NeedsAnIdAndANetworkName()
        {
            Assert.True(new NetworkAdapter { ID = 3, NetworkName = "Home" }.IsValid());
            Assert.False(new NetworkAdapter { ID = 0, NetworkName = "Home" }.IsValid());
            Assert.False(new NetworkAdapter { ID = 3, NetworkName = " " }.IsValid());
            Assert.False(new NetworkAdapter { ID = 3 }.IsValid());
        }


        [Theory]
        [InlineData(AdapterVisibility.Hidden, false, false)]
        [InlineData(AdapterVisibility.Visible, true, false)]
        [InlineData(AdapterVisibility.Recommended, true, true)]
        public void Adapter_VisibilityDecidesHowItIsListed(AdapterVisibility visibility, bool isVisible, bool isRecommended)
        {
            var adapter = new NetworkAdapter { ID = 1, NetworkName = "Home", Visibility = visibility };

            Assert.Equal(isVisible, adapter.IsVisible());
            Assert.Equal(isRecommended, adapter.IsRecommended());
        }
    }
}
