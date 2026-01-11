using Chess.UI.Images;
using Chess.UI.Multiplayer;
using Chess.UI.Styles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.UI
{
    public partial class NetworkAdapterToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is NetworkAdapter adapter)
            {
                string iconglyph = adapter.Type switch
                {
                    AdapterType.Ethernet => "\uE839",
                    AdapterType.WiFi => "\uE701",
                    _ => "\uE701",
                };

                return iconglyph;
            }
            return "";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
    }


    public partial class PieceStylePreviewConverter : IValueConverter
    {
        private static readonly Dictionary<PieceStyle, ImageSource> _cache = new();
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PieceStyle style)
            {
                if (_cache.TryGetValue(style, out var cached))
                    return cached;

                var imgService = App.Current.Services.GetService<IImageService>();
                // Using a white pawn as representative icon
                var img = imgService?.GetPieceImage(style, PieceType.WPawn);

                if (img != null)
                    _cache[style] = img;

                return img;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
    }


    public partial class BoardStylePreviewConverter : IValueConverter
    {
        private static readonly Dictionary<BoardStyle, ImageSource> _cache = new();
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is BoardStyle style)
            {
                if (_cache.TryGetValue(style, out var cached))
                    return cached;

                var imgService = App.Current.Services.GetService<IImageService>();
                var img = imgService?.GetImage(style);

                if (img != null)
                    _cache[style] = img;

                return img;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotSupportedException();
    }


    public class Converter
    {
        public static ImageSource PlayerToPawnImage(Side player)
        {
            IImageService imageService = App.Current.Services.GetService<IImageService>();
            if (player == Side.White)
                return imageService.GetCapturedPieceImage(PieceType.WPawn);
            else if (player == Side.Black)
                return imageService.GetCapturedPieceImage(PieceType.BPawn);
            else
                return imageService.GetCapturedPieceImage(PieceType.WPawn); // falback
        }

        public static Visibility BoolToVisibility(bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility BoolToCollapsed(bool value)
        {
            return value ? Visibility.Collapsed : Visibility.Visible;
        }

        public static SolidColorBrush BoolToColor(bool value)
        {
            return value ? new SolidColorBrush(Microsoft.UI.Colors.Green) : new SolidColorBrush(Microsoft.UI.Colors.Red);
        }

        public static Thickness PlayerToWhiteBorderThickness(Side player)
        {
            int value = player == Side.White ? 1 : 0;

            Thickness thicknessValue = new Thickness(value);
            return thicknessValue;
        }

        public static Thickness PlayerToBlackBorderThickness(Side player)
        {
            int value = player == Side.Black ? 1 : 0;

            Thickness thicknessValue = new Thickness(value);
            return thicknessValue;
        }

        public static Thickness CpuDifficultyToEasyBorderThickness(CPUDifficulty difficulty)
        {
            int value = difficulty == CPUDifficulty.Easy ? 1 : 0;

            Thickness thicknessValue = new Thickness(value);
            return thicknessValue;
        }

        public static Thickness CpuDifficultyToMediumBorderThickness(CPUDifficulty difficulty)
        {
            int value = difficulty == CPUDifficulty.Medium ? 1 : 0;

            Thickness thicknessValue = new Thickness(value);
            return thicknessValue;
        }

        public static Thickness CpuDifficultyToHardBorderThickness(CPUDifficulty difficulty)
        {
            int value = difficulty == CPUDifficulty.Hard ? 1 : 0;

            Thickness thicknessValue = new Thickness(value);
            return thicknessValue;
        }
    }


}
