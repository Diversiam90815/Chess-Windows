using Chess.UI.Services;
using Chess.UI.Styles;
using Chess.UI.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Board
{
    public interface IBoardSquare : INotifyPropertyChanged
    {
        // Properties
        Square Position { get; set; }
        PieceType Piece { get; set; }
        bool IsHighlighted { get; set; }
        Brush BackgroundBrush { get; }
        ImageSource PieceImage { get; }
    }


    public class BoardSquare : IBoardSquare
    {
        private readonly IDispatcherQueueWrapper _dispatcherQueue;

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IStyleManager _styleManager;

        private readonly IImageService _images;


        public BoardSquare() : this(Square.None, PieceType.None)
        {
        }


        public BoardSquare(Square position, PieceType piece)
        {
            // Squares only read from the shared services and never subscribe to them: a new set of squares is
            // created with every game, and a subscription would keep each of them alive for the app's lifetime
            _dispatcherQueue = App.Current?.Services.GetService<IDispatcherQueueWrapper>();
            _styleManager = App.Current?.Services.GetService<IStyleManager>();
            _images = App.Current?.Services.GetService<IImageService>();

            _position = position;
            _piece = piece;
        }


        private Square _position;
        public Square Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged();
                }
            }
        }


        private PieceType _piece;
        public PieceType Piece
        {
            get => _piece;
            set
            {
                if (_piece != value)
                {
                    _piece = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PieceImage));
                }
            }
        }


        // Display coordinates for UI grid positioning
        public int DisplayX => Position.File();
        public int DisplayY => Position.Rank();


        private bool isHighlighted;
        public bool IsHighlighted
        {
            get => isHighlighted;
            set
            {
                if (isHighlighted != value)
                {
                    isHighlighted = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BackgroundBrush)); // if you choose a computed brush
                }
            }
        }


        public Brush BackgroundBrush
        {
            get
            {
                // Return a special color if IsHighlighted, otherwise transparent
                return IsHighlighted
                    ? new SolidColorBrush(Windows.UI.Color.FromArgb(128, 173, 216, 230)) // a light blue-ish
                    : new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
            }
        }


        public ImageSource PieceImage
        {
            get
            {
                if (Piece == PieceType.None || _images == null || _styleManager == null)
                    return null;

                return _images.GetPieceImage(_styleManager.CurrentPieceStyle, Piece);
            }
        }


        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            _dispatcherQueue?.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }
}
