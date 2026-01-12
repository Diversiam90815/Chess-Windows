using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Chess.UI.Services.EngineAPI;
using Chess.UI.Score;
using Chess.UI.Wrappers;
using Chess.UI.Services;


namespace Chess.UI.ViewModels
{
    public partial class ScoreViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IDispatcherQueueWrapper _dispatcherQueue;

        private readonly IScoreModel _scoreModel;

        private readonly IImageService _images;

        public ScoreViewModel(IDispatcherQueueWrapper dispatcherQueue, IScoreModel model, IImageService images)
        {
            _dispatcherQueue = dispatcherQueue;
            _scoreModel = model;
            _images = images;

            Init();

            _scoreModel.PlayerCapturedPiece += OnPlayerCapturedPiece;
        }


        private void Init()
        {
            CapturedWhitePawnImage = _images.GetCapturedPieceImage(PieceType.WPawn);
            CapturedWhiteBishopImage = _images.GetCapturedPieceImage(PieceType.WBishop);
            CapturedWhiteRookImage = _images.GetCapturedPieceImage(PieceType.WRook);
            CapturedWhiteQueenImage = _images.GetCapturedPieceImage(PieceType.WQueen);
            CapturedWhiteKnightImage = _images.GetCapturedPieceImage(PieceType.WKnight);
            CapturedBlackPawnImage = _images.GetCapturedPieceImage(PieceType.BPawn);
            CapturedBlackBishopImage = _images.GetCapturedPieceImage(PieceType.BBishop);
            CapturedBlackRookImage = _images.GetCapturedPieceImage(PieceType.BRook);
            CapturedBlackQueenImage = _images.GetCapturedPieceImage(PieceType.BQueen);
            CapturedBlackKnightImage = _images.GetCapturedPieceImage(PieceType.BKnight);
        }



        public void OnPlayerCapturedPiece(PlayerCapturedPiece piece)
        {
            Side player = piece.playerColor;
            PieceType type = piece.pieceType;
            bool captured = piece.captured;
            AlterCapturedPieces(type, captured);
        }


        public void ReinitScoreValues()
        {
            BlackCapturedBishop = 0;
            BlackCapturedKnight = 0;
            BlackCapturedQueen = 0;
            BlackCapturedRook = 0;
            BlackCapturedPawn = 0;

            WhiteCapturedBishop = 0;
            WhiteCapturedKnight = 0;
            WhiteCapturedQueen = 0;
            WhiteCapturedRook = 0;
            WhiteCapturedPawn = 0;
        }


        private void AlterCapturedPieces(PieceType piece, bool captured)
        {
            if (_scoreModel.CapturedPieces.TryGetValue(piece, out int value))
            {
                if (captured)
                {
                    _scoreModel.CapturedPieces[piece] = ++value;
                }
                else
                {
                    _scoreModel.CapturedPieces[piece] = --value;
                }
                OnPropertyChanged($"Captured{piece}");
            }
        }


        #region Images Captured Pieces


        private ImageSource capturedWhitePawnImage;
        public ImageSource CapturedWhitePawnImage
        {
            get => capturedWhitePawnImage;
            set
            {
                if (capturedWhitePawnImage != value)
                {
                    capturedWhitePawnImage = value;
                    OnPropertyChanged();
                }
            }
        }

        private ImageSource capturedWhiteBishopImage;
        public ImageSource CapturedWhiteBishopImage
        {
            get => capturedWhiteBishopImage;
            set
            {
                if (capturedWhiteBishopImage != value)
                {
                    capturedWhiteBishopImage = value;
                    OnPropertyChanged();
                }
            }
        }

        private ImageSource capturedWhiteRookImage;
        public ImageSource CapturedWhiteRookImage
        {
            get => capturedWhiteRookImage;
            set
            {
                if (capturedWhiteRookImage != value)
                {
                    capturedWhiteRookImage = value;
                    OnPropertyChanged();
                }
            }
        }

        private ImageSource capturedWhiteQueenImage;
        public ImageSource CapturedWhiteQueenImage
        {
            get => capturedWhiteQueenImage;
            set
            {
                if (capturedWhiteQueenImage != value)
                {
                    capturedWhiteQueenImage = value;
                    OnPropertyChanged();
                }
            }
        }

        private ImageSource capturedWhiteKnightImage;
        public ImageSource CapturedWhiteKnightImage
        {
            get => capturedWhiteKnightImage;
            set
            {
                if (capturedWhiteKnightImage != value)
                {
                    capturedWhiteKnightImage = value;
                    OnPropertyChanged();
                }
            }
        }


