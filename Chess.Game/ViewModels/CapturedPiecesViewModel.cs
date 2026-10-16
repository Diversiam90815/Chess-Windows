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


        public void ClearCaptures()
        {
            _model.Clear();

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
            // The model counts by the piece that was taken, so a captured black piece belongs to white's tally
            return piece switch
            {
                PieceType.BPawn => nameof(WhiteCapturedPawn),
                PieceType.BBishop => nameof(WhiteCapturedBishop),
                PieceType.BKnight => nameof(WhiteCapturedKnight),
                PieceType.BRook => nameof(WhiteCapturedRook),
                PieceType.BQueen => nameof(WhiteCapturedQueen),
                PieceType.WPawn => nameof(BlackCapturedPawn),
                PieceType.WBishop => nameof(BlackCapturedBishop),
                PieceType.WKnight => nameof(BlackCapturedKnight),
                PieceType.WRook => nameof(BlackCapturedRook),
                PieceType.WQueen => nameof(BlackCapturedQueen),
                _ => null
            };
        }


        #region Captured Piece Counts

        // Pieces white has captured
        public int WhiteCapturedPawn => _model.CapturedPieces[PieceType.BPawn];
        public int WhiteCapturedBishop => _model.CapturedPieces[PieceType.BBishop];
        public int WhiteCapturedKnight => _model.CapturedPieces[PieceType.BKnight];
        public int WhiteCapturedRook => _model.CapturedPieces[PieceType.BRook];
        public int WhiteCapturedQueen => _model.CapturedPieces[PieceType.BQueen];

        // Pieces black has captured
        public int BlackCapturedPawn => _model.CapturedPieces[PieceType.WPawn];
        public int BlackCapturedBishop => _model.CapturedPieces[PieceType.WBishop];
        public int BlackCapturedKnight => _model.CapturedPieces[PieceType.WKnight];
        public int BlackCapturedRook => _model.CapturedPieces[PieceType.WRook];
        public int BlackCapturedQueen => _model.CapturedPieces[PieceType.WQueen];

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
