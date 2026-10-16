using Chess.UI.Settings;
using Chess.UI.UI;
using Microsoft.UI.Xaml;
using static Chess.UI.Services.EngineAPI;


namespace Chess.UI.Test
{
    public class ConverterTests
    {
        [Theory]
        [InlineData(Side.White, "white", 3)]
        [InlineData(Side.White, "White", 3)]
        [InlineData(Side.White, "black", 0)]
        [InlineData(Side.Black, "black", 3)]
        [InlineData(Side.None, "white", 0)]
        [InlineData(Side.White, "green", 0)]
        public void SideConverter_MarksTheSelectedSide(Side side, string buttonSide, double expectedThickness)
        {
            object result = new SideToBorderThicknessConverter().Convert(side, typeof(Thickness), buttonSide, "");

            Assert.Equal(new Thickness(expectedThickness), result);
        }


        [Theory]
        [InlineData(CPUDifficulty.Easy, "easy", 3)]
        [InlineData(CPUDifficulty.Medium, "medium", 3)]
        [InlineData(CPUDifficulty.Hard, "Hard", 3)]
        [InlineData(CPUDifficulty.Hard, "easy", 0)]
        [InlineData(CPUDifficulty.None, "easy", 0)]
        public void DifficultyConverter_MarksTheSelectedDifficulty(CPUDifficulty difficulty, string buttonDifficulty, double expectedThickness)
        {
            object result = new CPUDifficultyToBorderThicknessConverter().Convert(difficulty, typeof(Thickness), buttonDifficulty, "");

            Assert.Equal(new Thickness(expectedThickness), result);
        }


        [Fact]
        public void BorderConverters_WithUnexpectedInput_DrawNoBorder()
        {
            Assert.Equal(new Thickness(0), new SideToBorderThicknessConverter().Convert("White", typeof(Thickness), "white", ""));
            Assert.Equal(new Thickness(0), new SideToBorderThicknessConverter().Convert(Side.White, typeof(Thickness), null, ""));
            Assert.Equal(new Thickness(0), new CPUDifficultyToBorderThicknessConverter().Convert(null, typeof(Thickness), "easy", ""));
        }


        [Fact]
        public void BorderConverters_DoNotConvertBack()
        {
            Assert.Throws<NotSupportedException>(() => new SideToBorderThicknessConverter().ConvertBack(new Thickness(3), typeof(Side), "white", ""));
            Assert.Throws<NotSupportedException>(() => new CPUDifficultyToBorderThicknessConverter().ConvertBack(new Thickness(3), typeof(CPUDifficulty), "easy", ""));
        }


        [Fact]
        public void NetworkAdapterConverter_ShowsAnIconForAdaptersOnly()
        {
            var converter = new NetworkAdapterToIconConverter();

            Assert.Equal("", converter.Convert(new NetworkAdapter { ID = 1, NetworkName = "Home" }, typeof(string), null, ""));
            Assert.Equal("", converter.Convert("Not recommended:", typeof(string), null, ""));
        }


        [Fact]
        public void BoolToVisibility_ShowsOnTrue()
        {
            Assert.Equal(Visibility.Visible, Converter.BoolToVisibility(true));
            Assert.Equal(Visibility.Collapsed, Converter.BoolToVisibility(false));
        }


        [Fact]
        public void BoolToCollapsed_HidesOnTrue()
        {
            Assert.Equal(Visibility.Collapsed, Converter.BoolToCollapsed(true));
            Assert.Equal(Visibility.Visible, Converter.BoolToCollapsed(false));
        }


        [Theory]
        [InlineData(Side.White, 1, 0)]
        [InlineData(Side.Black, 0, 1)]
        [InlineData(Side.None, 0, 0)]
        public void PlayerBorders_MarkTheChosenColor(Side player, double whiteBorder, double blackBorder)
        {
            Assert.Equal(new Thickness(whiteBorder), Converter.PlayerToWhiteBorderThickness(player));
            Assert.Equal(new Thickness(blackBorder), Converter.PlayerToBlackBorderThickness(player));
        }


        [Theory]
        [InlineData(CPUDifficulty.Easy, 1, 0, 0)]
        [InlineData(CPUDifficulty.Medium, 0, 1, 0)]
        [InlineData(CPUDifficulty.Hard, 0, 0, 1)]
        [InlineData(CPUDifficulty.None, 0, 0, 0)]
        public void DifficultyBorders_MarkTheChosenDifficulty(CPUDifficulty difficulty, double easy, double medium, double hard)
        {
            Assert.Equal(new Thickness(easy), Converter.CpuDifficultyToEasyBorderThickness(difficulty));
            Assert.Equal(new Thickness(medium), Converter.CpuDifficultyToMediumBorderThickness(difficulty));
            Assert.Equal(new Thickness(hard), Converter.CpuDifficultyToHardBorderThickness(difficulty));
        }


        [Fact]
        public void ComboNodes_SeparateItemsFromPlaceholders()
        {
            var adapter = new NetworkAdapter { ID = 1, NetworkName = "Home" };
            var item = new NetworkAdapterComboItem(adapter);
            var header = new ComboHeader("Not recommended:");
            var divider = new ComboDivider();

            Assert.Equal((ComboNodeKind.Item, false), (item.Kind, item.IsPlaceHolderItem));
            Assert.Same(adapter, item.NetworkAdapter);
            Assert.Equal((ComboNodeKind.Header, true), (header.Kind, header.IsPlaceHolderItem));
            Assert.Equal("Not recommended:", header.HeadingText);
            Assert.Equal((ComboNodeKind.Divider, true), (divider.Kind, divider.IsPlaceHolderItem));
        }
    }
}
