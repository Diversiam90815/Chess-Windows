using Chess.UI.Styles;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Services
{
    public interface IImageService
    {
        ImageSource GetPieceImage(PieceStyle theme, PieceType pieceType);
        ImageSource LoadImage(string relativeFilePath);
        ImageSource GetImage(ImageServices.MainMenuButton button);
        ImageSource GetImage(BoardStyle background);
        ImageSource GetCapturedPieceImage(PieceType pieceTypeInstance);
    }


    public class ImageServices : IImageService
    {
        public enum MainMenuButton
        {
            StartGame = 1,
            Settings,
            Multiplayer,
            EndGame
        }

        public Dictionary<BoardStyle, ImageSource> BoardBackgroundImages;

        public Dictionary<MainMenuButton, ImageSource> MainMenuImages;

        public Dictionary<PieceType, ImageSource> CapturedPiecesImages;


        public ImageServices()
        {
            Init();
        }



        private void Init()
        {
            BoardBackgroundImages = new()
        {
            { BoardStyle.Wood, LoadImage("/Assets/Board/Wood.png") },
            { BoardStyle.Wood2, LoadImage("/Assets/Board/Wood2.png") },
            { BoardStyle.Plain, LoadImage("/Assets/Board/Plain.png") },
            { BoardStyle.Plastic, LoadImage("/Assets/Board/Plastic.png") },
            { BoardStyle.Marble, LoadImage("/Assets/Board/Marble.png") },
            { BoardStyle.Marble2, LoadImage("/Assets/Board/Marble2.png") },
            { BoardStyle.Glass, LoadImage("/Assets/Board/Glass.png") }
        };

            MainMenuImages = new()
        {
        {MainMenuButton.StartGame, LoadImage("/Assets/Buttons/pawn.png") },
        {MainMenuButton.Settings, LoadImage("/Assets/Buttons/rook.png") },
        {MainMenuButton.Multiplayer, LoadImage("/Assets/Buttons/queen.png") },
        {MainMenuButton.EndGame, LoadImage("/Assets/Buttons/horse.png") }
        };

            CapturedPiecesImages = new()
        {
        {PieceType.WPawn, LoadImage("/Assets/Pieces/Standard/PawnW.png") },
        {PieceType.WBishop, LoadImage("/Assets/Pieces/Standard/BishopW.png") },
        {PieceType.WQueen, LoadImage("/Assets/Pieces/Standard/QueenW.png") },
        {PieceType.WRook, LoadImage("/Assets/Pieces/Standard/RookW.png") },
        {PieceType.WKnight, LoadImage("/Assets/Pieces/Standard/KnightW.png") },
        {PieceType.WKing, LoadImage("/Assets/Pieces/Standard/KingW.png") },
        {PieceType.BPawn, LoadImage("/Assets/Pieces/Standard/PawnB.png") },
        {PieceType.BBishop, LoadImage("/Assets/Pieces/Standard/BishopB.png") },
        {PieceType.BQueen, LoadImage("/Assets/Pieces/Standard/QueenB.png") },
        {PieceType.BRook, LoadImage("/Assets/Pieces/Standard/RookB.png") },
        {PieceType.BKnight, LoadImage("/Assets/Pieces/Standard/KnightB.png") },
        {PieceType.BKing, LoadImage("/Assets/Pieces/Standard/KingB.png") }
        };
        }


        // Dynamic piece image loading based on theme, color, and type
        public ImageSource GetPieceImage(PieceStyle theme, PieceType piece)
        {
            // Convert enum values to strings that match folder and file naming conventions
            string themeName = theme.ToString();
            string pieceName = piece.ToString();

            // Construct the relative file path. Assumes folder structure: Assets/Pieces/{Theme}/
            string relativePath = $"/Assets/Pieces/{themeName}/{pieceName}.png";

            return LoadImage(relativePath);
        }


        public ImageSource LoadImage(string relativeFilePath)
        {
            return new BitmapImage(new Uri($"ms-appx:///{relativeFilePath.TrimStart('/')}"));
        }


        public ImageSource GetImage(MainMenuButton button) => MainMenuImages[button];

        public ImageSource GetImage(BoardStyle background) => BoardBackgroundImages[background];

        public ImageSource GetCapturedPieceImage(PieceType piece) => CapturedPiecesImages[piece];
    }
}
