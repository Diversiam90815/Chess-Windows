using static Chess.UI.Services.EngineAPI;

namespace Chess.UI.Moves
{
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
