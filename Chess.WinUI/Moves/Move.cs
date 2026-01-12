using static Chess.UI.Services.EngineAPI;

namespace Chess.UI.Moves
{
    public enum Square
    {
        a8, b8, c8, d8, e8, f8, g8, h8,
        a7, b7, c7, d7, e7, f7, g7, h7,
        a6, b6, c6, d6, e6, f6, g6, h6,
        a5, b5, c5, d5, e5, f5, g5, h5,
        a4, b4, c4, d4, e4, f4, g4, h4,
        a3, b3, c3, d3, e3, f3, g3, h3,
        a2, b2, c2, d2, e2, f2, g2, h2,
        a1, b1, c1, d1, e1, f1, g1, h1,
        None
    }


    /// <summary>
    /// Move flags matching the C++ MoveFlag enum
    /// </summary>
    public enum MoveFlag : byte
    {
        Quiet = 0b0000,
        DoublePawnPush = 0b0001,
        KingCastle = 0b0010,
        QueenCastle = 0b0011,
        Capture = 0b0100,
        EnPassant = 0b0101,
        KnightPromotion = 0b1000,
        BishopPromotion = 0b1001,
        RookPromotion = 0b1010,
        QueenPromotion = 0b1011,
        KnightPromoCapture = 0b1100,
        BishopPromoCapture = 0b1101,
        RookPromoCapture = 0b1110,
        QueenPromoCapture = 0b1111
    }


    /// <summary>
    /// Represents a chess move encoded in 16 bits
    /// Encoding: 0-5: source square, 6-11: target square, 12-15: flags
    /// </summary>
    public readonly struct Move
    {
        private readonly ushort _data;

        public Move(ushort data)
        {
            _data = data;
        }

        public Move(Square from, Square to, MoveFlag flags = MoveFlag.Quiet)
        {
            _data = (ushort)(((int)from) | ((int)to << 6) | ((int)flags << 12));
        }

        // Accessors
        public Square From => (Square)(_data & 0x3F);
        public Square To => (Square)((_data >> 6) & 0x3F);
        public MoveFlag Flags => (MoveFlag)((_data >> 12) & 0x0F);
        public ushort Raw => _data;


        // Move type checks
        public bool IsQuiet => Flags == MoveFlag.Quiet;
        public bool IsCapture => ((_data >> 12) & 0b0100) != 0;
        public bool IsPromotion => ((_data >> 12) & 0b1000) != 0;
        public bool IsCastle => Flags == MoveFlag.KingCastle || Flags == MoveFlag.QueenCastle;
        public bool IsEnPassant => Flags == MoveFlag.EnPassant;
        public bool IsDoublePush => Flags == MoveFlag.DoublePawnPush;
        public bool IsValid => _data != 0;


        // Get promotion piece type (only valid if IsPromotion is true)
        public PieceType PromotionPieceOffset => ((PieceType)((int)Flags & 0b0011));

        public static Move None => new Move(0);

        public override string ToString()
        {
            if (!IsValid) return "None";
            return $"{From}->{To} ({Flags})";
        }
    }
}
