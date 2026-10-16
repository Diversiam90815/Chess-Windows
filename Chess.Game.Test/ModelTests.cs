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
        private static readonly NetworkPeer Anna = new(11, "Anna");
        private static readonly NetworkPeer Ben = new(22, "Ben");

        private readonly Mock<ICommunicationLayer> _communication = new();
        private readonly Mock<INetworkService> _network = new();
        private readonly Mock<ISettingsService> _settings = new();
        private readonly Mock<IChessGameService> _gameService = new();
        private readonly MultiplayerModel _model;

        private readonly List<(MultiplayerState State, string Name)> _states = new();


        public MultiplayerModelTests()
        {
            _settings.SetupGet(settings => settings.PlayerName).Returns("Jens");
            _network.Setup(network => network.Start(It.IsAny<string>())).Returns(true);
            _network.Setup(network => network.Connect(It.IsAny<ulong>())).Returns(true);
            _network.Setup(network => network.Send(It.IsAny<ulong>(), It.IsAny<uint>(), It.IsAny<byte[]>())).Returns(true);

            _model = new MultiplayerModel(_communication.Object, _network.Object, _settings.Object, _gameService.Object);
            _model.OnMultiplayerStateChanged += (state, name) => _states.Add((state, name));
        }


        private void ConnectTo(NetworkPeer peer)
        {
            _network.Raise(network => network.Connected += null, peer);
        }


        private void Receive(NetworkPeer from, ChessMessageType type, params byte[] data)
        {
            _network.Raise(network => network.MessageReceived += null, from.Id, (uint)type, data);
        }


        private void InMultiplayerGame()
        {
            _gameService.SetupGet(service => service.IsMultiplayerGame).Returns(true);
        }


        [Fact]
        public void StartMultiplayer_StartsNetworkingWithThePlayerName()
        {
            _model.StartMultiplayer();

            _network.Verify(network => network.Start("Jens"), Times.Once);
        }


        [Fact]
        public void FindOpponent_ListsEveryDiscoveredPlayerOnce()
        {
            var opponents = new List<string>();
            _model.OnOpponentDiscovered += opponents.Add;

            _model.FindOpponent();
            _network.Raise(network => network.PeerDiscovered += null, Anna);
            _network.Raise(network => network.PeerDiscovered += null, Anna);
            _network.Raise(network => network.PeerDiscovered += null, Ben);

            _network.Verify(network => network.StartDiscovery(), Times.Once);
            Assert.Equal(new[] { "Anna", "Ben" }, opponents);
            Assert.Equal(MultiplayerState.Searching, _states.Single().State);
        }


        [Fact]
        public void ConnectToOpponent_ConnectsToThePlayerAtThatIndex()
        {
            _network.Raise(network => network.PeerDiscovered += null, Anna);
            _network.Raise(network => network.PeerDiscovered += null, Ben);

            _model.ConnectToOpponent(1);
            _model.ConnectToOpponent(5);

            _network.Verify(network => network.Connect(Ben.Id), Times.Once);
            _network.Verify(network => network.Connect(It.IsAny<ulong>()), Times.Once);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConnectionRequest_IsShownAndAnswered(bool accept)
        {
            _network.Raise(network => network.ConnectionRequested += null, Anna);

            _model.RespondToConnectionRequest(accept);

            Assert.Equal((MultiplayerState.ConnectionRequested, "Anna"), _states.Single());
            _network.Verify(network => network.Accept(Anna.Id), accept ? Times.Once() : Times.Never());
            _network.Verify(network => network.Decline(Anna.Id), accept ? Times.Never() : Times.Once());
        }


        [Fact]
        public void Connected_LeadsToThePlayerSetup()
        {
            ConnectTo(Anna);

            Assert.Equal(new[] { MultiplayerState.Connected, MultiplayerState.PlayerSetup }, _states.Select(change => change.State));
            _network.Verify(network => network.StopDiscovery(), Times.Once);
        }


        [Fact]
        public void ChosenColor_IsSentAndTheOpponentGetsTheOtherOne()
        {
            Side? localFromRemote = null;
            _model.OnMultiplayerPlayerSetFromRemote += side => localFromRemote = side;
            ConnectTo(Anna);

            _model.SetLocalPlayerColor(Side.Black);
            Receive(Anna, ChessMessageType.PlayerChosen, (byte)Side.White);

            _network.Verify(network => network.Send(Anna.Id, (uint)ChessMessageType.PlayerChosen, new byte[] { (byte)Side.Black }), Times.Once);
            Assert.Equal(Side.Black, localFromRemote);
        }


        [Fact]
        public void Game_StartsOnceBothPlayersAreReady()
        {
            ConnectTo(Anna);

            _model.SetPlayerReady(true);
            Assert.DoesNotContain(_states, change => change.State == MultiplayerState.InGame);

            Receive(Anna, ChessMessageType.PlayerReady, 1);

            Assert.Equal((MultiplayerState.InGame, "Anna"), _states.Last());
            _network.Verify(network => network.Send(Anna.Id, (uint)ChessMessageType.PlayerReady, new byte[] { 1 }), Times.Once);
        }


        [Fact]
        public void OpponentsMove_IsSubmittedToTheEngine()
        {
            InMultiplayerGame();
            ConnectTo(Anna);

            Receive(Anna, ChessMessageType.Move, (byte)Square.e7, (byte)Square.e5, 0xFF);
            Receive(Anna, ChessMessageType.Move, (byte)Square.b2, (byte)Square.b1, 3);

            _communication.Verify(layer => layer.SubmitMove(Square.e7, Square.e5, PieceType.None), Times.Once);
            _communication.Verify(layer => layer.SubmitMove(Square.b2, Square.b1, PieceType.WQueen), Times.Once);
        }


        [Fact]
        public void MovesFromAnotherPeerOrBrokenMessages_AreIgnored()
        {
            InMultiplayerGame();
            ConnectTo(Anna);

            Receive(Ben, ChessMessageType.Move, (byte)Square.e7, (byte)Square.e5, 0xFF);
            Receive(Anna, ChessMessageType.Move, (byte)Square.e7);
            Receive(Anna, ChessMessageType.Move, 200, (byte)Square.e5, 0xFF);

            _communication.Verify(layer => layer.SubmitMove(It.IsAny<Square>(), It.IsAny<Square>(), It.IsAny<PieceType>()), Times.Never);
        }


        [Fact]
        public void LocalMove_IsSentToTheOpponent()
        {
            InMultiplayerGame();
            ConnectTo(Anna);

            _communication.Raise(layer => layer.MoveExecuted += null, new Move(Square.e2, Square.e4, MoveFlag.DoublePawnPush), "e4");
            _communication.Raise(layer => layer.MoveExecuted += null, new Move(Square.g7, Square.g8, MoveFlag.KnightPromotion), "g8=N");

            _network.Verify(network => network.Send(Anna.Id, (uint)ChessMessageType.Move, new byte[] { (byte)Square.e2, (byte)Square.e4, 0xFF }), Times.Once);
            _network.Verify(network => network.Send(Anna.Id, (uint)ChessMessageType.Move, new byte[] { (byte)Square.g7, (byte)Square.g8, 0 }), Times.Once);
        }


        [Fact]
        public void OpponentsMove_IsNotSentBack()
        {
            InMultiplayerGame();
            ConnectTo(Anna);

            Receive(Anna, ChessMessageType.Move, (byte)Square.e7, (byte)Square.e5, 0xFF);
            _communication.Raise(layer => layer.MoveExecuted += null, new Move(Square.e7, Square.e5, MoveFlag.DoublePawnPush), "e5");

            _network.Verify(network => network.Send(It.IsAny<ulong>(), (uint)ChessMessageType.Move, It.IsAny<byte[]>()), Times.Never);
        }


        [Fact]
        public void MovesOutsideAMultiplayerGame_AreNotSent()
        {
            ConnectTo(Anna);

            _communication.Raise(layer => layer.MoveExecuted += null, new Move(Square.e2, Square.e4, MoveFlag.DoublePawnPush), "e4");

            _network.Verify(network => network.Send(It.IsAny<ulong>(), (uint)ChessMessageType.Move, It.IsAny<byte[]>()), Times.Never);
        }


        [Fact]
        public void OpponentLeaving_EndsTheSession()
        {
            InMultiplayerGame();
            ConnectTo(Anna);

            _network.Raise(network => network.Disconnected += null, Anna.Id, NetLinkAPI.DisconnectReason.Remote);
            Receive(Anna, ChessMessageType.Move, (byte)Square.e7, (byte)Square.e5, 0xFF);

            Assert.Equal((MultiplayerState.Disconnected, "Anna"), _states.Last());
            _communication.Verify(layer => layer.SubmitMove(It.IsAny<Square>(), It.IsAny<Square>(), It.IsAny<PieceType>()), Times.Never);
        }


        [Fact]
        public void DeclinedConnection_IsReportedAsAnError()
        {
            string? error = null;
            _model.OnConnectionErrorOccured += message => error = message;

            _network.Raise(network => network.Disconnected += null, Anna.Id, NetLinkAPI.DisconnectReason.Declined);

            Assert.NotNull(error);
            Assert.Empty(_states);
        }


        [Fact]
        public void PlayerChanges_AreForwarded()
        {
            Side? currentPlayer = null;
            _model.OnPlayerChanged += side => currentPlayer = side;

            _communication.Raise(layer => layer.PlayerChanged += null, Side.Black);

            Assert.Equal(Side.Black, currentPlayer);
        }
    }


    public class NetworkAdapterTests
    {
        [Fact]
        public void Adapter_NeedsAnIdAndANetworkName()
        {
            Assert.True(new NetworkAdapter { ID = 3UL, NetworkName = "Home" }.IsValid());
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
