using Chess.UI.Models;
using Chess.UI.Services;
using Chess.UI.Wrappers;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.ViewModels
{
    public class CapturedPiecesViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ICapturedPiecesModel _model;
        private readonly IImageService _images;
        private readonly IDispatcherQueueWrapper _dispatcherQueue;

        // Captured piece images (readonly - set once)
        public ImageSource CapturedWhitePawnImage { get; }
        public ImageSource CapturedWhiteBishopImage { get; }
        public ImageSource CapturedWhiteKnightImage { get; }
        public ImageSource CapturedWhiteRookImage { get; }
        public ImageSource CapturedWhiteQueenImage { get; }
        public ImageSource CapturedBlackPawnImage { get; }
        public ImageSource CapturedBlackBishopImage { get; }
        public ImageSource CapturedBlackKnightImage { get; }
        public ImageSource CapturedBlackRookImage { get; }
        public ImageSource CapturedBlackQueenImage { get; }


        public CapturedPiecesViewModel(IDispatcherQueueWrapper dispatcher, ICapturedPiecesModel model, IImageService images)
        {
            _model = model;
            _images = images;
            _dispatcherQueue = dispatcher;

            // Initialize images once
            CapturedWhitePawnImage = _images.GetCapturedPieceImage(PieceType.WPawn);
            CapturedWhiteBishopImage = _images.GetCapturedPieceImage(PieceType.WBishop);
            CapturedWhiteKnightImage = _images.GetCapturedPieceImage(PieceType.WKnight);
            CapturedWhiteRookImage = _images.GetCapturedPieceImage(PieceType.WRook);
            CapturedWhiteQueenImage = _images.GetCapturedPieceImage(PieceType.WQueen);
            CapturedBlackPawnImage = _images.GetCapturedPieceImage(PieceType.BPawn);
            CapturedBlackBishopImage = _images.GetCapturedPieceImage(PieceType.BBishop);
            CapturedBlackKnightImage = _images.GetCapturedPieceImage(PieceType.BKnight);
            CapturedBlackRookImage = _images.GetCapturedPieceImage(PieceType.BRook);
            CapturedBlackQueenImage = _images.GetCapturedPieceImage(PieceType.BQueen);

            _model.PieceCaptured += OnPieceCaptured;
            _model.CapturedPiecesCleared += OnCapturedPiecesCleared;
        }


        private void OnPieceCaptured(PlayerCapturedPiece piece)
        {
            // Notify property change for the specific piece count
            string propertyName = GetPropertyNameForPiece(piece.pieceType);
            if (!string.IsNullOrEmpty(propertyName))
            {
                OnPropertyChanged(propertyName);
            }
        }


        private void OnCapturedPiecesCleared()
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                // Notify all piece count properties changed
                OnPropertyChanged(nameof(WhiteCapturedPawn));
                OnPropertyChanged(nameof(WhiteCapturedBishop));
                OnPropertyChanged(nameof(WhiteCapturedKnight));
                OnPropertyChanged(nameof(WhiteCapturedRook));
                OnPropertyChanged(nameof(WhiteCapturedQueen));
                OnPropertyChanged(nameof(BlackCapturedPawn));
                OnPropertyChanged(nameof(BlackCapturedBishop));
                OnPropertyChanged(nameof(BlackCapturedKnight));
                OnPropertyChanged(nameof(BlackCapturedRook));
                OnPropertyChanged(nameof(BlackCapturedQueen));
            });
        }


        private string GetPropertyNameForPiece(PieceType piece)
        {
            return piece switch
            {
                PieceType.WPawn => nameof(WhiteCapturedPawn),
                PieceType.WBishop => nameof(WhiteCapturedBishop),
                PieceType.WKnight => nameof(WhiteCapturedKnight),
                PieceType.WRook => nameof(WhiteCapturedRook),
                PieceType.WQueen => nameof(WhiteCapturedQueen),
                PieceType.BPawn => nameof(BlackCapturedPawn),
                PieceType.BBishop => nameof(BlackCapturedBishop),
                PieceType.BKnight => nameof(BlackCapturedKnight),
                PieceType.BRook => nameof(BlackCapturedRook),
                PieceType.BQueen => nameof(BlackCapturedQueen),
                _ => null
            };
        }


        #region Captured Piece Counts

        public int WhiteCapturedPawn => _model.CapturedPieces[PieceType.WPawn];
        public int WhiteCapturedBishop => _model.CapturedPieces[PieceType.WBishop];
        public int WhiteCapturedKnight => _model.CapturedPieces[PieceType.WKnight];
        public int WhiteCapturedRook => _model.CapturedPieces[PieceType.WRook];
        public int WhiteCapturedQueen => _model.CapturedPieces[PieceType.WQueen];
        public int BlackCapturedPawn => _model.CapturedPieces[PieceType.BPawn];
        public int BlackCapturedBishop => _model.CapturedPieces[PieceType.BBishop];
        public int BlackCapturedKnight => _model.CapturedPieces[PieceType.BKnight];
        public int BlackCapturedRook => _model.CapturedPieces[PieceType.BRook];
        public int BlackCapturedQueen => _model.CapturedPieces[PieceType.BQueen];

        #endregion


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }
}
