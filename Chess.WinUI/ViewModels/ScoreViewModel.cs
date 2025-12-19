using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Chess.UI.Services.EngineAPI;
using Chess.UI.Score;
using Chess.UI.Images;
using Chess.UI.Wrappers;


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
            _scoreModel.PlayerScoreUpdated += OnPlayerScoreUpdated;
        }


        private void Init()
        {
            CapturedWhitePawnImage = _images.GetCapturedPieceImage(PlayerColor.White, PieceTypeInstance.Pawn);
            CapturedWhiteBishopImage = _images.GetCapturedPieceImage(PlayerColor.White, PieceTypeInstance.Bishop);
            CapturedWhiteRookImage = _images.GetCapturedPieceImage(PlayerColor.White, PieceTypeInstance.Rook);
            CapturedWhiteQueenImage = _images.GetCapturedPieceImage(PlayerColor.White, PieceTypeInstance.Queen);
            CapturedWhiteKnightImage = _images.GetCapturedPieceImage(PlayerColor.White, PieceTypeInstance.Knight);
            CapturedBlackPawnImage = _images.GetCapturedPieceImage(PlayerColor.Black, PieceTypeInstance.Pawn);
            CapturedBlackBishopImage = _images.GetCapturedPieceImage(PlayerColor.Black, PieceTypeInstance.Bishop);
            CapturedBlackRookImage = _images.GetCapturedPieceImage(PlayerColor.Black, PieceTypeInstance.Rook);
            CapturedBlackQueenImage = _images.GetCapturedPieceImage(PlayerColor.Black, PieceTypeInstance.Queen);
            CapturedBlackKnightImage = _images.GetCapturedPieceImage(PlayerColor.Black, PieceTypeInstance.Knight);
        }


        public void OnPlayerScoreUpdated(Services.EngineAPI.Score score)
        {
            int value = score.score;
            PlayerColor player = score.player;

            if (player == PlayerColor.White)
            {
                WhiteScoreValue = value;
            }
            else if (player == PlayerColor.Black)
            {
                BlackScoreValue = value;
            }
        }


        public void OnPlayerCapturedPiece(PlayerCapturedPiece piece)
        {
            PlayerColor player = piece.playerColor;
            PieceTypeInstance type = piece.pieceType;
            bool captured = piece.captured;
            AlterCapturedPieces(player, type, captured);
        }


        public void ReinitScoreValues()
        {
            BlackScoreValue = 0;
            BlackCapturedBishop = 0;
            BlackCapturedKnight = 0;
            BlackCapturedQueen = 0;
            BlackCapturedRook = 0;
            BlackCapturedPawn = 0;

            WhiteScoreValue = 0;
            WhiteCapturedBishop = 0;
            WhiteCapturedKnight = 0;
            WhiteCapturedQueen = 0;
            WhiteCapturedRook = 0;
            WhiteCapturedPawn = 0;
        }


        private void AlterCapturedPieces(PlayerColor player, PieceTypeInstance pieceType, bool captured)
        {
            switch (player)
            {
                case PlayerColor.White:
                    {
                        if (_scoreModel.WhiteCapturedPieces.TryGetValue(pieceType, out int value))
                        {
                            if (captured)
                            {
                                _scoreModel.WhiteCapturedPieces[pieceType] = ++value;
                            }
                            else
                            {
                                _scoreModel.WhiteCapturedPieces[pieceType] = --value;
                            }
                            OnPropertyChanged($"WhiteCaptured{pieceType}");
                        }
                        break;
                    }

                case PlayerColor.Black:
                    {
                        if (_scoreModel.BlackCapturedPieces.TryGetValue(pieceType, out int value))
                        {
                            if (captured)
                            {
                                _scoreModel.BlackCapturedPieces[pieceType] = ++value;
                            }
                            else
                            {
                                _scoreModel.BlackCapturedPieces[pieceType] = --value;
                            }
                            OnPropertyChanged($"BlackCaptured{pieceType}");
                        }
                        break;
                    }
                default: break;
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
            get => _scoreModel.BlackCapturedPieces[PieceTypeInstance.Pawn];
            set
            {
                if (_scoreModel.BlackCapturedPieces[PieceTypeInstance.Pawn] != value)
                {
                    _scoreModel.BlackCapturedPieces[PieceTypeInstance.Pawn] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int BlackCapturedBishop
        {
            get => _scoreModel.BlackCapturedPieces[PieceTypeInstance.Bishop];
            set
            {
                if (_scoreModel.BlackCapturedPieces[PieceTypeInstance.Bishop] != value)
                {
                    _scoreModel.BlackCapturedPieces[PieceTypeInstance.Bishop] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int BlackCapturedKnight
        {
            get => _scoreModel.BlackCapturedPieces[PieceTypeInstance.Knight];
            set
            {
                if (_scoreModel.BlackCapturedPieces[PieceTypeInstance.Knight] != value)
                {
                    _scoreModel.BlackCapturedPieces[PieceTypeInstance.Knight] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int BlackCapturedQueen
        {
            get => _scoreModel.BlackCapturedPieces[PieceTypeInstance.Queen];
            set
            {
                if (_scoreModel.BlackCapturedPieces[PieceTypeInstance.Queen] != value)
                {
                    _scoreModel.BlackCapturedPieces[PieceTypeInstance.Queen] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int BlackCapturedRook
        {
            get => _scoreModel.BlackCapturedPieces[PieceTypeInstance.Rook];
            set
            {
                if (_scoreModel.BlackCapturedPieces[PieceTypeInstance.Rook] != value)
                {
                    _scoreModel.BlackCapturedPieces[PieceTypeInstance.Rook] = value;
                    OnPropertyChanged();
                }
            }
        }


        public int WhiteCapturedPawn
        {
            get => _scoreModel.WhiteCapturedPieces[PieceTypeInstance.Pawn];
            set
            {
                if (_scoreModel.WhiteCapturedPieces[PieceTypeInstance.Pawn] != value)
                {
                    _scoreModel.WhiteCapturedPieces[PieceTypeInstance.Pawn] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int WhiteCapturedBishop
        {
            get => _scoreModel.WhiteCapturedPieces[PieceTypeInstance.Bishop];
            set
            {
                if (_scoreModel.WhiteCapturedPieces[PieceTypeInstance.Bishop] != value)
                {
                    _scoreModel.WhiteCapturedPieces[PieceTypeInstance.Bishop] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int WhiteCapturedKnight
        {
            get => _scoreModel.WhiteCapturedPieces[PieceTypeInstance.Knight];
            set
            {
                if (_scoreModel.WhiteCapturedPieces[PieceTypeInstance.Knight] != value)
                {
                    _scoreModel.WhiteCapturedPieces[PieceTypeInstance.Knight] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int WhiteCapturedQueen
        {
            get => _scoreModel.WhiteCapturedPieces[PieceTypeInstance.Queen];
            set
            {
                if (_scoreModel.WhiteCapturedPieces[PieceTypeInstance.Queen] != value)
                {
                    _scoreModel.WhiteCapturedPieces[PieceTypeInstance.Queen] = value;
                    OnPropertyChanged();
                }
            }
        }

        public int WhiteCapturedRook
        {
            get => _scoreModel.WhiteCapturedPieces[PieceTypeInstance.Rook];
            set
            {
                if (_scoreModel.WhiteCapturedPieces[PieceTypeInstance.Rook] != value)
                {
                    _scoreModel.WhiteCapturedPieces[PieceTypeInstance.Rook] = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion


        #region Score Values

        private int whiteScoreValue = 0;
        public int WhiteScoreValue
        {
            get => whiteScoreValue;
            set
            {
                if (whiteScoreValue != value)
                {
                    whiteScoreValue = value;
                    OnPropertyChanged();
                }
            }
        }


        private int blackScoreValue = 0;
        public int BlackScoreValue
        {
            get => blackScoreValue;
            set
            {
                if (blackScoreValue != value)
                {
                    blackScoreValue = value;
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
