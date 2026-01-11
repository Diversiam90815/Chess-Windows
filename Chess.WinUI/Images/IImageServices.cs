using Chess.UI.Styles;
using Microsoft.UI.Xaml.Media;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Images
{
    public interface IImageService
    {
        ImageSource GetPieceImage(PieceStyle theme, PieceType pieceType);
        ImageSource LoadImage(string relativeFilePath);
        ImageSource GetImage(ImageServices.MainMenuButton button);
        ImageSource GetImage(BoardStyle background);
        ImageSource GetCapturedPieceImage(PieceType pieceTypeInstance);
    }
}
