using Chess.UI.Styles;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Board
{
    public interface IBoardSquare : INotifyPropertyChanged
    {
        // Properties
        PieceStyle _pieceStyle { get; }
        PositionInstance pos { get; set; }
        PieceTypeInstance piece { get; set; }
        PlayerColor colour { get; set; }
        bool IsHighlighted { get; set; }
        Brush BackgroundBrush { get; }
        ImageSource PieceImage { get; }

        // For responding to theme changes
        void UpdatePieceTheme(PieceStyle pieceTheme);
    }
}
