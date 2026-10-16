using Chess.UI.Settings;
using Chess.UI.Styles;
using Chess.UI.Test.Helpers;
using Moq;


namespace Chess.UI.Test
{
    public class StyleManagerTests
    {
        private readonly Mock<ISettingsService> _settings = new();


        private StyleManager CreateStyleManager(string boardStyle, string pieceStyle)
        {
            _settings.SetupProperty(settings => settings.BoardStyle, boardStyle);
            _settings.SetupProperty(settings => settings.ChessPieceStyle, pieceStyle);

            return new StyleManager(new InlineDispatcherQueue(), _settings.Object);
        }


        [Fact]
        public void Styles_AreLoadedFromTheSettings()
        {
            StyleManager styleManager = CreateStyleManager("Glass", "Standard");

            Assert.Equal(BoardStyle.Glass, styleManager.CurrentBoardStyle);
            Assert.Equal(PieceStyle.Standard, styleManager.CurrentPieceStyle);
        }


        [Fact]
        public void UnknownStyles_FallBackToTheDefaults()
        {
            StyleManager styleManager = CreateStyleManager("Granite", "");

            Assert.Equal(BoardStyle.Wood, styleManager.CurrentBoardStyle);
            Assert.Equal(PieceStyle.Standard, styleManager.CurrentPieceStyle);
        }


        [Fact]
        public void LoadingStyles_DoesNotWriteTheSettings()
        {
            CreateStyleManager("Glass", "Basic");

            _settings.VerifySet(settings => settings.BoardStyle = It.IsAny<string>(), Times.Never);
            _settings.VerifySet(settings => settings.ChessPieceStyle = It.IsAny<string>(), Times.Never);
        }


        [Fact]
        public void ChangingTheBoardStyle_IsReportedAndSaved()
        {
            StyleManager styleManager = CreateStyleManager("Wood", "Basic");
            List<string?> changedProperties = TestData.RecordPropertyChanges(styleManager);

            styleManager.CurrentBoardStyle = BoardStyle.Marble;

            Assert.Equal(new[] { nameof(IStyleManager.CurrentBoardStyle) }, changedProperties);
            Assert.Equal("Marble", _settings.Object.BoardStyle);
            Assert.Equal("Basic", _settings.Object.ChessPieceStyle);
        }


        [Fact]
        public void ChangingThePieceStyle_IsReportedAndSaved()
        {
            StyleManager styleManager = CreateStyleManager("Wood", "Basic");
            List<string?> changedProperties = TestData.RecordPropertyChanges(styleManager);

            styleManager.CurrentPieceStyle = PieceStyle.Standard;

            Assert.Equal(new[] { nameof(IStyleManager.CurrentPieceStyle) }, changedProperties);
            Assert.Equal("Standard", _settings.Object.ChessPieceStyle);
            Assert.Equal("Wood", _settings.Object.BoardStyle);
        }


        [Fact]
        public void SettingTheSameStyle_ChangesNothing()
        {
            StyleManager styleManager = CreateStyleManager("Wood", "Basic");
            List<string?> changedProperties = TestData.RecordPropertyChanges(styleManager);

            styleManager.CurrentBoardStyle = BoardStyle.Wood;
            styleManager.CurrentPieceStyle = PieceStyle.Basic;

            Assert.Empty(changedProperties);
            _settings.VerifySet(settings => settings.BoardStyle = It.IsAny<string>(), Times.Never);
        }
    }
}
