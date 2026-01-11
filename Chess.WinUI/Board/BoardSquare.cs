using Chess.UI.Images;
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
        PositionInstance pos { get; set; }
        PieceTypeInstance piece { get; set; }
        PlayerColor colour { get; set; }
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
            pos = new PositionInstance(0, 0);
            piece = PieceTypeInstance.DefaultType;
            colour = PlayerColor.NoColor;

            _dispatcherQueue = App.Current.Services.GetService<IDispatcherQueueWrapper>();
            _styleManager = App.Current.Services.GetService<IStyleManager>();

            _styleManager.PropertyChanged += OnThemeManagerPropertyChanged;

            _pieceStyle = _styleManager.CurrentPieceStyle;
        }


        public BoardSquare(int x, int y, PieceTypeInstance pieceTypeInstance, PlayerColor color)
        {
            pos = new PositionInstance(x, y);
            piece = pieceTypeInstance;
            colour = color;

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


        private PositionInstance _pos;
        public PositionInstance pos
        {
            get => _pos;
            set
            {
                if (_pos.x != value.x || _pos.y != value.y)
                {
                    _pos = value;
                    OnPropertyChanged();
                }
            }
        }


        private PieceTypeInstance _piece;
        public PieceTypeInstance piece
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


        private PlayerColor _colour;
        public PlayerColor colour
        {
            get => _colour;
            set
            {
                if (_colour != value)
                {
                    _colour = value;
                    OnPropertyChanged();
                }
            }
        }


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
                if (piece == PieceTypeInstance.DefaultType || colour == PlayerColor.NoColor)
                    return null;

                _images = new ImageServices();

                return _images.GetPieceImage(_pieceStyle, colour, piece);
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
