using Chess.UI.Services;
using Chess.UI.Settings;
using Chess.UI.Styles;
using Chess.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Views
{
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly INavigationService _navigationService;
        private readonly IImageService _imageService;
        private readonly IStyleManager _styleManager;

        private readonly StylesPreferencesViewModel _stylesViewModel;
        private readonly AudioPreferencesViewModel _audioViewModel;
        private readonly MultiplayerPreferencesViewModel _multiplayerPrefsViewModel;

        private ImageSource _previewBoardImage;


        public SettingsPage()
        {
            this.InitializeComponent();

            _navigationService = App.Current.Services.GetService<INavigationService>();
            _imageService = App.Current.Services.GetService<IImageService>();
            _styleManager = App.Current.Services.GetService<IStyleManager>();

            _stylesViewModel = App.Current.Services.GetService<StylesPreferencesViewModel>();
            _audioViewModel = App.Current.Services.GetService<AudioPreferencesViewModel>();
            _multiplayerPrefsViewModel = App.Current.Services.GetService<MultiplayerPreferencesViewModel>();

            // Subscribe to style changes for live preview
            _stylesViewModel.ItemSelected += RefreshPreview;
            _stylesViewModel.PropertyChanged += OnStylePropertyChanged;

            // Initial preview render
            RefreshPreview();
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Unsubscribe to prevent leaks
            _stylesViewModel.ItemSelected -= RefreshPreview;
            _stylesViewModel.PropertyChanged -= OnStylePropertyChanged;

            _navigationService.GoBack();
        }


        private void OnStylePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_stylesViewModel.SelectedBoardTheme) ||
                e.PropertyName == nameof(_stylesViewModel.SelectedPieceTheme))
            {
                RefreshPreview();
            }
        }


        /// <summary>
        /// Refreshes the live board preview with the currently selected board and piece styles.
        /// </summary>
        private void RefreshPreview()
        {
            if (_imageService == null || _styleManager == null) return;

            // Update board background
            _previewBoardImage = _imageService.GetImage(_styleManager.CurrentBoardStyle);
            OnPropertyChanged(nameof(_previewBoardImage));

            // Update the ImageBrush directly since x:Bind to a private field won't auto-refresh
            if (PreviewBoardBrush != null)
            {
                PreviewBoardBrush.ImageSource = _previewBoardImage;
            }

            var style = _styleManager.CurrentPieceStyle;

            // Update piece images
            SetPieceImage(PreviewBRook, style, PieceType.BRook);
            SetPieceImage(PreviewBKnight, style, PieceType.BKnight);
            SetPieceImage(PreviewBBishop, style, PieceType.BBishop);
            SetPieceImage(PreviewBQueen, style, PieceType.BQueen);

            SetPieceImage(PreviewBPawn0, style, PieceType.BPawn);
            SetPieceImage(PreviewBPawn1, style, PieceType.BPawn);
            SetPieceImage(PreviewBPawn2, style, PieceType.BPawn);
            SetPieceImage(PreviewBPawn3, style, PieceType.BPawn);

            SetPieceImage(PreviewWPawn0, style, PieceType.WPawn);
            SetPieceImage(PreviewWPawn1, style, PieceType.WPawn);
            SetPieceImage(PreviewWPawn2, style, PieceType.WPawn);
            SetPieceImage(PreviewWPawn3, style, PieceType.WPawn);

            SetPieceImage(PreviewWRook, style, PieceType.WRook);
            SetPieceImage(PreviewWKnight, style, PieceType.WKnight);
            SetPieceImage(PreviewWBishop, style, PieceType.WBishop);
            SetPieceImage(PreviewWQueen, style, PieceType.WQueen);
        }


        private void SetPieceImage(Image imageControl, PieceStyle style, PieceType pieceType)
        {
            if (imageControl != null)
            {
                imageControl.Source = _imageService.GetPieceImage(style, pieceType);
            }
        }


        #region Audio Slider Event Handlers

        private void Master_Volume_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Slider slider && _audioViewModel != null)
            {
                _audioViewModel.MasterVolume = (int)slider.Value;
            }
        }


        private void SFX_Volume_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Slider slider && _audioViewModel != null)
            {
                _audioViewModel.SfxVolume = (int)slider.Value;
            }
        }


        private void Atmos_Volume_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Slider slider && _audioViewModel != null)
            {
                _audioViewModel.AtmosVolume = (int)slider.Value;
            }
        }

        #endregion


        private void NetworkAdapterChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ignore first initialization
            if (e.RemovedItems.Count == 0 || e.AddedItems.Count == 0) return;
        }


        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
