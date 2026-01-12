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
        PieceStyle _pieceStyle { get; }
        Square Position { get; set; }
        PieceType Piece { get; set; }
        bool IsHighlighted { get; set; }
        Brush BackgroundBrush { get; }
        ImageSource PieceImage { get; }

        // For responding to theme changes
        void UpdatePieceTheme(PieceStyle pieceTheme);
    }


    public class BoardSquare : IBoardSquare
    {
        private readonly IDispatcherQueueWrapper _dispatcherQueue;

        public event PropertyChangedEventHandler PropertyChanged;

        public PieceStyle _pieceStyle { get; private set; }

        private readonly IStyleManager _styleManager;

        private IImageService _images;

        public BoardSquare()
        {
            Position = Square.None;
            Piece = PieceType.None;

            _dispatcherQueue = App.Current.Services.GetService<IDispatcherQueueWrapper>();
            _styleManager = App.Current.Services.GetService<IStyleManager>();

            _styleManager.PropertyChanged += OnThemeManagerPropertyChanged;

            _pieceStyle = _styleManager.CurrentPieceStyle;
        }


        public BoardSquare(Square position, PieceType piece)
        {
            Position = position;
            Piece = piece;

            _dispatcherQueue = App.Current.Services.GetService<IDispatcherQueueWrapper>();
            _styleManager = App.Current.Services.GetService<IStyleManager>();

            _styleManager.PropertyChanged += OnThemeManagerPropertyChanged;

            _pieceStyle = _styleManager.CurrentPieceStyle;
        }


        private void OnThemeManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StyleManager.CurrentPieceStyle))
            {
                UpdatePieceTheme(_styleManager.CurrentPieceStyle);
            }
        }


        public void UpdatePieceTheme(PieceStyle pieceTheme)
        {
            _pieceStyle = pieceTheme;
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
                }
            }
        }


        // Display coordinates for UI grid positioning
        public int DisplayX => Position.File();
        public int DisplayY => Position.DisplayRank();


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
                if (Piece == PieceType.None)
                    return null;

                _images = new ImageServices();

                return _images.GetPieceImage(_pieceStyle, Piece);
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
