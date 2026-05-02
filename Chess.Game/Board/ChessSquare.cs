
namespace Chess.UI.Board
{
    /// <summary>
    /// Represents all 64 squares on a chess board.
    /// Indexed from a8 (0) to h1 (63) matching the engine's internal representation.
    /// </summary>
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



    public static class SquareExtension
    {
        /// <summary>
        /// Gets the file (column) of the square (0-7, a-h)
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        public static int File(this Square square)
        {
            return (int)square % 8;
        }


        /// <summary>
        /// Gets the rank (row) of the square (0-7, from rank 8 to rank 1)
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        public static int Rank(this Square square)
        {
            return (int)square / 8;
        }


        ///// <summary>
        ///// Converts engine rank to display rank (flip vertically)
        ///// </summary>
        //public static int DisplayRank(this Square square)
        //{
        //    return 7 - square.Rank();
        //}


        /// <summary>
        /// Creates a Square from file and rank
        /// </summary>
        public static Square FromFileRank(int file, int rank)
        {
            if (file < 0 || file > 7 || rank < 0 || rank > 7)
                return Square.None;
            return (Square)(rank * 8 + file);
        }


        /// <summary>
        /// Converts to algebraic notation (e.g., "e4")
        /// </summary>
        public static string ToAlgebraic(this Square square)
        {
            if (square == Square.None) return "None";
            char file = (char)('a' + square.File());
            int rank = 8 - square.Rank();
            return $"{file}{rank}";
        }


        /// <summary>
        /// Converts display coordinates (x, y) to Square
        /// Assumes x is file (0-7), y is display rank (0-7 from top to bottom)
        /// </summary>
        public static Square FromDisplayCoordinates(int x, int y)
        {
            if (x < 0 || x > 7 || y < 0 || y > 7)
                return Square.None;

            return (Square)(y * 8 + x);
        }
    }
}