        private ImageSource capturedBlackPawnImage;
        public ImageSource CapturedBlackPawnImage
        {
            get => capturedBlackPawnImage;
            set
            {
                if (capturedBlackPawnImage != value)
                {
                    capturedBlackPawnImage = value;
                    OnPropertyChanged();
                }
            }
        }

        private ImageSource capturedBlackBishopImage;
        public ImageSource CapturedBlackBishopImage
        {
            get => capturedBlackBishopImage;
            set
            {
                if (capturedBlackBishopImage != value)
                {
                    capturedBlackBishopImage = value;
                    OnPropertyChanged();
                }
            }
        }

        private ImageSource capturedBlackRookImage;
        public ImageSource CapturedBlackRookImage
        {
            get => capturedBlackRookImage;
            set
            {
                if (capturedBlackRookImage != value)
                {
                    capturedBlackRookImage = value;
                    OnPropertyChanged();
                }
            }
        }

        private ImageSource capturedBlackQueenImage;
        public ImageSource CapturedBlackQueenImage
        {
            get => capturedBlackQueenImage;
            set
            {
                if (capturedBlackQueenImage != value)
                {
                    capturedBlackQueenImage = value;
                    OnPropertyChanged();
                }
            }
        }

        private ImageSource capturedBlackKnightImage;
        public ImageSource CapturedBlackKnightImage
        {
            get => capturedBlackKnightImage;
            set
            {
                if (capturedBlackKnightImage != value)
                {
                    capturedBlackKnightImage = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion


        #region Num Captured Pieces 


        public int BlackCapturedPawn
        {
            get => _scoreModel.CapturedPieces[PieceType.BPawn];
            set
            {
                if (_scoreModel.CapturedPieces[PieceType.BPawn] != value)
                {
                    _scoreModel.CapturedPieces[PieceType.BPawn] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int BlackCapturedBishop
        {
            get => _scoreModel.CapturedPieces[PieceType.BBishop];
            set
            {
                if (_scoreModel.CapturedPieces[PieceType.BBishop] != value)
                {
                    _scoreModel.CapturedPieces[PieceType.BBishop] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int BlackCapturedKnight
        {
            get => _scoreModel.CapturedPieces[PieceType.BKnight];
            set
            {
                if (_scoreModel.CapturedPieces[PieceType.BKnight] != value)
                {
                    _scoreModel.CapturedPieces[PieceType.BKnight] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int BlackCapturedQueen
        {
            get => _scoreModel.CapturedPieces[PieceType.BQueen];
            set
            {
                if (_scoreModel.CapturedPieces[PieceType.BQueen] != value)
                {
                    _scoreModel.CapturedPieces[PieceType.BQueen] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int BlackCapturedRook
        {
            get => _scoreModel.CapturedPieces[PieceType.BRook];
            set
            {
                if (_scoreModel.CapturedPieces[PieceType.BRook] != value)
                {
                    _scoreModel.CapturedPieces[PieceType.BRook] = value;
                    OnPropertyChanged();
                }
            }
        }


        public int WhiteCapturedPawn
        {
            get => _scoreModel.CapturedPieces[PieceType.WPawn];
            set
            {
                if (_scoreModel.CapturedPieces[PieceType.WPawn] != value)
                {
                    _scoreModel.CapturedPieces[PieceType.WPawn] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int WhiteCapturedBishop
        {
            get => _scoreModel.CapturedPieces[PieceType.WBishop];
            set
            {
                if (_scoreModel.CapturedPieces[PieceType.WBishop] != value)
                {
                    _scoreModel.CapturedPieces[PieceType.WBishop] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int WhiteCapturedKnight
        {
            get => _scoreModel.CapturedPieces[PieceType.WKnight];
            set
            {
                if (_scoreModel.CapturedPieces[PieceType.WKnight] != value)
                {
                    _scoreModel.CapturedPieces[PieceType.WKnight] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int WhiteCapturedQueen
        {
            get => _scoreModel.CapturedPieces[PieceType.WQueen];
            set
            {
                if (_scoreModel.CapturedPieces[PieceType.WQueen] != value)
                {
                    _scoreModel.CapturedPieces[PieceType.WQueen] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int WhiteCapturedRook
        {
            get => _scoreModel.CapturedPieces[PieceType.WRook];
            set
            {
                if (_scoreModel.CapturedPieces[PieceType.WRook] != value)
                {
                    _scoreModel.CapturedPieces[PieceType.WRook] = value;
                    OnPropertyChanged();
                }
            }
        }

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
