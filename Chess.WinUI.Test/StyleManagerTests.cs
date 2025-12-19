using Chess.UI.Styles;
using System.ComponentModel;


namespace Chess.UI.Test
{
    public partial class StyleManagerTests
    {
        [Fact]
        public void CurrentBoardStyle_PropertyChanged_PropertyRaised()
        {
            // Arrange
            var mockStyleManager = new MockStyleManager();
            bool eventRaised = false;
            string? propertyNameChanged = null;

            mockStyleManager.PropertyChanged += (sender, e) =>
            {
                eventRaised = true;
                propertyNameChanged = e.PropertyName;
            };

            // Act
            mockStyleManager.CurrentBoardStyle = BoardStyle.Glass;

            // Assert
            Assert.True(eventRaised);
            Assert.Equal(nameof(IStyleManager.CurrentBoardStyle), propertyNameChanged);
        }

        [Fact]
        public void CurrentPieceStyle_PropertyChanged_PropertyRaised()
        {
            // Arrange
            var mockStyleManager = new MockStyleManager();
            bool eventRaised = false;
            string? propertyNameChanged = null;

            mockStyleManager.PropertyChanged += (sender, e) =>
            {
                eventRaised = true;
                propertyNameChanged = e.PropertyName;
            };

            // Act
            mockStyleManager.CurrentPieceStyle = PieceStyle.Standard;

            // Assert
            Assert.True(eventRaised);
            Assert.Equal(nameof(IStyleManager.CurrentPieceStyle), propertyNameChanged);
        }


        // Mock implementation for testing
        private partial class MockStyleManager : IStyleManager
        {
            private BoardStyle _boardStyle;
            private PieceStyle _pieceStyle;

            public event PropertyChangedEventHandler? PropertyChanged;

            public BoardStyle CurrentBoardStyle
            {
                get => _boardStyle;
                set
                {
                    _boardStyle = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBoardStyle)));
                }
            }

            public PieceStyle CurrentPieceStyle
            {
                get => _pieceStyle;
                set
                {
                    _pieceStyle = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPieceStyle)));
                }
            }

            public void LoadStyles() { }
            public void SaveStyles() { }
        }
    }
}