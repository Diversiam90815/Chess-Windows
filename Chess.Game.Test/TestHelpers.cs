using Chess.UI.Styles;


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
}
