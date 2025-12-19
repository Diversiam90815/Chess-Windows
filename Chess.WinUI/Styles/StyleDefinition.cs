
namespace Chess.UI.Styles
{
    public enum BoardStyle
    {
        Wood = 1,
        Wood2 = 2,
        Plain,
        Plastic,
        Marble,
        Marble2,
        Glass
    }


    public enum PieceStyle
    {
        Basic = 1,
        Standard
    }


    public class BoardStyleInfo
    {
        public string Name { get; set; }
        public BoardStyle Style { get; set; }
        public string DisplayName { get; set; }
    }


    public class PieceStyleInfo
    {
        public string Name { get; set; }
        public PieceStyle Style { get; set; }
        public string DisplayName { get; set; }
    }
}
