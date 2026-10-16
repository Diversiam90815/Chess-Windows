using Chess.UI.Board;
using Chess.UI.Models;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Test
{
    public class MoveTests
    {
        [Fact]
        public void Move_EncodesSourceTargetAndFlags()
        {
            var move = new Move(Square.e2, Square.e4, MoveFlag.DoublePawnPush);

            Assert.Equal(Square.e2, move.From);
            Assert.Equal(Square.e4, move.To);
            Assert.Equal(MoveFlag.DoublePawnPush, move.Flags);
            Assert.True(move.IsDoublePush);
            Assert.True(move.IsValid);
        }


        [Fact]
        public void Move_CanBeRebuiltFromItsRawValue()
        {
            var move = new Move(Square.h7, Square.g8, MoveFlag.QueenPromoCapture);

            var rebuilt = new Move(move.Raw);

            Assert.Equal(Square.h7, rebuilt.From);
            Assert.Equal(Square.g8, rebuilt.To);
            Assert.Equal(MoveFlag.QueenPromoCapture, rebuilt.Flags);
        }


        [Fact]
        public void Move_UsesTheEngineBitLayout()
        {
            // 6 bits source, 6 bits target, 4 bits flags
            var move = new Move(Square.b8, Square.c8, MoveFlag.Capture);

            Assert.Equal((ushort)(1 | (2 << 6) | (0b0100 << 12)), move.Raw);
        }


        [Theory]
        [InlineData(MoveFlag.Quiet, false, false)]
        [InlineData(MoveFlag.DoublePawnPush, false, false)]
        [InlineData(MoveFlag.KingCastle, false, false)]
        [InlineData(MoveFlag.QueenCastle, false, false)]
        [InlineData(MoveFlag.Capture, true, false)]
        [InlineData(MoveFlag.EnPassant, true, false)]
        [InlineData(MoveFlag.KnightPromotion, false, true)]
        [InlineData(MoveFlag.BishopPromotion, false, true)]
        [InlineData(MoveFlag.RookPromotion, false, true)]
        [InlineData(MoveFlag.QueenPromotion, false, true)]
        [InlineData(MoveFlag.KnightPromoCapture, true, true)]
        [InlineData(MoveFlag.BishopPromoCapture, true, true)]
        [InlineData(MoveFlag.RookPromoCapture, true, true)]
        [InlineData(MoveFlag.QueenPromoCapture, true, true)]
        public void Move_ReportsCapturesAndPromotions(MoveFlag flag, bool isCapture, bool isPromotion)
        {
            var move = new Move(Square.a7, Square.b8, flag);

            Assert.Equal(isCapture, move.IsCapture);
            Assert.Equal(isPromotion, move.IsPromotion);
            Assert.Equal(flag == MoveFlag.Quiet, move.IsQuiet);
        }


        [Fact]
        public void Move_ReportsSpecialMoves()
        {
            Assert.True(new Move(Square.e1, Square.g1, MoveFlag.KingCastle).IsCastle);
            Assert.True(new Move(Square.e1, Square.c1, MoveFlag.QueenCastle).IsCastle);
            Assert.False(new Move(Square.e1, Square.f1).IsCastle);

            Assert.True(new Move(Square.e5, Square.d6, MoveFlag.EnPassant).IsEnPassant);
            Assert.False(new Move(Square.e5, Square.d6, MoveFlag.Capture).IsEnPassant);
        }


        [Fact]
        public void Move_None_IsNotValid()
        {
            Assert.False(Move.None.IsValid);
            Assert.Equal("None", Move.None.ToString());
        }


        [Fact]
        public void Move_ToString_NamesSquaresAndFlag()
        {
            Assert.Equal("e2->e4 (DoublePawnPush)", new Move(Square.e2, Square.e4, MoveFlag.DoublePawnPush).ToString());
        }
    }


    public class SquareTests
    {
        [Theory]
        [InlineData(Square.a8, 0, 0, "a8")]
        [InlineData(Square.h8, 7, 0, "h8")]
        [InlineData(Square.e4, 4, 4, "e4")]
        [InlineData(Square.a1, 0, 7, "a1")]
        [InlineData(Square.h1, 7, 7, "h1")]
        public void Square_KnowsItsFileRankAndName(Square square, int file, int rank, string name)
        {
            Assert.Equal(file, square.File());
            Assert.Equal(rank, square.Rank());
            Assert.Equal(name, square.ToAlgebraic());
        }


        [Fact]
        public void Square_None_HasNoAlgebraicName()
        {
            Assert.Equal("None", Square.None.ToAlgebraic());
        }


        [Fact]
        public void Square_IsBuiltFromFileAndRank()
        {
            Assert.Equal(Square.a8, SquareExtension.FromFileRank(0, 0));
            Assert.Equal(Square.e4, SquareExtension.FromFileRank(4, 4));
            Assert.Equal(Square.h1, SquareExtension.FromFileRank(7, 7));
        }


        [Theory]
        [InlineData(-1, 0)]
        [InlineData(8, 0)]
        [InlineData(0, -1)]
        [InlineData(0, 8)]
        public void Square_OutsideTheBoard_IsNone(int file, int rank)
        {
            Assert.Equal(Square.None, SquareExtension.FromFileRank(file, rank));
            Assert.Equal(Square.None, SquareExtension.FromDisplayCoordinates(file, rank));
        }


        [Fact]
        public void Square_DisplayCoordinatesStartAtTheTopLeft()
        {
            Assert.Equal(Square.a8, SquareExtension.FromDisplayCoordinates(0, 0));
            Assert.Equal(Square.h8, SquareExtension.FromDisplayCoordinates(7, 0));
            Assert.Equal(Square.a1, SquareExtension.FromDisplayCoordinates(0, 7));
        }


        [Fact]
        public void Square_EveryIndexRoundTrips()
        {
            for (int index = 0; index < 64; index++)
            {
                var square = (Square)index;

                Assert.Equal(square, SquareExtension.FromFileRank(square.File(), square.Rank()));
            }
        }
    }


    public class GameConfigurationTests
    {
        [Fact]
        public void LocalCoop_IsValid()
        {
            GameConfiguration config = GameConfiguration.CreateLocalCoop();

            Assert.Equal(GameModeSelection.LocalCoop, config.Mode);
            Assert.True(config.IsValid());
        }


        [Fact]
        public void SinglePlayer_KeepsColorAndDifficulty()
        {
            GameConfiguration config = GameConfiguration.CreateSinglePlayer(Side.Black, CPUDifficulty.Medium);

            Assert.Equal(GameModeSelection.SinglePlayer, config.Mode);
            Assert.Equal(Side.Black, config.PlayerColor);
            Assert.Equal(CPUDifficulty.Medium, config.CpuDifficulty);
            Assert.True(config.IsValid());
        }


        [Fact]
        public void SinglePlayer_NeedsColorAndDifficulty()
        {
            Assert.False(GameConfiguration.CreateSinglePlayer(Side.None, CPUDifficulty.Easy).IsValid());
            Assert.False(GameConfiguration.CreateSinglePlayer(Side.White, CPUDifficulty.None).IsValid());
        }


        [Fact]
        public void Multiplayer_NeedsAColor()
        {
            Assert.True(GameConfiguration.CreateMultiplayer(Side.White).IsValid());
            Assert.False(GameConfiguration.CreateMultiplayer(Side.None).IsValid());
        }


        [Fact]
        public void WithoutAMode_IsNotValid()
        {
            Assert.False(default(GameConfiguration).IsValid());
        }
    }
}
